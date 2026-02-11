namespace CareLog.Mobile;

public partial class App : Application
{
    public App(Views.TodayPage page)
    {
        InitializeComponent();
        MainPage = new NavigationPage(page);
    }
}
