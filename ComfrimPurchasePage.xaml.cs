namespace MazeEscape;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;
using Microsoft.Maui.Graphics;
using System;

public partial class ComfrimPurchasePage : Popup
{


    public int price;

    public SkinModel Skin;

    //Dictionary<int, string> PlayerIconNames = new Dictionary<int, string>();

	public ComfrimPurchasePage(SkinModel skin, ImageButton imageButton, Label label, ImageButton lock_icon)
	{
		InitializeComponent();

        //PlayerIconNames.Add(0, "Maze Runner");
        //PlayerIconNames.Add(1, "Cool Lion");
        //PlayerIconNames.Add(2, "Sunset Spiral");
        //PlayerIconNames.Add(3, "Pink Sky");
        //PlayerIconNames.Add(4, "Heart Disk");
        //PlayerIconNames.Add(5, "Fireball");
        //PlayerIconNames.Add(6, "Galaxy Marble");

        double width = 250; // App.PlayerData.WindowWidth * 0.4;  // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / 4;
        double height = 250; // App.PlayerData.WindowHeight * 0.6; // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / 4;

        // Set   the size of the popup
        this.Size = new Size(width, height);

        price = skin.CoinPrice;
        itemPriceLabel.Text = price.ToString();
        if (skin.IsSpecialSkin) { itemPriceLabel.Text = "Special Unlock"; itemPriceLabel.FontSize = 12; }
        if (skin.GemPrice > 0) { itemPriceLabel.Text = $"💎 {skin.GemPrice}"; itemPriceLabel.TextColor = Colors.MediumPurple; }

        Skin = skin;

        titleLabel.Text = skin.Name;
        itemImage.Source = $"{skin.ImageUrl}_icon.png";


        if ((skin.GemPrice > 0 && App.PlayerData.GemCount >= skin.GemPrice) ||
            (App.PlayerData.CoinCount >= price && !skin.IsSpecialSkin && skin.GemPrice == 0))
        {
            itemPriceLabel.TextColor = Colors.Gold;
            itemPriceLabel.Opacity = 1;
            purchaseButton.Opacity = 0;


            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                await PurchaseButton_Clicked(s, e);
            };

            itemPriceLabel.GestureRecognizers.Add(tapGestureRecognizer);
        }
        else
        {
            itemPriceLabel.TextColor = Colors.Black;
            itemPriceLabel.Opacity = 0.8;
            purchaseButton.Opacity = 0.5;

            if (!skin.IsSpecialSkin)
            {
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += async (s, e) =>
                {
                    await NotEnoughCoinsPurchaseButton_Clicked(s, e);
                };

                itemPriceLabel.GestureRecognizers.Add(tapGestureRecognizer);
            }
        }
    }

    async Task PurchaseButton_Clicked(object sender, EventArgs e)
    {
        if (Skin.GemPrice > 0)
        {
            App.PlayerData.GemCount -= Skin.GemPrice;
        }
        else
        {
            App.PlayerData.CoinCount -= price;
        }
        Skin.IsUnlocked = true;
        App.PlayerData.Save();

        _ = itemPriceLabel.ScaleTo(1.1, 500);
        await itemPriceLabel.TextColorTo(Colors.Green, 200, 500);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Purchased", cts.Token);
    }

    async Task NotEnoughCoinsPurchaseButton_Clicked(object sender, EventArgs e)
    {
        itemPriceLabel.TextColor = Colors.Black;
        itemPriceLabel.FontSize = 12;
        itemPriceLabel.Text = Skin.GemPrice > 0 ? "Not Enough Gems" : "Not Enough Coins";

        _ = itemPriceLabel.ScaleTo(1.1, 200);
        await itemPriceLabel.TextColorTo(Colors.Black, 500, 500);

        await itemPriceLabel.ScaleTo(1, 500);
        itemPriceLabel.FontSize = 18;
        itemPriceLabel.Text = price.ToString();

        //var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        //await CloseAsync("Purchased", cts.Token);
    }

    async void ExitButton_Clicked(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync("Close", cts.Token);
    }
}