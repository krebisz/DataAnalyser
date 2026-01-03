using DataFileReader.Ingestion;
using DataFileReader.Normalization.Canonical;
using DataFileReader.Normalization.Stages;

namespace DataFileReader.Normalization;

/// <summary>
///     Normalization pipeline implementation.
///     Executes configured stages and produces canonical metric series.
/// </summary>
public sealed class DefaultNormalizationPipeline : INormalizationPipeline
{
    private readonly CmsProductionStage                 _cmsProductionStage;
    private readonly NormalizationContext               _context;
    private readonly IReadOnlyList<INormalizationStage> _stages;

    public DefaultNormalizationPipeline(IReadOnlyList<INormalizationStage> stages, NormalizationContext context)
    {
        _stages = stages ?? throw new ArgumentNullException(nameof(stages));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cmsProductionStage = new CmsProductionStage();
    }

    public IReadOnlyList<CanonicalMetricSeries<object>> Normalize(IReadOnlyCollection<RawRecord> rawRecords)
    {
        if (rawRecords == null)
            throw new ArgumentNullException(nameof(rawRecords));

        var current = rawRecords;

        // Execute all configured stages
        foreach (var stage in _stages)
            current = stage.Process(current, _context);

        // Run CMS production stage to convert processed RawRecords to CMS
        _cmsProductionStage.Process(current, _context);

        // Return the produced CMS
        return _cmsProductionStage.ProducedCms;
    }
}