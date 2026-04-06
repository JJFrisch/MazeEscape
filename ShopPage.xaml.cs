
namespace MazeEscape;

using MazeEscape.Services;

public partial class ShopPage : ContentPage
{
	public List<string> NamesOfPowerUps = new List<string> { "Hint", "ExtraTime", "ExtraMoves" };
    Dictionary<string, int> CostsOfPowerUps = new Dictionary<string, int>();
    Dictionary<string, Label> NumberOwnedLabels = new Dictionary<string, Label>();
    private ISaveSynchronizer? _saveSynchronizer;

    public ShopPage(ISaveSynchronizer? saveSynchronizer = null)
	{
		InitializeComponent();
		_saveSynchronizer = saveSynchronizer;



        CostsOfPowerUps.Add("Hint", 200);
        CostsOfPowerUps.Add("ExtraTime", 150);
        CostsOfPowerUps.Add("ExtraMoves", 50);

        hintCostLabel.Text = CostsOfPowerUps["Hint"].ToString();
        extraTimeCostLabel.Text = CostsOfPowerUps["ExtraTime"].ToString();
        extraMovesCostLabel.Text = CostsOfPowerUps["ExtraMoves"].ToString();

        NumberOwnedLabels.Add("ExtraTime", extraTimeNumberLabel);
        NumberOwnedLabels.Add("Hint", hintNumberLabel);
        NumberOwnedLabels.Add("ExtraMoves", extraMovesNumberLabel);

        foreach (string name in NamesOfPowerUps)
        {
            NumberOwnedLabels[name].Text = App.PlayerData.GetPowerupCountFromName(name).ToString();
        }

        //DrawPowerUps();

        CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();

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
                Text = App.PlayerData.GetPowerupCountFromName(name).ToString(),
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

            //shopItemGrid.Add(backgroundImage);
            //shopItemGrid.Add(nameLabel);
            //shopItemGrid.Add(costLabel);
            //shopItemGrid.Add(seperationLine);
            //shopItemGrid.Add(numberOwnedLabel);
            //shopItemGrid.Add(powerupIconimage);

            row++;
        }
    }


    public async Task Buy(string name, ImageButton imageButton)
    {
        if (App.PlayerData.CoinCount >= CostsOfPowerUps[name])
        {
            App.PlayerData.CoinCount -= CostsOfPowerUps[name];
            App.PlayerData.AddPowerup(name);
            App.PlayerData.Save();

            // Sync save to API
            if (_saveSynchronizer != null)
            {
                _ = _saveSynchronizer.SaveCheckpointAsync(App.PlayerData, "item_purchased");
            }

            _ = imageButton.FadeTo(0.5, 500);
            await imageButton.ScaleTo(0.8, 500);
            _ = imageButton.FadeTo(1, 500);
            await imageButton.ScaleTo(1, 500);

            CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();
            NumberOwnedLabels[name].Text = App.PlayerData.GetPowerupCountFromName(name).ToString();
        }
        else
        {
            await DisplayAlert("Not Enough Coins", "You can earn more by beating mazes or opening chests.", "OK");
            //await imageButton.ScaleTo(1.5, 500);
            //await imageButton.ScaleTo(1, 500);
        }
    }

    private async void BuyTimeClicked(object sender, EventArgs e)
    {
        await Buy("ExtraTime", timeImageButton);
    }
    private async void BuyHintClicked(object sender, EventArgs e)
    {
        await Buy("Hint", hintImageButton);
    }
    private async void BuyMovesClicked(object sender, EventArgs e)
    {
        await Buy("ExtraMoves", movesImageButton);
    }



    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
