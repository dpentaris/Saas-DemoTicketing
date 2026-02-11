namespace CareLog.Mobile.Views;

public partial class TodayPage : ContentPage
{
    public TodayPage(ViewModels.TodayViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
