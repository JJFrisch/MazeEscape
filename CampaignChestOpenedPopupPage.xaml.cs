namespace MazeEscape;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;

public partial class CampaignChestOpenedPopupPage : Popup
{
	public CampaignChestOpenedPopupPage(int coinsEarned)
	{
        InitializeComponent();
        // Set the popup size using current page dimensions with safe bounds.
        this.Size = PopupSizing.Calculate(0.5, 0.42, 280, 520, 240, 420);

        coinLabel.Text = $"{coinsEarned} coins!";
        App.PlayerData.CoinCount += coinsEarned;
    }
}