using CareLog.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareLog.Mobile.ViewModels;

public partial class TodayViewModel(Services.ApiClient api, Services.SyncService sync) : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    public ObservableCollection<VisitItem> Visits { get; } = new();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var items = await api.GetTodayVisitsAsync() ?? new();
            Visits.Clear();
            foreach (var item in items) Visits.Add(item);
            await sync.SyncPendingAsync(default);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] public Task CheckInAsync(Guid visitId) => Task.CompletedTask;
    [RelayCommand] public Task AddRecordAsync(Guid visitId) => Task.CompletedTask;
    [RelayCommand] public Task CheckOutAsync(Guid visitId) => Task.CompletedTask;
}
