using CommunityToolkit.Maui.Views;
using MazeEscape.Drawables;
using MazeEscape.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using SharpHook;
using SharpHook.Native;
using SharpHook.Reactive;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;



namespace MazeEscape;

public partial class CampaignLevelPage : ContentPage
{
    public event EventHandler<CampaignLevel>? LevelSaved;
    CampaignLevel Level;

    public double MazeWindowWidth = Application.Current.MainPage.Width * 0.9;
    public double MazeWindowHeight = Application.Current.MainPage.Height * 0.8;

    MazeModel Maze = new MazeModel();

    PlayerDrawable drawer;
    SimpleReactiveGlobalHook? hook;

    //private DateTime timeStarted;
    public int numberOfMoves = 0;
    private bool running;
    TimeSpan TotalTime;

    private System.Timers.Timer _timer;
    private DateTime timeStarted;

    public void StartTimer()
    {
        timeStarted = DateTime.Now;
        _timer = new System.Timers.Timer(10); // Interval in milliseconds
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        running = true;
    }

    private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        if (!running)
        {
            TotalTime = DateTime.Now - timeStarted;
            _timer.Stop();
            return;
        }

        TimeSpan timePassed = DateTime.Now - timeStarted;
        Dispatcher.Dispatch(new Action(() =>
        {
            labelTimer.Text = Level.ThreeStarTime.ToString() + ":  " + Math.Round(timePassed.TotalSeconds, 1).ToString();
            moveNumberText.Text = Level.TwoStarMoves.ToString() + ":  " + numberOfMoves.ToString();

            if (Maze.Player.X == Maze.End.Item1 && Maze.Player.Y == Maze.End.Item2)
            {
                TotalTime = DateTime.Now - timeStarted;
                CompletedMaze();
            }
        }));
    }

    StateContainerViewModel stateContainerModel = new StateContainerViewModel();
    public CampaignLevelPage(CampaignLevel level)
    {
        InitializeComponent();

        PageAbsoluteLayout.BindingContext = stateContainerModel;
        stateContainerModel.CurrentState = "Loading";

        Level = level;

        AbsoluteLayout.SetLayoutBounds(main_absolute_layout, new Rect(0.45, 0.6, MazeWindowWidth, MazeWindowHeight));
        AbsoluteLayout.SetLayoutFlags(main_absolute_layout, AbsoluteLayoutFlags.PositionProportional);

        InitializeReactiveKeyboard();

        drawer = new PlayerDrawable();
        mazeGraphicsView.Drawable = drawer;
        drawer.Initialize();

        InitializeMaze();

        DrawMaze("line");

        UpdatePlayerDrawerPosition();

        AddSwipeGestures();

        Task.Delay(1000).ContinueWith(t => StartPlay());
    }

    public void StartPlay()
    {
        //var list = PlayerData.levelDatabase.GetItemsAsync().Result;
        stateContainerModel.CurrentState = "Success";
        int time_to_wait = Level.Width * Level.Height * 5;
        Task.Delay(time_to_wait).ContinueWith(t => StartTimer());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        extraTimePowerUpLabel.Text = PlayerData.ExtraTimesOwned.ToString();
        extraMovesPowerUpLabel.Text = PlayerData.ExtraMovesOwned.ToString();
        hintPowerUpLabel.Text = PlayerData.HintsOwned.ToString();
        //CoinCountLabel.Text = PlayerData.CoinCount.ToString();

    }

    void OnSwiped(object sender, SwipedEventArgs e)
    {

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                MoveLeft();
                break;
            case SwipeDirection.Right:
                MoveRight();
                break;
            case SwipeDirection.Up:
                MoveUp();
                break;
            case SwipeDirection.Down:
                MoveDown();
                break;
        }

    }

    public void DrawMaze(string wall_type = "line")
    {
        Dictionary<int, Color> dict_int_to_color = new Dictionary<int, Color>();
        dict_int_to_color.Add(0, Colors.White);
        dict_int_to_color.Add(1, Colors.Black);
        dict_int_to_color.Add(2, Colors.GreenYellow);
        dict_int_to_color.Add(3, Colors.IndianRed);
        dict_int_to_color.Add(4, Colors.LightGoldenrodYellow); // For Debugging Purposes

        Dictionary<string, Color> dict_str_to_color = new Dictionary<string, Color>();
        dict_str_to_color.Add("-", Colors.White);
        dict_str_to_color.Add("#", Colors.Black);
        dict_str_to_color.Add("s", Colors.Green);
        dict_str_to_color.Add("F", Colors.Red);

        double cell_width = MazeWindowWidth / Maze.Width;
        double cell_height = MazeWindowHeight / Maze.Height;

        int line_thickness = 4;


        // Color in the Start and Finish Squares before drawing lines
        (int w, int h) = Maze.Start;
        //main_absolute_layout.Add(new BoxView
        //{
        //    Color = dict_int_to_color[2]

        //}, new Rect(w * cell_width + (line_thickness / 2), h * cell_height + (line_thickness / 2), cell_width - line_thickness, cell_height - line_thickness));

        (w, h) = Maze.End;
        main_absolute_layout.Add(new BoxView
        {
            Color = dict_int_to_color[3]

        }, new Rect(w * cell_width + (line_thickness / 2), h * cell_height + (line_thickness / 2), cell_width - line_thickness, cell_height - line_thickness));



        for (h = 0; h < Maze.Height; h++)
        {
            for (w = 0; w < Maze.Width; w++)
            {
                if (wall_type == "rect")
                {
                    mazeGrid.Add(new BoxView
                    {
                        Color = dict_int_to_color[Maze.Cells[h][w].Value]

                    }, w, h);
                }
                if (wall_type == "line")
                {
                    //if (Maze.Cells[h][w].Value != 0)
                    //{
                    //    main_absolute_layout.Add(new BoxView
                    //    {
                    //        Color = dict_int_to_color[Maze.Cells[h][w].Value]

                    //    }, new Rect(w * cell_width + (line_thickness / 2), h * cell_height + (line_thickness / 2), cell_width - line_thickness, cell_height - line_thickness));
                    //}


                    if (Maze.Cells[h][w].North) // Draw North Wall is cell.North is true
                    {
                        main_absolute_layout.Add(new BoxView
                        {
                            Color = dict_int_to_color[1],

                        }, new Rect(w * cell_width - (line_thickness / 2), h * cell_height - (line_thickness / 2), cell_width + line_thickness, line_thickness));
                    }
                    if (Maze.Cells[h][w].East) // Draw East Wall if cell.East is true
                    {
                        main_absolute_layout.Add(new BoxView
                        {
                            Color = dict_int_to_color[1],

                        }, new Rect((w + 1) * cell_width - (line_thickness / 2), h * cell_height - (line_thickness / 2), line_thickness, cell_height + line_thickness));
                    }

                    // Draw South Wall if h == Maze.Height-1
                    if (h == Maze.Height - 1)
                    {
                        main_absolute_layout.Add(new BoxView
                        {
                            Color = dict_int_to_color[1],

                        }, new Rect(w * cell_width - (line_thickness / 2), (h + 1) * cell_height - (line_thickness / 2), cell_width + line_thickness, line_thickness));
                    }

                    // Draw West Wall if w == 0
                    if (w == 0)
                    {
                        main_absolute_layout.Add(new BoxView
                        {
                            Color = dict_int_to_color[1],

                        }, new Rect(w * cell_width - (line_thickness / 2), h * cell_height, line_thickness, cell_height));
                    }

                }
            }
        }
    }

    public void InitializeMaze()
    {
        Maze.MazeGenerationDelegateList[Level.LevelType](Level.Width, Level.Height);

        Level.TwoStarMoves = Math.Max((int)(Maze.PathLength * 1.01), Maze.PathLength + 5);
        if (Level.Width * Level.Height < 100)
        {
            Level.ThreeStarTime = Maze.PathLength / 2;
        }
        else
        {
            Level.ThreeStarTime = Maze.PathLength / (1 + (int)(Maze.PathLength / 100));
        }

        drawer.WindowWidth = MazeWindowWidth;
        drawer.WindowHeight = MazeWindowHeight;

        drawer.MazeHeight = Maze.Height;
        drawer.MazeWidth = Maze.Width;
    }

    public async void InitializeReactiveKeyboard()
    {
        hook = new SimpleReactiveGlobalHook(GlobalHookType.Keyboard, runAsyncOnBackgroundThread: true);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcUp).Subscribe(OnUpArrowKeyPressed);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcDown).Subscribe(OnDownArrowKeyPressed);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcLeft).Subscribe(OnLeftArrowKeyPressed);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcRight).Subscribe(OnRightArrowKeyPressed);

        await hook.RunAsync();
    }

    public void AddSwipeGestures()
    {
        if (Level != null)
        {
            SwipeGestureRecognizer leftSwipeGesture = new() { Direction = SwipeDirection.Left };
            leftSwipeGesture.Swiped += OnSwiped;
            SwipeGestureRecognizer rightSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            rightSwipeGesture.Swiped += OnSwiped;
            SwipeGestureRecognizer upSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            upSwipeGesture.Swiped += OnSwiped;
            SwipeGestureRecognizer downSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            downSwipeGesture.Swiped += OnSwiped;

            mazeGraphicsView.GestureRecognizers.Add(leftSwipeGesture);
            mazeGraphicsView.GestureRecognizers.Add(rightSwipeGesture);
            mazeGraphicsView.GestureRecognizers.Add(upSwipeGesture);
            mazeGraphicsView.GestureRecognizers.Add(downSwipeGesture);
        }
    }

    public void RedrawPlayer()
    {
        mazeGraphicsView.Invalidate();
    }


    public void MoveLeft()
    {
        if (Maze.Player.X > 0 && !Maze.Cells[Maze.Player.Y][Maze.Player.X - 1].East)
        {

            Maze.Player.X--;
            UpdatePlayerDrawerPosition();
            RedrawPlayer();
        }
    }
    public void MoveRight()
    {
        if (Maze.Player.X < Maze.Width - 1 && !Maze.Cells[Maze.Player.Y][Maze.Player.X].East)
        {

            Maze.Player.X++;
            UpdatePlayerDrawerPosition();
            RedrawPlayer();
        }
    }
    public void MoveUp()
    {
        if (Maze.Player.Y > 0 && !Maze.Cells[Maze.Player.Y][Maze.Player.X].North)
        {

            Maze.Player.Y--;
            UpdatePlayerDrawerPosition();
            RedrawPlayer();
        }
    }
    public void MoveDown()
    {
        if (Maze.Player.Y < Maze.Height - 1 && !Maze.Cells[Maze.Player.Y + 1][Maze.Player.X].North)
        {

            Maze.Player.Y++;
            UpdatePlayerDrawerPosition();
            RedrawPlayer();
        }
    }

    public void OnUpClicked(object sender, EventArgs e)
    {
        MoveUp();
    }
    public void OnDownClicked(object sender, EventArgs e)
    {
        MoveDown();
    }
    public void OnLeftClicked(object sender, EventArgs e)
    {
        MoveLeft();
    }
    public void OnRightClicked(object sender, EventArgs e)
    {
        MoveRight();
    }

    public void OnUpArrowKeyPressed(KeyboardHookEventArgs args)
    {
        MoveUp();
    }
    public void OnDownArrowKeyPressed(KeyboardHookEventArgs args)
    {
        MoveDown();
    }
    public void OnLeftArrowKeyPressed(KeyboardHookEventArgs args)
    {
        MoveLeft();
    }
    public void OnRightArrowKeyPressed(KeyboardHookEventArgs args)
    {
        MoveRight();
    }


    public void UpdatePlayerDrawerPosition()
    {
        drawer.XPos = Maze.Player.X;
        drawer.YPos = Maze.Player.Y;

        numberOfMoves++;

        //if (Maze.Player.X == Maze.End.Item1 && Maze.Player.Y == Maze.End.Item2)
        //{
        //    CompletedMaze();
        //}
    }

    public async void CompletedMaze()
    {
        // Score like time, number of moves, or amount of false steps
        // Award 0-3 stars
        // Button to retry Maze or exit back to previous screen
        running = false;
        hook?.Dispose();

        double time = TotalTime.TotalSeconds;
        int coinsEarned = 0;

        char[] charsToTrim = { 'c', 'b' };
        int num = Int32.Parse(Level.LevelNumber.Trim(charsToTrim));

        if (Level.Star1 == false)
        {
            PlayerData.UnlockedMazesNumbers.AddRange(PlayerData.LevelConnectsToDictionary[Level.LevelNumber]);
            PlayerData.StarCount++;
            Level.NumberOfStars++;
            coinsEarned += num;
        }

        Level.Star1 = true;

        if (time < Level.ThreeStarTime && !Level.Star3)
        {
            Level.Star3 = true;
            Level.NumberOfStars++;
            PlayerData.StarCount++;
            
            coinsEarned += num * 3;
        }


        if (numberOfMoves <= Level.TwoStarMoves && !Level.Star2)
        {
            Level.Star2 = true;
            Level.NumberOfStars++;
            PlayerData.StarCount++;
            coinsEarned += num * 2;
        }
        
        if (time < Level.BestTime.TotalSeconds)
        {
            Level.BestTime = TotalTime;
        }
        if (numberOfMoves < Level.BestMoves)
        {
            Level.BestMoves = numberOfMoves;
        }

        PlayerData.CoinCount += coinsEarned;

        PlayerData.Save();

        await main_absolute_layout.FadeTo(0.2, 1000);
        LevelSaved?.Invoke(this, Level);

        try
        {
            mazeGraphicsView.IsGameOver = false;
            var result = await this.ShowPopupAsync(new CampaignMazeFinishedPopupPage(TotalTime, numberOfMoves, Level, coinsEarned), CancellationToken.None);
            if (result == "Retry")
            {
                var page = new CampaignLevelPage(Level);
                page.LevelSaved += async (obj, copyOfLevel) => { // Any variables that may be changed
                    Level.BestTime = copyOfLevel.BestTime;
                    Level.BestMoves = copyOfLevel.BestMoves;
                    Level.Completed = copyOfLevel.Completed;
                    Level.Star1 = copyOfLevel.Star1;
                    Level.Star2 = copyOfLevel.Star2;
                    Level.Star3 = copyOfLevel.Star3;
                    Level.NumberOfStars = copyOfLevel.NumberOfStars;
                    await PlayerData.levelDatabase.SaveLevelAsync(Level);
                };
                await Navigation.PushAsync(page);
            }
            else if (result == "Close")
            {
                await Navigation.PushAsync(new CampaignPage());
            }
            else if (result == "Next Level")
            {
                CampaignLevel next_level = await PlayerData.levelDatabase.GetItemAsync(PlayerData.LevelConnectsToDictionary[Level.LevelNumber][0]);
                var page = new CampaignLevelPage(next_level);
                page.LevelSaved += async (obj, copyOfLevel) => { // Any variables that may be changed
                    next_level.BestTime = copyOfLevel.BestTime;
                    next_level.BestMoves = copyOfLevel.BestMoves;
                    next_level.Completed = copyOfLevel.Completed;
                    next_level.Star1 = copyOfLevel.Star1;
                    next_level.Star2 = copyOfLevel.Star2;
                    next_level.Star3 = copyOfLevel.Star3;
                    next_level.NumberOfStars = copyOfLevel.NumberOfStars;
                    await PlayerData.levelDatabase.SaveLevelAsync(next_level);
                };
                await Navigation.PushAsync(page);
            }
        }
        catch (Exception ex)
        {
            mazeGraphicsView.IsGameOver = true;
        }

    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CampaignPage());
    }

    public async void OnShopButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShopPage());
    }

    public async void OnHintButtonClicked(object sender, EventArgs e)
    {
        if (PlayerData.HintsOwned > 0)
        {
            PlayerData.HintsOwned--;
            hintPowerUpLabel.Text = PlayerData.HintsOwned.ToString();
            //Maze.Path.Find()
        }
        else
        {
            await DisplayAlert("No Hints", "You do not have any hints left", "OK");
        }
    }

    public async void OnExtraTimeButtonClicked(object sender, EventArgs e)
    {
        if (PlayerData.ExtraTimesOwned > 0)
        {
            PlayerData.ExtraTimesOwned--;
            extraTimePowerUpLabel.Text = PlayerData.ExtraTimesOwned.ToString();
            Level.ThreeStarTime += 10;
            labelTimer.TextColor = Colors.Green;
            await labelTimer.ScaleTo(1.2, 800);
            await labelTimer.ScaleTo(1, 800);
            labelTimer.TextColor = Colors.White;

        }
        else
        {
            await DisplayAlert("No Extra Time", "You do not have any extra time left", "OK");
        }
    }

    public async void OnExtraMovesButtonClicked(object sender, EventArgs e)
    {
        if (PlayerData.ExtraMovesOwned > 0)
        {
            PlayerData.ExtraMovesOwned--;
            extraMovesPowerUpLabel.Text = PlayerData.ExtraMovesOwned.ToString();
            Level.TwoStarMoves += 10;
            moveNumberText.TextColor = Colors.Gold;
            await moveNumberText.ScaleTo(1.2, 800);
            await moveNumberText.ScaleTo(1, 800);
            moveNumberText.TextColor = Colors.White;
        }
        else
        {
            await DisplayAlert("No Extra Moves", "You do not have any extra time left", "OK");
        }
    }

    public async void OnResetButtonClicked(object sender, EventArgs e)
    {
        bool ans = await DisplayAlert("Reset", "Are you sure you want to reset the maze?", "Yes", "No");
        if (ans)
        {
            InitializeMaze();
            UpdatePlayerDrawerPosition();
            RedrawPlayer();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        hook?.Dispose();
    }


}

public class StateContainerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    private string _state = "Loading";
    public string CurrentState
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged();
        }
    }
    
    public StateContainerViewModel()
    {
        CurrentState = "Loading";
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}