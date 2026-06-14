using System.IO;
using System.Text.Json;
using System.Windows;
using Exam.Core;
using Microsoft.Win32;

namespace Exam.App;

public partial class AgentEditWindow : Window
{
    private readonly string resourceRoot;
    public Agent Agent { get; }
    public List<LookupItem> Types { get; }

    public AgentEditWindow(Agent agent, List<LookupItem> types, string resourceRoot)
    {
        InitializeComponent();
        Agent = JsonSerializer.Deserialize<Agent>(JsonSerializer.Serialize(agent)) ?? new Agent();
        Types = types;
        this.resourceRoot = resourceRoot;
        DataContext = new AgentEditViewModel(Agent, Types);
    }

    private void PickLogo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        var imagesDir = Path.Combine(resourceRoot, "images");
        Directory.CreateDirectory(imagesDir);
        var fileName = Path.GetFileName(dialog.FileName);
        var target = Path.Combine(imagesDir, fileName);
        File.Copy(dialog.FileName, target, true);
        Agent.Logo = Path.Combine("images", fileName).Replace('\\', '/');
        DataContext = new AgentEditViewModel(Agent, Types);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Agent.Title))
        {
            MessageBox.Show(this, "Введите наименование агента.", "Проверка");
            return;
        }
        if (Agent.AgentTypeId == 0)
        {
            MessageBox.Show(this, "Выберите тип агента.", "Проверка");
            return;
        }
        if (string.IsNullOrWhiteSpace(Agent.Phone)) Agent.Phone = "+7 000 000 00 00";
        if (string.IsNullOrWhiteSpace(Agent.Inn)) Agent.Inn = "0000000000";
        DialogResult = true;
    }
}

public sealed class AgentEditViewModel
{
    public AgentEditViewModel(Agent agent, List<LookupItem> types)
    {
        Agent = agent;
        Types = types;
    }

    public Agent Agent { get; }
    public List<LookupItem> Types { get; }
    public string Title { get => Agent.Title; set => Agent.Title = value; }
    public string Phone { get => Agent.Phone; set => Agent.Phone = value; }
    public string Email { get => Agent.Email; set => Agent.Email = value; }
    public int Priority { get => Agent.Priority; set => Agent.Priority = value; }
    public string Director { get => Agent.Director; set => Agent.Director = value; }
    public string Address { get => Agent.Address; set => Agent.Address = value; }
    public string Inn { get => Agent.Inn; set => Agent.Inn = value; }
    public string Kpp { get => Agent.Kpp; set => Agent.Kpp = value; }
    public string Logo { get => Agent.Logo; set => Agent.Logo = value; }
}
