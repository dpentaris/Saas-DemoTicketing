namespace CareFlux.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new ContentPage
        {
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label
                    {
                        Text = "Welcome to CareFlux",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            }
        };
    }
}
