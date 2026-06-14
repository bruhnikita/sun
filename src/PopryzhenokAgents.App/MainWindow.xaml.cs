using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Exam.Core;
using Exam.Data;

namespace Exam.App;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly AgentRepository repo = new();
    private List<Agent> all = new();
    private List<LookupItem> typeItems = new();
    private int page = 1;
    private const int PageSize = 10;

    public ObservableCollection<Agent> PageItems { get; } = new();
    public List<string> Types { get; private set; } = new() { "Все типы" };
    public string[] Sorts { get; } = { "Сортировка: наименование", "Сортировка: скидка", "Сортировка: приоритет" };

    private string search = "";
    public string Search { get => search; set { search = value; page = 1; RefreshView(); } }

    private string selectedType = "Все типы";
    public string SelectedType { get => selectedType; set { selectedType = value; page = 1; RefreshView(); } }

    private string selectedSort = "Сортировка: наименование";
    public string SelectedSort { get => selectedSort; set { selectedSort = value; RefreshView(); } }

    public Agent? Selected { get; set; }
    public int MassValue { get; set; }
    public string Counter { get; set; } = "";

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand MassCommand { get; }
    public RelayCommand HistoryCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand NextCommand { get; }
    public RelayCommand PrevCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        AddCommand = new(_ => AddAgent());
        EditCommand = new(_ => EditSelected());
        DeleteCommand = new(_ => DeleteSelected());
        MassCommand = new(_ => UpdateSelectedPriority());
        HistoryCommand = new(_ => ShowHistory());
        RefreshCommand = new(_ => LoadFromDatabase(true));
        NextCommand = new(_ => { page++; RefreshView(); });
        PrevCommand = new(_ => { if (page > 1) page--; RefreshView(); });
        DataContext = this;
        LoadFromDatabase(false);
    }

    private void LoadFromDatabase(bool showSuccess)
    {
        try
        {
            typeItems = repo.GetTypes();
            Types = typeItems.Select(x => x.Title).Prepend("Все типы").ToList();
            all = repo.Load();
            OnPropertyChanged(nameof(Types));
            RefreshView();
            if (showSuccess) MessageBox.Show(this, "Данные загружены из SQL Server.", "Обновление");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Не удалось подключиться к SQL Server. Выполните database/sqlserver/setup-db.ps1 и проверьте переменную SQLSERVER." + Environment.NewLine + ex.Message, "База данных");
        }
    }

    private void AddAgent()
    {
        var firstType = typeItems.FirstOrDefault();
        var agent = new Agent
        {
            AgentTypeId = firstType?.Id ?? 0,
            Type = firstType?.Title ?? "",
            Title = "Новый агент",
            Phone = "+7 000 000 00 00",
            Email = "agent@example.ru",
            Inn = "0000000000",
            Kpp = "",
            Logo = "picture.png",
            Priority = 1
        };
        ShowEditor(agent);
    }

    private void EditSelected()
    {
        if (Selected == null) return;
        ShowEditor(Selected);
    }

    private void ShowEditor(Agent agent)
    {
        var resourceRoot = System.IO.Path.Combine(AppContext.BaseDirectory, "resources");
        var window = new AgentEditWindow(agent, typeItems, resourceRoot) { Owner = this };
        if (window.ShowDialog() != true) return;
        repo.Save(window.Agent);
        LoadFromDatabase(false);
    }

    private void ShowHistory()
    {
        if (Selected == null) return;
        var window = new SalesHistoryWindow(Selected.Title, repo.GetSales(Selected.Id)) { Owner = this };
        window.ShowDialog();
    }

    private void DeleteSelected()
    {
        var selected = GetSelected().ToList();
        if (selected.Count == 0 && Selected != null) selected.Add(Selected);
        if (selected.Count == 0) return;
        var result = repo.Delete(selected);
        LoadFromDatabase(false);
        MessageBox.Show(this, result.Summary, "Удаление");
    }

    private void UpdateSelectedPriority()
    {
        var selected = GetSelected().Select(x => x.Id).ToList();
        if (selected.Count == 0 && Selected != null) selected.Add(Selected.Id);
        if (selected.Count == 0)
        {
            MessageBox.Show(this, "Выберите одну или несколько записей.", "Массовое изменение");
            return;
        }
        repo.BulkUpdatePriority(selected, MassValue);
        LoadFromDatabase(false);
    }

    private IEnumerable<Agent> GetSelected() => AgentsList.SelectedItems.Cast<Agent>();

    private void RefreshView()
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
