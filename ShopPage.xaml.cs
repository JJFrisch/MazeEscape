
namespace MazeEscape;

public partial class ShopPage : ContentPage
{
	public List<string> NamesOfPowerUps = new List<string> { "Hint", "Hot or Cold" };
    Dictionary<string, int> CostsOfPowerUps = new Dictionary<string, int>();
    Dictionary<string, Label> NumberOwnedLabels = new Dictionary<string, Label>();
    public ShopPage()
	{
		InitializeComponent();

        CostsOfPowerUps.Add("Hint", 300);
        CostsOfPowerUps.Add("Hot or Cold", 50);

        DrawPowerUps();

        CoinCountLabel.Text = PlayerData.CoinCount.ToString();

    }

	public void DrawPowerUps()
	{
        int row = 0;
        foreach (string name in NamesOfPowerUps)
        {
            
            // Background Image
            ImageButton backgroundImage = new ImageButton
            {
                HeightRequest = 100,
                Aspect = Aspect.Fill,
                Source = "button_background_1.png",
                BackgroundColor = Colors.Green,
                CornerRadius = 20,
            };
            backgroundImage.Clicked += async (s, e) =>
            {
                await Buy(name, backgroundImage);
            };
            Grid.SetColumnSpan(backgroundImage, 7);
            Grid.SetRow(backgroundImage, row);

            // Defining On tapped/Clicked for all the elements
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                await Buy(name, backgroundImage);
            };


            Label nameLabel = new()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Text = name + ": ",
                FontSize = 26,
                TextColor = Colors.Black,
            };
            nameLabel.GestureRecognizers.Add(tapGestureRecognizer);
            Grid.SetColumn(nameLabel, 1);
            Grid.SetRow(nameLabel, row);

            Label costLabel = new()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Text = CostsOfPowerUps[name].ToString(),
                FontSize = 28,
                TextColor = Colors.SaddleBrown,
                FontAttributes = FontAttributes.Bold,
            };
            costLabel.GestureRecognizers.Add(tapGestureRecognizer);
            Grid.SetColumn(costLabel, 2);
            Grid.SetRow(costLabel, row);

            BoxView seperationLine = new()
            {
                BackgroundColor = Colors.OrangeRed,
                WidthRequest = 5,
                Opacity = 0.7,
            };
            seperationLine.GestureRecognizers.Add(tapGestureRecognizer);
            Grid.SetColumn(seperationLine, 3);
            Grid.SetRow(seperationLine, row);

            Label numberOwnedLabel = new()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Text = PlayerData.GetPowerupCountFromName(name).ToString(),
                FontSize = 18,
                TextColor = Colors.Black,
            };
            NumberOwnedLabels.Add(name, numberOwnedLabel);
            numberOwnedLabel.GestureRecognizers.Add(tapGestureRecognizer);
            Grid.SetColumn(numberOwnedLabel, 4);
            Grid.SetRow(numberOwnedLabel, row);

            Image powerupIconimage = new()
            {
                HorizontalOptions = LayoutOptions.Center,
                Source = "play_button_image_2.png",
            };
            powerupIconimage.GestureRecognizers.Add(tapGestureRecognizer);
            Grid.SetColumn(powerupIconimage, 5);
            Grid.SetRow(powerupIconimage, row);

            shopItemGrid.Add(backgroundImage);
            shopItemGrid.Add(nameLabel);
            shopItemGrid.Add(costLabel);
            shopItemGrid.Add(seperationLine);
            shopItemGrid.Add(numberOwnedLabel);
            shopItemGrid.Add(powerupIconimage);

            row++;
        }
    }


    public async Task Buy(string name, ImageButton imageButton)
    {
        if (PlayerData.CoinCount >= CostsOfPowerUps[name])
        {
            PlayerData.CoinCount -= CostsOfPowerUps[name];
            PlayerData.AddPowerup(name);

            _ = imageButton.FadeTo(0.5, 500);
            await imageButton.ScaleTo(1.2, 500);
            _ = imageButton.FadeTo(1, 500);
            await imageButton.ScaleTo(1, 500);

            CoinCountLabel.Text = PlayerData.CoinCount.ToString();
            NumberOwnedLabels[name].Text = PlayerData.GetPowerupCountFromName(name).ToString();
        }
        else
        {
            await imageButton.ScaleTo(1.5, 500);
            await imageButton.ScaleTo(1, 500);
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
