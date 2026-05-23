using Explorer.Models;

namespace Explorer.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public string User => FileSystem.CurrentUser;
    public string InitialDirectory => FileSystem.HomeDirectory;
}