using Explorer.Models;

namespace Explorer.ViewModels;

public partial class HomeViewModel(Browser browser) : ViewModelBase
{
    public string User { get; } = browser.CurrentUser;
    public string InitialDirectory { get; } = browser.HomeDirectory;
}