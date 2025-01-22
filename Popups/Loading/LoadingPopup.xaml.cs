using Mopups.Interfaces;
using Mopups.Services;

namespace MazeEscape;

public partial class LoadingPopup
{
    public LoadingPopup()
    {
        InitializeComponent();
    }
}

//public class LoadingService : ILoadingService, IDisposable
//{
//    private readonly IPopupNavigation navigation;

//    public LoadingService()
//    {
//        navigation = MopupService.Instance;
//    }

//    public async void Dispose()
//    {
//        await navigation.PopAsync();
//    }

//    public async Task<IDisposable> Show()
//    {
//        await navigation.PushAsync(new LoadingPopup(), true);
//        return this;
//    }
//}