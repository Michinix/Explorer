using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected static async void FireAndForget(Task task)
    {
        try { await task; }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }
}