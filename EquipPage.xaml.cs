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



    public void InitializeButtons()
    {
        for (int num=0; num <= 6; num++) { 

            int col = num % 3;
            int row = num / 3;
            string name = $"player_image{num}";

            ImageButton imageButton = new ImageButton
            {
                WidthRequest = 80,
                Source = $"player_image{num}_icon.png",
                CornerRadius = 20,
            };
            imageButton.Clicked += async (s, e) =>
            {
                Equip(name, imageButton);
            };
            Grid.SetColumn(imageButton, col);
            Grid.SetRow(imageButton, row);
            playerImageGrid.Add(imageButton);
        }

    }

    public async Task Equip(string name, ImageButton imageButton)
    {
        PlayerData.PlayerImageName = name+".png";
        characterImage.Source = $"{name}_icon.png";

        await imageButton.FadeTo(0.8, 200);
        await imageButton.FadeTo(1, 200);
    }


    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}