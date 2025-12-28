WORKSPACE CONTEXT DECLARATION
1. Workspace Identity

Workspace Name:

Date / Time Initialized:

Primary Objective:

Active Phase: (e.g. CMS Migration – Weekly Distribution)

2. Execution Context
2.1 Execution Locus (Required)

Declare where executable behavior is exercised in this workspace.

Executable Entry Point(s):

File:

Class:

Method / Runner / Host:

Non-Executable Components (Assumed):

(Explicit list or “None”)

No component may be assumed executable unless listed here.

3. Observability Strategy

Declare how correctness will be assessed during this workspace.

Primary Observability Mode:
☐ Inspection-only
☐ Temporary instrumentation
☐ Parity harness
☐ Mixed (declare per step)

Constraints / Notes:

4. Temporary Instrumentation Policy

Instrumentation Allowed: ☐ Yes ☐ No

Permitted Scope:

Mandatory Removal Phase / Step:

Any instrumentation not declared here is prohibited.

5. Legacy Parity Anchor

Declare the authoritative comparison baseline.

Legacy Strategy / Path:

Dataset / Scenario:

Snapshot / Hash / Output Reference:

This anchor is treated as authoritative for parity unless superseded explicitly.

6. Test & Verification Posture

Testing Enabled in This Workspace:
☐ Yes ☐ Deferred ☐ Parity-only

Test Project(s) in Scope:

7. Termination Semantics

Declare upfront how this workspace may end.

Permitted Termination Modes:
☐ Clean closure
☐ Pause (resumable)
☐ Terminate without closure (unsynced)

Default on Failure:

8. Signal & Interaction Contract

Preferred Interaction Mode:
☐ Procedural
☐ Minimal / execute-mode
☐ Exploratory

Verbosity Attenuation Expected: ☐ Yes

9. Acknowledgement

This declaration instantiates contextual commitments (Set C) for this workspace only.
It does not modify or override foundational documents.

Declared by:

Acknowledged by:

10. Status

Workspace State: ☐ ACTIVE ☐ PAUSED ☐ UNSYNCED ☐ TERMINATED

Last Updated:

Disposal Notice

This document may be discarded once the workspace is formally closed or terminated.