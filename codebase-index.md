# Codebase Index

_Auto-generated. Do not edit by hand._

## Projects
- C:\Development\POCs\DataFileReaderRedux\DataFileReader\DataFileReadercsproj
- C:\Development\POCs\DataFileReaderRedux\DataVisualiser\DataVisualisercsproj

## C# Source Files
### C:\Development\POCs\DataFileReaderRedux\DataFileReader\App\HealthDataAppcs
**Namespace:** DataFileReader.App;
**Classes:**
- public class HealthDataApp

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\CanonicalMetricIdentityResolvercs
**Namespace:** DataFileReader.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\CanonicalMetricMappingcs
**Namespace:** DataFileReader.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\CanonicalMetricSeriescs
**Namespace:** DataFileReader.Canonical
**Interfaces:**
- ICanonicalMetricSeries
**Enums:**
- TimeRepresentation
- MetricDimension
- DataCompleteness
- ValidationStatus

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\CmsTypeConvertercs
**Namespace:** DataFileReader.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\HealthMetricToCmsMappercs
**Namespace:** DataFileReader.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\MetricCompatibilityHelpercs
**Namespace:** DataFileReader.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Canonical\MetricIdentityResolutionResultcs
**Namespace:** DataFileReader.Canonical
**Enums:**
- IdentityResolutionFailureReason

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\JSON\IJsoncs
**Interfaces:**
- IJson
- IJsonPrimitive
- IJsonComplex

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\JSON\JsonArraycs
**Classes:**
- public class JsonArray : IJsonComplex, IEnumerable<IJson>, IEnumerable

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\JSON\JsonObjectcs
**Classes:**
- public class JsonObject : IJsonComplex, IEnumerable<IJson>, IEnumerable

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\JSON\JsonValuecs
**Classes:**
- public class JsonValue : IJsonPrimitive, IConvertible
- public class Types

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\AggregationPeriodcs
**Namespace:** DataFileReader.Class
**Enums:**
- AggregationPeriod

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\HierarchyObjectcs
**Classes:**
- public class HierarchyObject

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\HierarchyObjectListcs
**Namespace:** DataFileReader.Class;
**Classes:**
- public class HierarchyObjectList

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\MetaDatacs
**Namespace:** DataFileReader.Class
**Classes:**
- public class MetaData

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\MetaDataComparercs
**Namespace:** DataFileReader.Class;
**Classes:**
- public class MetaDataComparer : IEqualityComparer<MetaData>

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Class\MetaDataListcs
**Namespace:** DataFileReader.Class
**Classes:**
- public class MetaDataList

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\ConsoleHelpercs
**Namespace:** DataFileReader.Helper;
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\DataHelpercs
**Namespace:** DataFileReader.Helper;
**Classes:**
- public class RowComparer : IEqualityComparer<object[]>

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\DataHelper_Oldcs
**Namespace:** DataFileReader.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\DataNormalizationcs
**Namespace:** DataFileReader.Helper
**Classes:**
- public class DataNormalization

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\FileHelpercs
**Namespace:** DataFileReader.Helper;
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\JsoonHelpercs
**Namespace:** DataFileReader.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\MetricTypeParsercs
**Namespace:** DataFileReader.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\SamsungHealthCsvParsercs
**Namespace:** DataFileReader.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\SamsungHealthParsercs
**Namespace:** DataFileReader.Helper
**Classes:**
- public class HealthMetric

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\SQLHelpercs
**Namespace:** DataFileReader.Helper;
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Helper\TimeNormalizationHelpercs
**Namespace:** DataFileReader.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Ingestion\RawRecordcs
**Namespace:** DataFileReader.Ingestion
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Ingestion\RawRecordFactorycs
**Namespace:** DataFileReader.Ingestion
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\Canonical\CanonicalMetricSeriescs
**Namespace:** DataFileReader.Normalization.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\Canonical\DimensionSetcs
**Namespace:** DataFileReader.Normalization.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\Canonical\MetricIdentitycs
**Namespace:** DataFileReader.Normalization.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\Canonical\TimeAxiscs
**Namespace:** DataFileReader.Normalization.Canonical
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\Stages\CmsProductionStagecs
**Namespace:** DataFileReader.Normalization.Stages
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\Stages\MetricIdentityResolutionStagecs
**Namespace:** DataFileReader.Normalization.Stages
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\DefaultNormalizationPipelinecs
**Namespace:** DataFileReader.Normalization
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\INormalizationPipelinecs
**Namespace:** DataFileReader.Normalization
**Interfaces:**
- INormalizationPipeline

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\INormalizationStagecs
**Namespace:** DataFileReader.Normalization
**Interfaces:**
- INormalizationStage

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\NormalizationBootstrapcs
**Namespace:** DataFileReader.Normalization
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\NormalizationContextcs
**Namespace:** DataFileReader.Normalization
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Normalization\NormalizationDiagnosticscs
**Namespace:** DataFileReader.Normalization
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\obj\Debug\net90\NETCoreApp,Version=v90AssemblyAttributescs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\obj\Debug\net90\DataFileReaderAssemblyInfocs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\obj\Debug\net90\DataFileReaderGlobalUsingsgcs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Parsers\IHealthFileParsercs
**Namespace:** DataFileReader.Parsers;
**Interfaces:**
- IHealthFileParser

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Parsers\LegacyJsonParsercs
**Classes:**
- public class LegacyJsonParser : IHealthFileParser

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Parsers\SamsungCsvParsercs
**Classes:**
- public class SamsungCsvParser : IHealthFileParser

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Parsers\SamsungJsonParsercs
**Classes:**
- public class SamsungJsonParser : IHealthFileParser

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Services\FileProcessingServicecs
**Namespace:** DataFileReader.Services;
**Tags:** Service
**Classes:**
- public class FileProcessingService

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Services\MetricAggregatorcs
**Namespace:** DataFileReader.Services;
**Classes:**
- public class MetricAggregator

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\Programcs
**Entry Points:**
- Main()

### C:\Development\POCs\DataFileReaderRedux\DataFileReader\ShadowValidate_SamsungHealthCsvcs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Computation\ChartComputationEnginecs
**Namespace:** DataVisualiser.Charts.Computation
**Tags:** Engine
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Computation\ChartComputationResultcs
**Namespace:** DataVisualiser.Charts.Computation
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Helpers\ChartHelpercs
**Namespace:** DataVisualiser.Charts.Helpers
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Helpers\ChartTooltipManagercs
**Namespace:** DataVisualiser.Charts.Helpers
**Classes:**
- public class ChartTooltipManager : IDisposable

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Helpers\WeeklyDistributionTooltipcs
**Namespace:** DataVisualiser.Charts.Helpers
**Classes:**
- public class WeeklyDistributionTooltip : IDisposable

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Rendering\ChartRenderEnginecs
**Namespace:** DataVisualiser.Charts.Rendering
**Tags:** Engine
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Rendering\ChartRenderModelcs
**Namespace:** DataVisualiser.Charts.Rendering
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Rendering\ChartSeriesModecs
**Namespace:** DataVisualiser.Charts.Rendering
**Enums:**
- ChartSeriesMode

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\CombinedMetricStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\DifferenceStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\MultiMetricStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\NormalizedStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\RatioStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\SingleMetricStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\WeekdayTrendStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\Strategies\WeeklyDistributionStrategycs
**Namespace:** DataVisualiser.Charts.Strategies
**Tags:** Strategy
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\ChartDataContextcs
**Namespace:** DataVisualiser.Charts
**Classes:**
- public class ChartDataContext

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\ColourPalettecs
**Namespace:** DataVisualiser.Charts
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Charts\IChartComputationStrategycs
**Namespace:** DataVisualiser.Charts
**Tags:** Strategy
**Interfaces:**
- IChartComputationStrategy

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Converters\NormalizationModeConvertercs
**Namespace:** DataVisualiser.Converters
**Classes:**
- public class NormalizationModeConverter : IValueConverter

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Data\Repositories\CmsDataServicecs
**Namespace:** DataVisualiser.Data.Repositories
**Tags:** Service
**Classes:**
- public class CmsDataService

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Data\Repositories\DataFetchercs
**Namespace:** DataVisualiser.Data.Repositories
**Classes:**
- public class DataFetcher

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Data\SqlQueryBuildercs
**Namespace:** DataVisualiser.Data
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Helper\CmsConversionHelpercs
**Namespace:** DataVisualiser.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Helper\FrequencyBinningHelpercs
**Namespace:** DataVisualiser.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Helper\MathHelpercs
**Namespace:** DataVisualiser.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Helper\StrategyComputationHelpercs
**Namespace:** DataVisualiser.Helper
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\BinaryOperatorRegistrycs
**Namespace:** DataVisualiser.Models
**Classes:**
- public class BinaryOperatorRegistry

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\BinaryOperatorscs
**Namespace:** DataVisualiser.Models
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\DateRangeResultcs
**Namespace:** DataVisualiser.Models
**Tags:** DataCarrier (heuristic)
**Classes:**
- public class DateRangeResult

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\HealthMetricDatacs
**Namespace:** DataVisualiser.Models
**Tags:** DataCarrier (heuristic)
**Classes:**
- public class HealthMetricData

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\NormalizationModecs
**Namespace:** DataVisualiser.Models
**Enums:**
- NormalizationMode

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\RecordToDayRatiocs
**Namespace:** DataVisualiser.Models
**Enums:**
- RecordToDayRatio

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\SmoothedDataPointcs
**Namespace:** DataVisualiser.Models
**Tags:** DataCarrier (heuristic)
**Classes:**
- public class SmoothedDataPoint

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\TickIntervalcs
**Namespace:** DataVisualiser.Models
**Enums:**
- TickInterval

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\WeekdayTrendPointcs
**Namespace:** DataVisualiser.Models
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\WeekdayTrendResultcs
**Namespace:** DataVisualiser.Models
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\WeekdayTrendSeriescs
**Namespace:** DataVisualiser.Models
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Models\WeeklyDistributionResultcs
**Namespace:** DataVisualiser.Models
**Classes:**
- public class WeeklyDistributionResult

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\NETCoreApp,Version=v90AssemblyAttributescs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\Appgcs
**Namespace:** DataVisualiser {
**Entry Points:**
- Main()

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\Appgics
**Namespace:** DataVisualiser {
**Entry Points:**
- Main()

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\DataVisualiserAssemblyInfocs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\DataVisualiserGlobalUsingsgcs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\MainWindowgcs
**Namespace:** DataVisualiser {
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\MainWindowgics
**Namespace:** DataVisualiser {
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\obj\Debug\net90-windows\unoreloadcookiegcs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\Shading\FrequencyBasedShadingStrategycs
**Namespace:** DataVisualiser.Services.Shading
**Tags:** Strategy
**Classes:**
- public class FrequencyBasedShadingStrategy : IIntervalShadingStrategy

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\Shading\IIntervalShadingStrategycs
**Namespace:** DataVisualiser.Services.Shading
**Tags:** Strategy
**Classes:**
- public class IntervalShadingContext
**Interfaces:**
- IIntervalShadingStrategy

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\ChartDataContextBuildercs
**Namespace:** DataVisualiser.Services
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\ChartUpdateCoordinatorcs
**Namespace:** DataVisualiser.Services
**Classes:**
- public class ChartUpdateCoordinator

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\MetricSelectionServicecs
**Namespace:** DataVisualiser.Services
**Tags:** Service
**Classes:**
- public class MetricSelectionService

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\WeeklyDistributionServicecs
**Namespace:** DataVisualiser.Services
**Tags:** Service
**Classes:**
- public class WeeklyDistributionService

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Services\WeeklyFrequencyRenderercs
**Namespace:** DataVisualiser.Services
**Classes:**
- public class WeeklyFrequencyRenderer

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\State\ChartStatecs
**Namespace:** DataVisualiser.State
**Classes:**
- public class ChartState

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\State\CmsConfigurationcs
**Namespace:** DataVisualiser.State
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\State\MetricStatecs
**Namespace:** DataVisualiser.State
**Classes:**
- public class MetricState

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\State\UiStatecs
**Namespace:** DataVisualiser.State
**Tags:** DataCarrier (heuristic)
**Classes:**
- public class UiState

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\UI\SubtypeSelectors\SubtypeSelectorManagercs
**Namespace:** DataVisualiser.UI.SubtypeSelectors
**Classes:**
- public class SubtypeSelectorManager
- public class SubtypeControlPair

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\ViewModels\Commands\RelayCommandcs
**Namespace:** DataVisualiser.ViewModels.Commands
**Classes:**
- public class RelayCommand : ICommand

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\ViewModels\Events\ViewModelEventscs
**Namespace:** DataVisualiser.ViewModels.Events
**Classes:**
- public class MetricTypesLoadedEventArgs : EventArgs
- public class SubtypesLoadedEventArgs : EventArgs
- public class DateRangeLoadedEventArgs : EventArgs
- public class DataLoadedEventArgs : EventArgs
- public class ChartVisibilityChangedEventArgs : EventArgs
- public class ErrorEventArgs : EventArgs
- public class ChartUpdateRequestedEventArgs : EventArgs

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\ViewModels\MainWindowViewModelcs
**Namespace:** DataVisualiser.ViewModels
**Tags:** ViewModel
**Classes:**
- public class MainWindowViewModel : INotifyPropertyChanged

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Appxamlcs
**Namespace:** DataVisualiser
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\AssemblyInfocs
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\MainWindowWeeklycs
**Namespace:** DataVisualiser
_No public symbols detected._

### C:\Development\POCs\DataFileReaderRedux\DataVisualiser\MainWindowxamlcs
**Namespace:** DataVisualiser
_No public symbols detected._

## XAML Files
- C:\Development\POCs\DataFileReaderRedux\DataVisualiser\Appxaml
- C:\Development\POCs\DataFileReaderRedux\DataVisualiser\MainWindowxaml
