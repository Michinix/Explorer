using Explorer.Services;

namespace Explorer.ViewModels;

public class NavBarViewModel(
    NavigationService navigation,
    DataGridViewModel dataGrid,
    FileOperationsViewModel fileOperations) : ViewModelBase
{
    public NavigationService Navigation { get; } = navigation;
    public DataGridViewModel DataGrid { get; } = dataGrid;
    public FileOperationsViewModel FileOperations { get; } = fileOperations;
}