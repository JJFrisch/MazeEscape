namespace MazeEscape;

public partial class LoadingPage : ContentPage
{
	private int Width { get; set; }
	private int Height { get; set; }
	public LoadingPage(int width, int height)
	{
		InitializeComponent();
		Width = width;
		Height = height;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

        System.Threading.Thread.Sleep(5000);
        //_ = Navigation.PushAsync(new BasicGridPage(Width, Height, "Your First Maze"));
    }

}