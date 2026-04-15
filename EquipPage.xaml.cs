using CommunityToolkit.Maui.Views;
using MazeEscape.Models;
using System.Xml.Linq;

namespace MazeEscape;

public partial class EquipPage : ContentPage
{
	public EquipPage()
	{
		InitializeComponent();

        playerImageGrid.HeightRequest = 7 * 110;

        InitializeButtons();

        characterImage.Source = App.PlayerData.PlayerCurrentSkin.ImageUrl+"_icon.png";
        CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();
        GemCountLabel.Text = App.PlayerData.GemCount.ToString();


    }


    //List<int> prices = new List<int>() { 500, 1000, 2000, 3000, 5000, 10000 };
    public void InitializeButtons()
    {
        int grid_cols = 3;

        //App.PlayerData.UnlockedSkins.Sort((x, y) => y.CoinPrice.CompareTo(x.CoinPrice));
        //App.PlayerData.UnlockedSkins.Sort((x, y) => x.IsSpecialSkin.CompareTo(y.IsSpecialSkin));
        //App.PlayerData.UnlockedSkins.Sort((x, y) => y.IsUnlocked.CompareTo(x.IsUnlocked));

        App.PlayerData.UnlockedSkins =
                    App.PlayerData.UnlockedSkins
                        .OrderByDescending(m => m.IsUnlocked)
                        .ThenBy(m => m.IsSpecialSkin)
                        .ThenBy(m => m.CoinPrice)
                        .ToList<SkinModel>();

        for (int num = 0; num < App.PlayerData.UnlockedSkins.Count; num++)
        {
            SkinModel skin = App.PlayerData.UnlockedSkins[num];
            ImageButton imageButton;
            int col = num % grid_cols;
            int row = num / grid_cols;

            imageButton = new ImageButton
            {
                WidthRequest = 80,
                Source = $"{skin.ImageUrl}_icon.png",
                CornerRadius = 20,
                VerticalOptions = LayoutOptions.Start,
                Background = Colors.Transparent,
            };

            if (skin.IsUnlocked)
            {
                imageButton.Clicked += async (s, e) =>
                {
                    await Equip(skin, imageButton);
                };
                Grid.SetColumn(imageButton, col);
                Grid.SetRow(imageButton, row);

                playerImageGrid.Add(imageButton);
            }
            else
            {
                imageButton.Opacity = 0.3;
                Label priceLabel = new Label();

                if (!skin.IsSpecialSkin)
                {
                    BoxView priceBackground = new BoxView
                    {
                        BackgroundColor = Colors.DarkGreen,
                        Opacity = 0.4,
                        CornerRadius = 50,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.End,
                        HeightRequest = 22,
                        WidthRequest = 80,
                    };
                    Grid.SetColumn(priceBackground, col);
                    Grid.SetRow(priceBackground, row);
                    playerImageGrid.Add(priceBackground);



                    priceLabel = new Label
                    {
                        Text = skin.GemPrice > 0 ? skin.GemPrice.ToString() : skin.CoinPrice.ToString(),
                        TextColor = skin.GemPrice > 0 ? Colors.MediumPurple : Colors.Gold,
                        FontSize = 15,
                        WidthRequest = 70,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.End,
                        VerticalOptions = LayoutOptions.End,
                    };

                    if (skin.GemPrice == 0 && skin.CoinPrice > App.PlayerData.CoinCount)
                    {
                        priceLabel.TextColor = ThemeColors.OverlayMutedText;
                    }

                    Grid.SetColumn(priceLabel, col);
                    Grid.SetRow(priceLabel, row);
                    playerImageGrid.Add(priceLabel);


                    ImageButton coin = new ImageButton
                    {
                        Source = "coin.png",
                        BackgroundColor = Colors.Transparent,
                        Aspect = Aspect.AspectFit,
                        //Scale = 0.75,
                        WidthRequest = 20,
                        HeightRequest = 20,
                        MaximumWidthRequest = 20,
                        Padding = new Thickness(17, 27, 0, 4),
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.End,
                    };

                    if (skin.GemPrice > 0)
                    {
                        coin.IsVisible = false;
                        Label gemLabel = new Label
                        {
                            Text = $"💎 {skin.GemPrice}",
                            TextColor = Colors.MediumPurple,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.End,
                            Padding = new Thickness(0, 0, 0, 4),
                        };
                        Grid.SetColumn(gemLabel, col);
                        Grid.SetRow(gemLabel, row);
                        playerImageGrid.Add(gemLabel);
                    }

                    Grid.SetColumn(coin, col);
                    Grid.SetRow(coin, row);
                    playerImageGrid.Add(coin);
                }

                ImageButton lock_icon = new ImageButton
                {
                    WidthRequest = 80,
                    Source = $"lock.png",
                    CornerRadius = 20,
                    VerticalOptions = LayoutOptions.Start,
                    Background = Colors.Transparent,
                };
                int skin_number = num;
                lock_icon.Clicked += async (s, e) =>
                {
                    await OnClick_Locked(skin, imageButton, priceLabel, lock_icon);
                };
                Grid.SetColumn(lock_icon, col);
                Grid.SetRow(lock_icon, row);

                imageButton.Clicked += async (s, e) =>
                {
                    await OnClick_Locked(skin, imageButton, priceLabel, lock_icon);
                };
                Grid.SetColumn(imageButton, col);
                Grid.SetRow(imageButton, row);
                playerImageGrid.Add(imageButton);

                playerImageGrid.Add(lock_icon);


            }

        }

    }

    public async Task Equip(SkinModel skin, ImageButton imageButton)
    {
        App.PlayerData.PlayerCurrentSkin = skin;
        characterImage.Source = $"{skin.ImageUrl}_icon.png";
        App.PlayerData.Save();

        await imageButton.FadeTo(0.8, 200);
        await imageButton.FadeTo(1, 200);
    }

    public async Task OnClick_Locked(SkinModel skin, ImageButton imageButton, Label label, ImageButton lock_icon)
    {

        var result = await this.ShowPopupAsync(new ComfrimPurchasePage(skin, imageButton, label, lock_icon), CancellationToken.None);
        CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();
        GemCountLabel.Text = App.PlayerData.GemCount.ToString();

        if (result == "Purchased")
        {
            string name = skin.ImageUrl;
            App.PlayerData.PlayerCurrentSkin = skin;
            characterImage.Source = $"{name}_icon.png";
            await Navigation.PushAsync(new EquipPage());
            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);

        }
        else if (result == "Close")
        {

        }


        //if (label.Text != "Comfirm?")
        //{
        //    if (App.PlayerData.CoinCount >= int.Parse(label.Text))
        //    {
        //        App.PlayerData.CoinCount -= int.Parse(label.Text);
        //        CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();
        //        label.Text = "Comfirm?";
        //    }
        //    else
        //    {
        //        await DisplayAlert("Not enough coins", "You need " + (int.Parse(label.Text) - App.PlayerData.CoinCount) + " more coins to unlock this skin", "OK");
        //    }
        //}
        //else
        //{
        //    App.PlayerData.CoinCount -= int.Parse(label.Text);
        //    CoinCountLabel.Text = App.PlayerData.CoinCount.ToString();

        //    App.PlayerData.UnlockedSkins.Add(num);  
        //}
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}