using CommunityToolkit.Maui.Views;
using System.Xml.Linq;

namespace MazeEscape;

public partial class EquipPage : ContentPage
{
	public EquipPage()
	{
		InitializeComponent();

        InitializeButtons();

        characterImage.Source = PlayerData.PlayerImageName.Replace(".png", "")+"_icon.png";
        CoinCountLabel.Text = PlayerData.CoinCount.ToString();


    }


    List<int> prices = new List<int>() { 500, 1000, 2000, 3000, 5000, 10000 };
    public void InitializeButtons()
    {
        for (int num = 0; num <= 6; num++)
        {
            ImageButton imageButton;
            int col = num % 3;
            int row = num / 3;
            string name = $"player_image{num}";


            imageButton = new ImageButton
            {
                WidthRequest = 80,
                Source = $"player_image{num}_icon.png",
                CornerRadius = 20,
                VerticalOptions = LayoutOptions.Start,
                Background = Colors.Transparent,
            };

            if (PlayerData.UnlockedSkins.Contains(num))
            {
                imageButton.Clicked += async (s, e) =>
                {
                    await Equip(name, imageButton);
                };
                Grid.SetColumn(imageButton, col);
                Grid.SetRow(imageButton, row);

                playerImageGrid.Add(imageButton);
            }
            else
            {
                imageButton.Opacity = 0.3;

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

                Label priceLabel = new Label
                {
                    Text = prices[num-1].ToString(),
                    TextColor = Colors.Gold,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                };

                if (prices[num-1] > PlayerData.CoinCount)
                {
                    priceLabel.TextColor = Colors.Black;
                }

                Grid.SetColumn(priceLabel, col);
                Grid.SetRow(priceLabel, row);
                playerImageGrid.Add(priceLabel);

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
                    await OnClick_Locked(skin_number, imageButton, priceLabel, lock_icon);
                };
                Grid.SetColumn(lock_icon, col);
                Grid.SetRow(lock_icon, row);

                imageButton.Clicked += async (s, e) =>
                {
                    await OnClick_Locked(skin_number, imageButton, priceLabel, lock_icon);
                };
                Grid.SetColumn(imageButton, col);
                Grid.SetRow(imageButton, row);
                playerImageGrid.Add(imageButton);
                
                playerImageGrid.Add(lock_icon);


            }

        }

    }

    public async Task Equip(string name, ImageButton imageButton)
    {
        PlayerData.PlayerImageName = name+".png";
        characterImage.Source = $"{name}_icon.png";
        PlayerData.Save();

        await imageButton.FadeTo(0.8, 200);
        await imageButton.FadeTo(1, 200);
    }

    public async Task OnClick_Locked(int num, ImageButton imageButton, Label label, ImageButton lock_icon)
    {

        var result = await this.ShowPopupAsync(new ComfrimPurchasePage(num, imageButton, label, lock_icon), CancellationToken.None);
        CoinCountLabel.Text = PlayerData.CoinCount.ToString();

        if (result == "Purchased")
        {
            string name = $"player_image{num}";
            PlayerData.PlayerImageName = name + ".png";
            characterImage.Source = $"{name}_icon.png";
            await Navigation.PushAsync(new EquipPage());
        }
        else if (result == "Close")
        {

        }


        //if (label.Text != "Comfirm?")
        //{
        //    if (PlayerData.CoinCount >= int.Parse(label.Text))
        //    {
        //        PlayerData.CoinCount -= int.Parse(label.Text);
        //        CoinCountLabel.Text = PlayerData.CoinCount.ToString();
        //        label.Text = "Comfirm?";
        //    }
        //    else
        //    {
        //        await DisplayAlert("Not enough coins", "You need " + (int.Parse(label.Text) - PlayerData.CoinCount) + " more coins to unlock this skin", "OK");
        //    }
        //}
        //else
        //{
        //    PlayerData.CoinCount -= int.Parse(label.Text);
        //    CoinCountLabel.Text = PlayerData.CoinCount.ToString();

        //    PlayerData.UnlockedSkins.Add(num);  
        //}
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CampaignPage());
    }
}