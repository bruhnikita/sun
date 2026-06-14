using System.Windows;
using Exam.Core;

namespace Exam.App;

public partial class SalesHistoryWindow : Window
{
    public SalesHistoryWindow(string agentTitle, List<SaleHistoryItem> items)
    {
        InitializeComponent();
        DataContext = new { Title = $"История реализации: {agentTitle}", Items = items };
    }
}
