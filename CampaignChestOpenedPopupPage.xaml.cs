namespace MazeEscape;
using CommunityToolkit.Maui.Views;
using MazeEscape.Models;

public partial class CampaignChestOpenedPopupPage : Popup
{
	public CampaignChestOpenedPopupPage(int coinsEarned)
	{
        InitializeComponent();
        double width = PlayerData.WindowWidth * 0.4;  // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / 4;
        double height = PlayerData.WindowHeight * 0.4; // Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / 4;

        // Set   the size of the popup
        this.Size = new Size(width, height);

        coinLabel.Text = $"{coinsEarned} coins!";
    }
}