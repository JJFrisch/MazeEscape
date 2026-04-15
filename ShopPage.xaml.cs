
namespace MazeEscape;

using MazeEscape.Models;
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
                TextColor = ThemeColors.OverlayText,
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
                TextColor = ThemeColors.OverlayText,
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

    // ── Tab switching ──────────────────────────────────────────────────────────

    private void SwitchToTab(string tab)
    {
        powerupsScrollView.IsVisible = tab == "Powerups";
        skinsScrollView.IsVisible    = tab == "Skins";
        coresScrollView.IsVisible    = tab == "Cores";

        powerupsTabBtn.BackgroundColor = tab == "Powerups" ? Color.FromArgb("#3A5D8A") : Color.FromArgb("#2A2A2A");
        skinsTabBtn.BackgroundColor    = tab == "Skins"    ? Color.FromArgb("#3A5D8A") : Color.FromArgb("#2A2A2A");
        coresTabBtn.BackgroundColor    = tab == "Cores"    ? Color.FromArgb("#3A5D8A") : Color.FromArgb("#2A2A2A");
    }

    private void PowerupsTabClicked(object sender, EventArgs e) => SwitchToTab("Powerups");

    private void SkinsTabClicked(object sender, EventArgs e)
    {
        SwitchToTab("Skins");
        PopulateSkinsTab();
    }

    private void CoresTabClicked(object sender, EventArgs e)
    {
        SwitchToTab("Cores");
        PopulateCoresTab();
    }

    // ── Skins tab ──────────────────────────────────────────────────────────────

    private bool _skinsPopulated;

    private void PopulateSkinsTab()
    {
        if (_skinsPopulated) return;
        _skinsPopulated = true;

        var skins = App.PlayerData.UnlockedSkins
            .OrderByDescending(s => s.IsUnlocked)
            .ThenBy(s => s.IsSpecialSkin)
            .ThenBy(s => s.CoinPrice)
            .ToList();

        int cols = 3;
        int rowCount = (int)Math.Ceiling(skins.Count / (double)cols);
        for (int r = 0; r < rowCount; r++)
            skinsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < skins.Count; i++)
        {
            SkinModel skin = skins[i];
            int col = i % cols;
            int row = i / cols;

            var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center };

            var img = new ImageButton
            {
                Source = $"{skin.ImageUrl}_icon.png",
                WidthRequest = 70,
                HeightRequest = 70,
                CornerRadius = 14,
                BackgroundColor = Colors.Transparent,
                Opacity = skin.IsUnlocked ? 1.0 : 0.4,
            };

            if (skin.IsUnlocked)
            {
                img.Clicked += async (s, e) => await EquipSkin(skin, img);
            }
            else if (!skin.IsSpecialSkin)
            {
                img.Clicked += async (s, e) => await BuySkin(skin, img, stack);
            }

            stack.Add(img);

            if (!skin.IsUnlocked && !skin.IsSpecialSkin)
            {
                string priceText = skin.GemPrice > 0 ? $"💎 {skin.GemPrice}" : $"{skin.CoinPrice}";
                Color priceColor = skin.GemPrice > 0 ? ThemeColors.OverlayGemText :
                    (skin.CoinPrice > App.PlayerData.CoinCount ? ThemeColors.OverlayMutedText : ThemeColors.OverlayGoldText);

                stack.Add(new Label
                {
                    Text = priceText,
                    TextColor = priceColor,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                });
            }
            else if (skin.IsUnlocked)
            {
                stack.Add(new Label
                {
                    Text = skin.Name,
                    TextColor = ThemeColors.OverlayText,
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.Center,
                });
            }

            Grid.SetRow(stack, row);
            Grid.SetColumn(stack, col);
            skinsGrid.Add(stack);
        }
    }

    private async Task EquipSkin(SkinModel skin, ImageButton img)
    {
        App.PlayerData.PlayerCurrentSkin = skin;
        App.PlayerData.Save();
        await img.ScaleTo(0.85, 100);
        await img.ScaleTo(1.0, 100);
        await DisplayAlert("Equipped", $"{skin.Name} is now active.", "OK");
    }

    private async Task BuySkin(SkinModel skin, ImageButton img, VerticalStackLayout stack)
    {
        bool canAfford = skin.GemPrice > 0
            ? App.PlayerData.GemCount >= skin.GemPrice
            : App.PlayerData.CoinCount >= skin.CoinPrice;

        if (!canAfford)
        {
            await DisplayAlert("Not Enough", skin.GemPrice > 0 ? "Not enough gems." : "Not enough coins.", "OK");
            return;
        }

        if (skin.GemPrice > 0)
            App.PlayerData.GemCount -= skin.GemPrice;
        else
            App.PlayerData.CoinCount -= skin.CoinPrice;

        skin.IsUnlocked = true;
        App.PlayerData.PlayerCurrentSkin = skin;
        App.PlayerData.Save();

        if (_saveSynchronizer != null)
            _ = _saveSynchronizer.SaveCheckpointAsync(App.PlayerData, "skin_purchased");

        CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();

        img.Opacity = 1.0;
        img.Clicked -= (s, e) => { };
        img.Clicked += async (s, e) => await EquipSkin(skin, img);

        // Remove price label
        if (stack.Count > 1)
            stack.RemoveAt(1);
        stack.Add(new Label
        {
            Text = skin.Name,
            TextColor = ThemeColors.OverlayText,
            FontSize = 10,
            HorizontalTextAlignment = TextAlignment.Center,
        });

        await img.ScaleTo(0.85, 100);
        await img.ScaleTo(1.0, 100);
        await DisplayAlert("Purchased", $"{skin.Name} equipped!", "OK");
    }

    // ── Cores tab ─────────────────────────────────────────────────────────────

    private static readonly List<(int Id, string Name, Color Color, int Price)> CoreOptions =
    [
        (0, "Classic Red",    Colors.IndianRed,       0),
        (1, "Electric Blue",  Colors.CornflowerBlue, 400),
        (2, "Emerald",        Colors.MediumSeaGreen, 400),
        (3, "Golden",         Colors.Goldenrod,      800),
        (4, "Violet",         Colors.MediumOrchid,   800),
        (5, "Cosmic",         Colors.DeepSkyBlue,   1500),
        (6, "Lava",           Colors.OrangeRed,     1500),
    ];

    private bool _coresPopulated;

    private void PopulateCoresTab()
    {
        if (_coresPopulated) return;
        _coresPopulated = true;

        int cols = 2;
        int rowCount = (int)Math.Ceiling(CoreOptions.Count / (double)cols);
        for (int r = 0; r < rowCount; r++)
            coresGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < CoreOptions.Count; i++)
        {
            var (id, name, color, price) = CoreOptions[i];
            int col = i % cols;
            int row = i / cols;

            bool isUnlocked = App.PlayerData.UnlockedCoreIds.Contains(id);
            bool isSelected = App.PlayerData.SelectedCoreId == id;

            var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };

            var swatch = new Border
            {
                WidthRequest = 70,
                HeightRequest = 70,
                BackgroundColor = color,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(14) },
                Stroke = isSelected ? Colors.White : Colors.Transparent,
                StrokeThickness = isSelected ? 3 : 0,
                Opacity = isUnlocked ? 1.0 : 0.45,
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await SelectOrBuyCore(id, name, price, stack);
            swatch.GestureRecognizers.Add(tap);

            stack.Add(swatch);
            stack.Add(new Label
            {
                Text = name,
                    TextColor = ThemeColors.OverlayText,
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
            });

            if (!isUnlocked && price > 0)
            {
                stack.Add(new Label
                {
                    Text = $"{price} coins",
                    TextColor = price > App.PlayerData.CoinCount ? ThemeColors.OverlayMutedText : ThemeColors.OverlayGoldText,
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                });
            }
            else if (isSelected)
            {
                stack.Add(new Label
                {
                    Text = "Equipped",
                    TextColor = ThemeColors.OverlaySuccessText,
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.Center,
                });
            }

            Grid.SetRow(stack, row);
            Grid.SetColumn(stack, col);
            coresGrid.Add(stack);
        }
    }

    private async Task SelectOrBuyCore(int id, string name, int price, VerticalStackLayout stack)
    {
        if (!App.PlayerData.UnlockedCoreIds.Contains(id))
        {
            if (App.PlayerData.CoinCount < price)
            {
                await DisplayAlert("Not Enough Coins", "You need more coins to unlock this core.", "OK");
                return;
            }
            App.PlayerData.CoinCount -= price;
            App.PlayerData.UnlockedCoreIds.Add(id);
            CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();
        }

        App.PlayerData.SelectedCoreId = id;
        App.PlayerData.Save();

        if (_saveSynchronizer != null)
            _ = _saveSynchronizer.SaveCheckpointAsync(App.PlayerData, "core_selected");

        // Refresh cores tab to reflect new selection
        _coresPopulated = false;
        coresGrid.Clear();
        coresGrid.RowDefinitions.Clear();
        PopulateCoresTab();
    }
}

