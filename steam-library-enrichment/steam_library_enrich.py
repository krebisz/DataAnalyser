#!/usr/bin/env python3
"""Enrich a Steam owned-games snapshot using local cached public metadata.

Input:  steam-library-owned.json (Steam MCP export; list or {games:[...]})
Output: steam-library-enriched.json and steam-library-analysis.json

This uses only the Python standard library. It never reads or writes Steam
account data, credentials, or the Steam client. By default it makes no network
requests. Optional public metadata fetches require explicit AppIDs and are
deliberately capped, avoiding library-wide metadata scraping.
"""
from __future__ import annotations

import argparse
import json
import time
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen

# These deliberate delays keep the public services load-friendly.  Responses
# are cached per AppID, so a completed run makes no additional requests.
STORE_DELAY_SECONDS = 0.35
STEAMSPY_DELAY_SECONDS = 0.45
USER_AGENT = "steam-library-local-enrichment/1.0 (+local personal analysis)"


def load_json(path: Path):
    with path.open("r", encoding="utf-8") as fh:
        return json.load(fh)


def save_json(path: Path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    temp = path.with_suffix(path.suffix + ".tmp")
    with temp.open("w", encoding="utf-8") as fh:
        json.dump(value, fh, ensure_ascii=False, indent=2)
        fh.write("\n")
    temp.replace(path)


def fetch_json(url: str, timeout: int = 45):
    request = Request(url, headers={"User-Agent": USER_AGENT, "Accept": "application/json"})
    with urlopen(request, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def cache_read(path: Path):
    try:
        return load_json(path)
    except (OSError, json.JSONDecodeError):
        return None


def fetch_store(appids: list[int], cache_dir: Path, retry_failed: bool = False):
    result = {}
    for position, appid in enumerate(appids):
        path = cache_dir / "store" / f"{appid}.json"
        value = cache_read(path)
        # Failed responses are cached too: do not repeatedly hammer an endpoint
        # during a resume. Delete the specific cache file to retry that AppID.
        if value is not None and (value.get("success") or not retry_failed):
            result[appid] = value
            continue
        try:
            url = "https://store.steampowered.com/api/appdetails?" + urlencode({"appids": appid, "l": "en"})
            value = fetch_json(url).get(str(appid), {"success": False, "error": "missing appdetails response"})
        except (HTTPError, URLError, TimeoutError, json.JSONDecodeError) as exc:
            value = {"success": False, "error": f"Store request failed: {exc}"}
        save_json(path, value)
        result[appid] = value
        if position + 1 < len(appids):
            time.sleep(STORE_DELAY_SECONDS)
    return result


def fetch_steamspy(appids: list[int], cache_dir: Path, retry_failed: bool = False):
    result = {}
    for position, appid in enumerate(appids):
        path = cache_dir / "steamspy" / f"{appid}.json"
        value = cache_read(path)
        if value is None or (retry_failed and value.get("error")):
            try:
                value = fetch_json("https://steamspy.com/api.php?" + urlencode({"request": "appdetails", "appid": appid}))
            except (HTTPError, URLError, TimeoutError, json.JSONDecodeError) as exc:
                value = {"error": f"SteamSpy request failed: {exc}"}
            save_json(path, value)
        result[str(appid)] = value
        if position + 1 < len(appids):
            time.sleep(STEAMSPY_DELAY_SECONDS)
    return result


def normalize_owned(payload):
    games = payload.get("games", []) if isinstance(payload, dict) else payload
    result = []
    for game in games:
        appid = int(game.get("appid"))
        def minutes(key):
            value = game.get(key, 0)
            return int(value) if isinstance(value, (int, float)) else 0
        result.append({
            "appid": appid,
            "name": game.get("name") or f"Unknown ({appid})",
            "playtime_forever_minutes": minutes("playtime_forever_minutes") or minutes("playtime_forever"),
            "playtime_2weeks_minutes": minutes("playtime_2weeks_minutes") or minutes("playtime_2weeks"),
            "installed_status": game.get("installed_status"),
        })
    return sorted({game["appid"]: game for game in result}.values(), key=lambda game: game["appid"])


def review_summary(score):
    if score is None:
        return None
    if score >= 95: return "Overwhelmingly Positive"
    if score >= 80: return "Very Positive"
    if score >= 70: return "Mostly Positive"
    if score >= 40: return "Mixed"
    if score >= 20: return "Mostly Negative"
    return "Overwhelmingly Negative"


def enrich(owned, store, spy):
    rows = []
    for game in owned:
        appid = game["appid"]
        store_entry = store.get(appid, {})
        data = store_entry.get("data", {}) if store_entry.get("success") else {}
        spy_entry = spy.get(str(appid), {})
        positive, negative = spy_entry.get("positive"), spy_entry.get("negative")
        score = None
        if isinstance(positive, int) and isinstance(negative, int) and positive + negative:
            score = round(positive * 100 / (positive + negative), 1)
        genres = [item["description"] for item in data.get("genres", []) if item.get("description")]
        categories = [item["description"] for item in data.get("categories", []) if item.get("description")]
        tags = sorted((spy_entry.get("tags") or {}).keys())
        sources = []
        errors = []
        if data:
            sources.append("steam_store_appdetails")
        elif store_entry.get("error"):
            errors.append(store_entry["error"])
        elif store_entry:
            errors.append("Steam Store metadata unavailable")
        if spy_entry:
            sources.append("steamspy_appdetails_bulk")
        elif spy_entry.get("error"):
            errors.append(spy_entry["error"])
        else:
            errors.append("SteamSpy metadata unavailable for appid")
        status = "success" if sources and not errors else ("partial" if sources else "failed")
        rows.append({
            **game,
            "genres": genres,
            "categories": categories,
            "tags": tags,
            "review_summary": review_summary(score),
            "review_score": score,
            "release_date": (data.get("release_date") or {}).get("date"),
            "developers": data.get("developers", []),
            "publishers": data.get("publishers", []),
            "is_free": data.get("is_free"),
            "metadata_source": sources,
            "metadata_fetch_status": status,
            "metadata_fetch_error": "; ".join(errors) if errors else None,
        })
    return rows


def top_counts(rows, field, limit=50):
    counts = Counter(item for row in rows for item in row[field])
    return [{field[:-1] if field.endswith("s") else field: key, "games": count} for key, count in counts.most_common(limit)]


def analysis(rows):
    unplayed = [r for r in rows if r["playtime_forever_minutes"] == 0]
    top_played = sorted(rows, key=lambda r: r["playtime_forever_minutes"], reverse=True)[:50]
    recommendation = []
    for row in rows:
        if row["playtime_forever_minutes"]:
            continue
        recommendation.append({
            "appid": row["appid"], "name": row["name"], "genres": row["genres"], "tags": row["tags"][:10],
            "review_score": row["review_score"], "review_summary": row["review_summary"],
            "release_date": row["release_date"], "is_installed": bool(row["installed_status"]),
            "metadata_fetch_status": row["metadata_fetch_status"],
        })
    recommendation.sort(key=lambda r: (not r["is_installed"], -(r["review_score"] or -1), r["name"].lower()))
    return {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "library": {"owned_games": len(rows), "unplayed_games": len(unplayed), "metadata_success": sum(r["metadata_fetch_status"] == "success" for r in rows), "metadata_partial_or_failed": sum(r["metadata_fetch_status"] != "success" for r in rows)},
        "genre_breakdown": top_counts(rows, "genres"),
        "tag_breakdown": top_counts(rows, "tags"),
        "unplayed_games": {"count": len(unplayed), "genre_breakdown": top_counts(unplayed, "genres"), "tag_breakdown": top_counts(unplayed, "tags")},
        "top_played_games": {"games": [{"appid": r["appid"], "name": r["name"], "playtime_forever_minutes": r["playtime_forever_minutes"], "genres": r["genres"], "tags": r["tags"][:10]} for r in top_played], "genre_breakdown": top_counts(top_played, "genres"), "tag_breakdown": top_counts(top_played, "tags")},
        "recommendation_ready_unplayed_games": recommendation,
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", type=Path, default=Path("steam-library-owned.json"))
    parser.add_argument("--output", type=Path, default=Path("steam-library-enriched.json"))
    parser.add_argument("--analysis-output", type=Path, default=Path("steam-library-analysis.json"))
    parser.add_argument("--cache-dir", type=Path, default=Path(".steam-library-cache"))
    parser.add_argument("--offline", action="store_true", help="Compatibility flag; offline behavior is now the default.")
    parser.add_argument("--fetch-appid", type=int, action="append", default=[], metavar="APPID", help="Explicitly fetch public Store and SteamSpy metadata for one selected game. Repeat up to 20 times.")
    args = parser.parse_args()
    owned = normalize_owned(load_json(args.input))
    appids = [row["appid"] for row in owned]
    requested = list(dict.fromkeys(args.fetch_appid))
    if len(requested) > 20:
        parser.error("--fetch-appid is capped at 20 games per run; select a smaller shortlist.")
    unknown = sorted(set(requested) - set(appids))
    if unknown:
        parser.error(f"Requested AppIDs are not in the owned-games input: {unknown}")
    # Read existing cache for the whole library.  Only explicit AppIDs may
    # access the network, which keeps the workflow bounded and predictable.
    store = {appid: cache_read(args.cache_dir / "store" / f"{appid}.json") or {"success": False, "error": "No cached Steam Store metadata"} for appid in appids}
    spy = {str(appid): cache_read(args.cache_dir / "steamspy" / f"{appid}.json") or {"error": "No cached SteamSpy metadata"} for appid in appids}
    if requested and not args.offline:
        store.update(fetch_store(requested, args.cache_dir, retry_failed=True))
        spy.update(fetch_steamspy(requested, args.cache_dir, retry_failed=True))
    rows = enrich(owned, store, spy)
    save_json(args.output, rows)
    save_json(args.analysis_output, analysis(rows))
    print(f"Wrote {len(rows)} games to {args.output}")
    print(f"Wrote breakdowns and recommendation fields to {args.analysis_output}")


if __name__ == "__main__":
    main()
