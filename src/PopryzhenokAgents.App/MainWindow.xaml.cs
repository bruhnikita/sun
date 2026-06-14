using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using Exam.Core;
using Exam.Data;

namespace Exam.App;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly AgentRepository repo = new();
    private readonly List<Agent> original;
    private List<Agent> all;
    private int page = 1;
    private const int PageSize = 10;

    public ObservableCollection<Agent> PageItems { get; } = new();
    public List<string> Types { get; }
    public string[] Sorts { get; } = { "Сортировка: наименование", "Сортировка: скидка", "Сортировка: приоритет" };

    private string search = "";
    public string Search { get => search; set { search = value; page = 1; Refresh(); } }

    private string selectedType = "Все типы";
    public string SelectedType { get => selectedType; set { selectedType = value; page = 1; Refresh(); } }

    private string selectedSort = "Сортировка: наименование";
    public string SelectedSort { get => selectedSort; set { selectedSort = value; Refresh(); } }

    public Agent? Selected { get; set; }
    public int MassValue { get; set; }
    public string Counter { get; set; } = "";

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand MassCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand NextCommand { get; }
    public RelayCommand PrevCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        original = Clone(repo.Load());
        all = Clone(original);
        Types = all.Select(x => x.Type).Distinct().OrderBy(x => x).Prepend("Все типы").ToList();

        AddCommand = new(_ => AddAgent());
        EditCommand = new(_ => EditSelected());
        DeleteCommand = new(_ => DeleteSelected());
        MassCommand = new(_ => UpdateSelectedPriority());
        ResetCommand = new(_ => ResetData());
        NextCommand = new(_ => { page++; Refresh(); });
        PrevCommand = new(_ => { if (page > 1) page--; Refresh(); });

        DataContext = this;
        Refresh();
    }

    private void AddAgent()
    {
        all.Insert(0, new Agent
        {
            Id = all.Count == 0 ? 1 : all.Max(x => x.Id) + 1,
            Title = "Новый агент",
            Type = Types.Skip(1).FirstOrDefault() ?? "ООО",
            Phone = "+7 000 000 00 00",
            Email = "agent@example.ru",
            Priority = 1,
            SalesCount365 = 0,
            SalesAmount365 = 0
        });
        Save();
    }

    private void EditSelected()
    {
        if (Selected == null) return;
        Selected.Email = "updated@example.ru";
        Save();
    }

    private void DeleteSelected()
    {
        var selected = GetSelected().ToList();
        if (selected.Count == 0 && Selected != null) selected.Add(Selected);
        foreach (var item in selected)
        {
            if (!repo.CanDelete(item))
            {
                MessageBox.Show("Удаление запрещено: есть история продаж.", "Удаление");
                continue;
            }
            all.Remove(item);
        }
        Save();
    }

    private void UpdateSelectedPriority()
    {
        var selected = GetSelected().ToList();
        if (selected.Count == 0 && Selected != null) selected.Add(Selected);
        if (selected.Count == 0)
        {
            MessageBox.Show("Выберите одну или несколько записей.", "Массовое изменение");
            return;
        }
        foreach (var item in selected) item.Priority = MassValue;
        Save();
    }

    private void ResetData()
    {
        all = Clone(original);
        page = 1;
        Save();
    }

    private IEnumerable<Agent> GetSelected() => AgentsList.SelectedItems.Cast<Agent>();

    private void Save()
    {
        repo.Save(all);
        Refresh();
    }

    private void Refresh()
    {
        IEnumerable<Agent> query = all;
        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(x => (x.Title + " " + x.Email + " " + x.Phone).Contains(Search, StringComparison.OrdinalIgnoreCase));
        if (SelectedType != "Все типы")
            query = query.Where(x => x.Type == SelectedType);
        query = SelectedSort switch
        {
            "Сортировка: скидка" => query.OrderBy(x => x.Discount),
            "Сортировка: приоритет" => query.OrderBy(x => x.Priority),
            _ => query.OrderBy(x => x.Title)
        };

        var list = query.ToList();
        var maxPage = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
        if (page > maxPage) page = maxPage;

        PageItems.Clear();
        foreach (var item in ListTools.Page(list, page, PageSize)) PageItems.Add(item);
        Counter = $"Показано {PageItems.Count} из {list.Count}    {page}/{maxPage}";
        OnPropertyChanged(nameof(Counter));
    }

    private static List<Agent> Clone(List<Agent> items) =>
        JsonSerializer.Deserialize<List<Agent>>(JsonSerializer.Serialize(items)) ?? new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
