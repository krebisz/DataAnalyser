using System.Windows.Controls;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.OperationChain;

public partial class OperationChainWorkbenchView : UserControl
{
    public OperationChainWorkbenchView()
    {
        InitializeComponent();
    }

    public void DisplayResult(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var presentation = OperationChainWorkbenchPresenter.Build(result);
        SummaryText.Text = presentation.Summary;
        TraceItems.ItemsSource = presentation.TraceRows;
        DatasetGrid.ItemsSource = presentation.DatasetRows;
        EvidenceText.Text = presentation.Evidence;
    }
}
