using Xunit;

// WPF/XAML-loading tests are sensitive to concurrency and shared WPF singletons.
// Disable xUnit parallelization to keep UI tests deterministic.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

