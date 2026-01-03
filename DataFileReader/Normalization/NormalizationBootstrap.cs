namespace DataFileReader.Normalization;

/// <summary>
///     Provides a canonical way to construct a normalization pipeline.
///     Not used unless explicitly invoked.
/// </summary>
public static class NormalizationBootstrap
{
    public static INormalizationPipeline CreateDefaultPipeline()
    {
        return new DefaultNormalizationPipeline(new List<INormalizationStage>(), new NormalizationContext(new Dictionary<string, string>()));
    }
}