namespace DocSearch.MauiApp;

public partial class MainPage : ContentPage
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MainPage(IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();

        _httpClientFactory = httpClientFactory;
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        SetLoading();
    }

    private async void OnSaveAllClicked(object sender, EventArgs e)
    {
        
    }

    private void SetLoading()
    {
        picture0.Source = picture1.Source = picture2.Source = picture3.Source = "loading.gif";

        btnGenerate.IsEnabled = false;
        btnSaveAll.IsEnabled = false;
    }

    private void SetError()
    {
        picture0.Source = picture1.Source = picture2.Source = picture3.Source = "error.png";

        btnGenerate.IsEnabled = true;
        btnSaveAll.IsEnabled = false;
    }
}