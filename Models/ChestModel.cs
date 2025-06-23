using MazeEscape.Models;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MazeEscape
{

    // Interface
    //     Draw(Grid)
    //     Animation       (continuous running)
    //     OnClicked

    // Lootbox: random goods
    // Chest: coin reward
    // Skin Unlock: Get a skin
    // Special Power-Up: A unique, unpurchasable power-up
    // Power-up Bundle: many normal power-ups
    // Portal: Gives access to the next world
    // Colored Key: Unlocks a gate down the line
    // Easter Eggs: Seceret chests that under specific conditions turns into an easter egg.

    public interface IReward
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Opened { get; set; }
        public bool Unlocked { get; set; }
        public ICommand? ButtonCommand { set; get; }


        void Draw(Grid gridView, AbsoluteLayout absLayout, Label coinCountLabel);
        void Animation();
        void CancelAnimation();
        Task OnClicked(object sender, EventArgs e, AbsoluteLayout? absLayout, Label? coinCountLabel);

    }


    public class ChestModel : IReward
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Opened { get; set; }
        public bool Unlocked { get; set; }
        public string Name { get; set; }
        public int WorldNumber { get; set; }
        public ImageButton? imageButton { get; set; }
        public ICommand? ButtonCommand { set; get; }


        public ChestModel(int world, int x, int y, string name)
        {
            WorldNumber = world;
            X = x;
            Y = y;
            Name = name;
        }

        List<String> pictureURLs = ["chest.png", "world2_chest.png"];
        public void Draw(Grid gridView, AbsoluteLayout absLayout, Label coinCountLabel)
        {
            if (true) //(App.PlayerData.HighestAreaUnlocked >= chest.Area)
            {
                imageButton = new ImageButton
                {

                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 60,
                    WidthRequest = 60,
                    Source = pictureURLs[WorldNumber - 1],
                    Background = Colors.Transparent,
                };
                if (Opened) { imageButton.Source = "world2_opened_chest.png"; }

                if (App.PlayerData.Worlds[WorldNumber - 1].UnlockedMazesNumbers.Contains(Name))
                {
                    Unlocked = true;
                }

                imageButton.Clicked += async (s, e) =>
                {
                    await OnClicked(s, e, absLayout, coinCountLabel);
                };

                Grid.SetRow(imageButton, Y);
                Grid.SetColumn(imageButton, X);
                gridView.Add(imageButton);
            }


        }

        public void Animation()
        {
            int degree_of_rotation = 10;
            if (imageButton == null) return;

            double rand_duration = new Random().Next(10, 20);
            double rand_start_skew = new Random().Next(1, (int)(rand_duration - 2));
            double rand_start = rand_start_skew / rand_duration;
            //Task.Delay(rand_start * 1000).Wait(); // Random delay to simulate different animation start times

            new Animation
            {
                { rand_start, rand_start + (0.5 / rand_duration), new Animation (v => imageButton.Rotation = v, 0, +degree_of_rotation) },
                { rand_start + (0.5 / rand_duration), rand_start + (1.5 / rand_duration), new Animation (v => imageButton.Rotation = v, +degree_of_rotation, -degree_of_rotation) },
                { rand_start + (1.5 / rand_duration), rand_start + (2 / rand_duration), new Animation (v => imageButton.Rotation = v, -degree_of_rotation, 0) }
            }.Commit(imageButton, "ChildAnimations", 16, (uint)(rand_duration * 1000), null, (v, c) => AnimationIsOver(true, false), () => true);
        }

        public void CancelAnimation()
        {
            imageButton?.AbortAnimation("ChildAnimations");
        }
        public void AnimationIsOver(bool isCompleted, bool isCancelled)
        {
            if (isCompleted)
            {
                // Animation completed successfully
                // Handle any post-animation logic here
            }
            else if (isCancelled)
            {
                // Animation was cancelled
                // Handle any cancellation logic here
            }
        }

        public async Task OnClicked(object sender, EventArgs e, AbsoluteLayout absLayout, Label coinCountLabel)
        {
            if (Unlocked && !Opened)
            {

                Opened = true;
                await OpenChest(sender, e, absLayout);
                coinCountLabel.Text = App.PlayerData.CoinCount.ToString();
            }
            else
            {

                await OnLockedChestImageButtonClicked(sender, e);

            }
        }

        public async Task OpenChest(object sender, EventArgs e, AbsoluteLayout absLayout)
        {
            CancelAnimation();

            ImageButton self = (ImageButton)sender;
            _ = self.ScaleTo(1.1, 1000);
            Random rnd = new Random();
            int coinsEarned = rnd.Next(100, 1000);
            App.PlayerData.CoinCount += coinsEarned;
            App.PlayerData.Save();
            ImageButton chest = (ImageButton)sender;

            await self.FadeTo(0, 1000);


            Label coinsEarnedLabel = new Label
            {
                Text = coinsEarned.ToString(),
                TextColor = Colors.Gold,
                FontSize = 16,
                HorizontalTextAlignment = TextAlignment.Center,
            };
            absLayout.Add(coinsEarnedLabel, new Rect(chest.X, chest.Y - 10, 50, 110));

            chest.Source = "coin_pile.png";
            await self.FadeTo(1, 500);

            _ = coinsEarnedLabel.FadeTo(0, 10000);
        }

        public async Task OnLockedChestImageButtonClicked(object sender, EventArgs e)
        {
            ImageButton self = (ImageButton)sender;
            await self.ScaleTo(1.1, 500);
            await self.ScaleTo(1, 500);
        }
    }

    public class PortalModel : IReward
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Opened { get; set; }
        public bool Unlocked { get; set; }
        public string Name { get; set; }
        public int WorldNumber { get; set; }
        public int StarsNeeded { get; set; }
        public Image? imageButton { get; set; }
        public ICommand?ButtonCommand { set; get; }


        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();



        public PortalModel(int world, int x, int y, string name, int stars_needed)
        {
            WorldNumber = world;
            X = x;
            Y = y;
            Name = name;
            StarsNeeded = stars_needed;
        }

        public void Draw(Grid gridView, AbsoluteLayout absLayout, Label coinCountLabel)
        {

            imageButton = new Image
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Aspect = Aspect.Fill,
                Source = "portal_purple.gif",
                Background = Colors.Transparent,
                IsAnimationPlaying = true,

            };

            if (App.PlayerData.Worlds[WorldNumber - 1].UnlockedMazesNumbers.Contains(Name) && App.PlayerData.Worlds[WorldNumber - 1].StarCount >= StarsNeeded)
            {
                Unlocked = true;
            }

            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                await OnClicked(s, e);
            };

            imageButton.GestureRecognizers.Add(tapGestureRecognizer);


            Border border = new Border
            {
                Stroke = Colors.Black,
                Margin = 5,
                StrokeThickness = 1,
                HeightRequest = 60,
                WidthRequest = 40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new Ellipse
                {
                    //CornerRadius = new CornerRadius(40, 40, 40, 40)
                },
                Content = imageButton,
            };


            Grid.SetRow(border, Y);
            Grid.SetColumn(border, X);
            gridView.Add(border);


        }

        public void Animation()
        {

        }
        public void CancelAnimation()
        {
            imageButton?.AbortAnimation("ChildAnimations");
            //imageButton.CancelAnimations();
        }

        public async Task AnimationIsOver(bool isCompleted, bool isCancelled, int unlocked_num)
        {
            if (isCompleted)
            {
                // Animation completed successfully
                // Handle any post-animation logic here
                await Application.Current.MainPage.Navigation.PushAsync(new WorldsPage(unlocked_num));
            }
            else if (isCancelled)
            {
                // Animation was cancelled
                // Handle any cancellation logic here
                await Application.Current.MainPage.Navigation.PushAsync(new WorldsPage(unlocked_num));
            }
        }

        public async Task OnClicked(object sender, EventArgs e, AbsoluteLayout? absLayout = null, Label? coinCountLabel = null)
        {
            if (Unlocked && !Opened)
            {
                Opened = true;
                await OpenPortalClicked(sender, e);
            }
            else if (Unlocked && Opened)
            {
                // Go to next world
                CancelAnimation();
                //await Application.Current.MainPage.Navigation.PushAsync(new WorldsPage());
            }
            else
            {
                await LockedPortalClicked(sender, e);
            }
        }

        public async Task OpenPortalClicked(object sender, EventArgs e)
        {
            int unlocked_num = -1;
            foreach (CampaignWorld w in App.PlayerData.Worlds)
            {
                if (w.Locked)
                {
                    //w.Locked = false;
                    unlocked_num = w.WorldID;
                    break;
                }
            }
            //App.PlayerData.Save();

            new Animation
            {
                { 0, 1, new Animation (v => imageButton.Scale = v, 1, 20) },
                { 0, 1, new Animation(v => imageButton.Rotation = v, 0, 360) },
            }.Commit(imageButton, "ChildAnimations", 16, 3000, null, async (v, c) => await AnimationIsOver(true, false, unlocked_num), () => false);

            imageButton.Scale = 1;
            imageButton.Rotation = 0;
        }

        public async Task LockedPortalClicked(object sender, EventArgs e)
        {
            Image self = (Image)sender;
            await self.ScaleTo(1.1, 500);
            await self.ScaleTo(1, 500);
        }
    }


    public class SkinUnlockModel : IReward
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Opened { get; set; }
        public bool Unlocked { get; set; }
        public string Name { get; set; }
        public int WorldNumber { get; set; }
        public SkinModel Skin { get; set; }
        public ImageButton? imageButton { get; set; }
        public ICommand? ButtonCommand { set; get; }


        public SkinUnlockModel(int world, int x, int y, string name, string skin_name)
        {
            WorldNumber = world;
            X = x;
            Y = y;
            Name = name;

            Skin = App.PlayerData.UnlockedSkins.Find(x => x.Name == skin_name);

        }
        public void Draw(Grid gridView, AbsoluteLayout absLayout, Label coinCountLabel)
        {
            imageButton = new ImageButton
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit,
                HeightRequest = 60,
                WidthRequest = 60,
                Source = $"{Skin.ImageUrl}_icon.png",
                Background = Colors.Transparent,
            };
            if (App.PlayerData.Worlds[WorldNumber - 1].UnlockedMazesNumbers.Contains(Name))
            {
                Unlocked = true;
            }
            imageButton.Clicked += async (s, e) =>
            {
                await OnClicked(s, e, absLayout, coinCountLabel);
            };
            Grid.SetRow(imageButton, Y);
            Grid.SetColumn(imageButton, X);
            gridView.Add(imageButton);
        }
        public void Animation()
        {
            int degree_of_rotation = 10;
            if (imageButton == null) return;
            double rand_duration = new Random().Next(10, 15);
            double rand_start_skew = new Random().Next(1, (int)(rand_duration - 2));
            double rand_start = rand_start_skew / rand_duration;

            new Animation
            {
                { rand_start, rand_start + (0.25 / rand_duration), new Animation (v => imageButton.Rotation = v, 0, +degree_of_rotation) },
                { rand_start + (0.25 / rand_duration), rand_start + (0.75 / rand_duration), new Animation (v => imageButton.Rotation = v, +degree_of_rotation, -degree_of_rotation) },
                { rand_start + (0.75 / rand_duration), rand_start + (1 / rand_duration), new Animation (v => imageButton.Rotation = v, -degree_of_rotation, 0) },
                { rand_start + (0.1 / rand_duration), rand_start + (1.25 / rand_duration), new Animation (v => imageButton.Rotation = v, 0, +degree_of_rotation) },
                { rand_start + (1.25 / rand_duration), rand_start + (1.75 / rand_duration), new Animation (v => imageButton.Rotation = v, +degree_of_rotation, -degree_of_rotation) },
                { rand_start + (1.75 / rand_duration), rand_start + (2 / rand_duration), new Animation (v => imageButton.Rotation = v, -degree_of_rotation, 0) }
                }.Commit(imageButton, "ChildAnimations", 16, (uint)(rand_duration * 1000), null, null, () => true);
        }

        public void CancelAnimation()
        {
            imageButton?.AbortAnimation("ChildAnimations");
        }

        public async Task OnClicked(object sender, EventArgs e, AbsoluteLayout absLayout, Label coinCountLabel)
        {
            if (Unlocked && !Opened)
            {
                Opened = true;
                await OpenSkinUnlock(sender, e, absLayout);

                await imageButton.ScaleTo(0, 250);
                await imageButton.ScaleTo(1, 250);

            }else if (Unlocked && Opened)
            {
                await imageButton.ScaleTo(0.5, 250);
                await imageButton.ScaleTo(1, 250);
            }
            else
            {
                await OnLockedSkinUnlockImageButtonClicked(sender, e);
            }
        }

        public async Task OpenSkinUnlock(object sender, EventArgs e, AbsoluteLayout absLayout)
        {
            CancelAnimation();
            await imageButton.ScaleTo(0, 250);
            await imageButton.ScaleTo(1, 250);

            Skin.IsUnlocked = true;
            App.PlayerData.Save();
        }

        public async Task OnLockedSkinUnlockImageButtonClicked(object sender, EventArgs e)
        {
            ImageButton self = (ImageButton)sender;
            await self.ScaleTo(1.1, 500);
            await self.ScaleTo(1, 500);
        }
    }

    public class KeyModel : IReward
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Opened { get; set; }
        public bool Unlocked { get; set; }
        public string Name { get; set; }
        public string ConnectsTo { get; set; }
        public string ImageUrl { get; set; }
        public int WorldNumber { get; set; }
        public ImageButton? imageButton { get; set; }
        public ICommand? ButtonCommand { set; get; }

        public KeyModel(int world, int x, int y, string name, string connectsTo, string imgUrl)
        {
            WorldNumber = world;
            X = x;
            Y = y;
            Name = name;
            ConnectsTo = connectsTo;
            ImageUrl = imgUrl;

        }
        public void Draw(Grid gridView, AbsoluteLayout absLayout, Label coinCountLabel)
        {
            imageButton = new ImageButton
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit,
                HeightRequest = 60,
                WidthRequest = 60,
                Source = $"{ImageUrl}.png",
                Background = Colors.Transparent,
                Command = ButtonCommand,
            };
            if (App.PlayerData.Worlds[WorldNumber - 1].UnlockedMazesNumbers.Contains(Name))
            {
                Unlocked = true;
            }
            imageButton.Clicked += async (s, e) =>
            {
                await OnClicked(s, e, absLayout, coinCountLabel);
            };
            Grid.SetRow(imageButton, Y);
            Grid.SetColumn(imageButton, X);
            gridView.Add(imageButton);
        }
        public void Animation()
        {
            int degree_of_rotation = 10;
            if (imageButton == null) return;
            double rand_duration = new Random().Next(5, 10);
            double rand_start_skew = new Random().Next(1, (int)(rand_duration - 2));
            double rand_start = rand_start_skew / rand_duration;

            new Animation
            {
                { rand_start, rand_start + (0.25 / rand_duration), new Animation (v => imageButton.Rotation = v, 0, +degree_of_rotation) },
                { rand_start + (0.25 / rand_duration), rand_start + (0.75 / rand_duration), new Animation (v => imageButton.Rotation = v, +degree_of_rotation, -degree_of_rotation) },
                { rand_start + (0.75 / rand_duration), rand_start + (1 / rand_duration), new Animation (v => imageButton.Rotation = v, -degree_of_rotation, 0) },
                { rand_start + (0.1 / rand_duration), rand_start + (1.25 / rand_duration), new Animation (v => imageButton.Rotation = v, 0, +degree_of_rotation) },
                { rand_start + (1.25 / rand_duration), rand_start + (1.75 / rand_duration), new Animation (v => imageButton.Rotation = v, +degree_of_rotation, -degree_of_rotation) },
                { rand_start + (1.75 / rand_duration), rand_start + (2 / rand_duration), new Animation (v => imageButton.Rotation = v, -degree_of_rotation, 0) },
                { 0, 0.25, new Animation (v => imageButton.TranslationX = v, 0, +5) },
                { 0.25, 0.75, new Animation (v => imageButton.Rotation = v, -5, +5) },
                { 0.75, 1, new Animation (v => imageButton.Rotation = v, -5, 0) },
                }.Commit(imageButton, "ChildAnimations", 16, (uint)(rand_duration * 1000), null, null, () => true);

        }

        public void CancelAnimation()
        {
            imageButton?.AbortAnimation("ChildAnimations");
        }

        public async Task OnClicked(object sender, EventArgs e, AbsoluteLayout absLayout, Label coinCountLabel)
        {
            if (Unlocked && !Opened)
            {
                Opened = true;
                await OpenSkinUnlock(sender, e, absLayout);

                await imageButton.ScaleTo(0, 250);
                await imageButton.ScaleTo(1, 250);

            }
            else if (Unlocked && Opened)
            {
                await imageButton.ScaleTo(0.5, 250);
                await imageButton.ScaleTo(1, 250);
            }
            else
            {
                await OnLockedSkinUnlockImageButtonClicked(sender, e);
            }
        }

        public async Task OpenSkinUnlock(object sender, EventArgs e, AbsoluteLayout absLayout)
        {
            CancelAnimation();
            await imageButton.ScaleTo(0, 250);
            await imageButton.ScaleTo(1, 250);

            App.PlayerData.Worlds[WorldNumber - 1].UnlockedMazesNumbers.Add(ConnectsTo);
            App.PlayerData.Save();

            ButtonCommand = null;
        }

        public async Task OnLockedSkinUnlockImageButtonClicked(object sender, EventArgs e)
        {
            ImageButton self = (ImageButton)sender;
            await self.ScaleTo(1.1, 500);
            await self.ScaleTo(1, 500);
        }
    }



}
