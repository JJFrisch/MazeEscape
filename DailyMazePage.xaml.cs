using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Timers;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using MazeEscape.Drawables;
using MazeEscape.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using SharpHook;
using SharpHook.Reactive;
using SharpHook.Native;



namespace MazeEscape;

public partial class DailyMazePage : ContentPage
{

    static ObservableCollection<DailyMazeLevel> monthlyMazes = new ObservableCollection<DailyMazeLevel>();

    public bool loading = false;

    DateTime date_time = DateTime.Now;

    string date;

    string month_year;

    int days_in_this_month; 

    Random rnd = new Random();

    StateContainerViewModel stateContainerModel = new StateContainerViewModel();


    // For Play State
    public event EventHandler<CampaignLevel>? LevelSaved;

    public double MazeWindowWidth = Application.Current.MainPage.Width * 0.9;
    public double MazeWindowHeight = Application.Current.MainPage.Height * 0.8;

    MazeModel Maze = new MazeModel();
    MazeGraphicsView mazeGraphicsView;

    PlayerDrawable drawer;
    SimpleReactiveGlobalHook? hook;

    //private DateTime timeStarted;
    public int numberOfMoves = 0;
    private bool running;
    TimeSpan TotalTime;

    private System.Timers.Timer _timer;
    private DateTime timeStarted;

    public DailyMazePage()
	{
		InitializeComponent();

        stateContainerModel.CurrentState = "Calendar";
        PageAbsoluteLayout.BindingContext = stateContainerModel;


        date = date_time.ToString("d");
        month_year = date_time.ToString("MM-yyyy");
        days_in_this_month = DateTime.DaysInMonth(date_time.Year, date_time.Month);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //await PlayerData.dailyMazeDatabase.DeleteAllLevelsAsync();

        if (monthlyMazes.Count == 0)
        {
            await InitializeDays();
        }

        //await LoadLevelsFromDatabase(month_year);

        await DrawLevels();
    }

    BoxView previous_boxView;
    DailyMazeLevel selected_dailyLevel;

    public async Task InitializeDays()
    {
        if (PlayerData.dailyMazeDatabase.GetItemAsync(date) == null)
        {
            string mazeType = Maze.MazeTypes[rnd.Next(0, Maze.MazeTypes.Count)];

            DailyMazeLevel today = new DailyMazeLevel()
            {
                Width = rnd.Next(12, 30),
                Height = rnd.Next(12, 30),
                LevelType = mazeType,
            };

            await PlayerData.dailyMazeDatabase.AddNewLevelAsync(today);
        }

        for (int i = 1; i <= days_in_this_month; i++)
        {
            DateTime new_date_time = new DateTime(date_time.Year, date_time.Month, i);
            string days_short_date = new_date_time.ToString("d");

            DailyMazeLevel day_item = await PlayerData.dailyMazeDatabase.GetItemAsync(days_short_date);

            if (day_item == null)
            {
                string mazeType = Maze.MazeTypes[rnd.Next(0, Maze.MazeTypes.Count)];

                //DateTime new_date_time = new DateTime(date_time.Year, date_time.Month, i);

                DailyMazeLevel that_day = new DailyMazeLevel()
                {
                    Width = rnd.Next(12, 30),
                    Height = rnd.Next(12, 30),
                    LevelType = mazeType,
                    Date = new_date_time,
                    ShortDate = new_date_time.ToString("d"),
                    Month_Year = new_date_time.ToString("MM-yyyy"),

                };

                await PlayerData.dailyMazeDatabase.AddNewLevelAsync(that_day);
            }
            else
            {
                monthlyMazes.Add(day_item);
            }
        }
    }

    Dictionary<string, int> dayOfWeekDict = new Dictionary<string, int>() ;

    public async Task DrawLevels()
    {
        dayOfWeekDict["Sunday"] = 0;
        dayOfWeekDict["Monday"] = 1;
        dayOfWeekDict["Tuesday"] = 2;
        dayOfWeekDict["Wednesday"] = 3;
        dayOfWeekDict["Thursday"] = 4;
        dayOfWeekDict["Friday"] = 5;
        dayOfWeekDict["Saturday"] = 6;

        int row = 1;
        foreach (DailyMazeLevel dailyLevel in monthlyMazes)
        {
            if (dailyLevel.Date.Day <= DateTime.Now.Day)
            {
                BoxView boxView = new BoxView
                {
                    Opacity = 0,
                    BackgroundColor = Color.FromArgb("8d99ae"),
                    CornerRadius = 10,
                };
                if (date == dailyLevel.ShortDate)
                {
                    boxView.BackgroundColor = Color.FromArgb("588157");
                    boxView.Opacity = 0.6;
                    previous_boxView = boxView;

                    selected_dailyLevel = dailyLevel;
                    playLabelCalendar.Text = $"Play {dailyLevel.Date.Day}";
                    playLabelInfo.Text = $"Play {dailyLevel.Date.Day}";
                }

                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += async (s, e) =>
                {
                    await DailyLevelClicked(dailyLevel, boxView);
                };
                boxView.GestureRecognizers.Add(tapGestureRecognizer);

                Grid.SetRow(boxView, row);
                Grid.SetColumn(boxView, dayOfWeekDict[dailyLevel.Date.DayOfWeek.ToString()]);


                monthGrid.Add(boxView);

                Label label = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Black,
                    Text = dailyLevel.Date.Day.ToString(),
                    FontSize = 16,
                    FontAutoScalingEnabled = true,

                };

                label.GestureRecognizers.Add(tapGestureRecognizer);
                Grid.SetRow(label, row);
                Grid.SetColumn(label, dayOfWeekDict[dailyLevel.Date.DayOfWeek.ToString()]);
                monthGrid.Add(label);

                if (dailyLevel.Status == "Completed"){
                    ImageButton star = new ImageButton()
                    {
                        Aspect = Aspect.AspectFit,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start,
                        WidthRequest = 15,
                        HeightRequest = 15,
                    };
                    star.Clicked += async (s, e) =>
                    {
                        await DailyLevelClicked(dailyLevel, boxView);
                    };
                    Grid.SetRow(star, row);
                    Grid.SetColumn(star, dayOfWeekDict[dailyLevel.Date.DayOfWeek.ToString()]);
                }
            }
            else
            {
                Label label = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    Text = dailyLevel.Date.Day.ToString(),
                    //FontSize = 16,
                    FontAutoScalingEnabled = true,

                };

                Grid.SetRow(label, row);
                Grid.SetColumn(label, dayOfWeekDict[dailyLevel.Date.DayOfWeek.ToString()]);
                monthGrid.Add(label);
            }


            if (dailyLevel.Date.DayOfWeek == DayOfWeek.Saturday)
            {
                row++; // Move to next row on grid
            }

        }

    }

    private void Star_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    public async Task DailyLevelClicked(DailyMazeLevel level, BoxView background)
    {
        selected_dailyLevel = level;

        if (previous_boxView != null)
        {
            _ = previous_boxView.BackgroundColorTo(Colors.Black, 10, 10);
            _ = previous_boxView.FadeTo(0, 500);
        }

        _ = background.FadeTo(0.8, 500);
        await background.BackgroundColorTo(Color.FromArgb("588157"), 16, 500);

        playLabelCalendar.Text = $"Play {level.Date.Day}";
        playLabelInfo.Text = $"Play {level.Date.Day}";

        //if (level.Status == "not_started")
        //{
        //    await DisplayAlert($"{level.ShortDate}", $"{level.Month_Year} {level.LevelID} {level.LevelType} {level.Width} {level.Height}", "OK");
        //    //await Navigation.PushAsync(new ShopPage());
        //}
        //else
        //{
        //    await DisplayAlert("Level Completed", $"You completed this level in {level.CompletetionMoves} moves and {level.CompletetionTime} seconds", "OK");
        //}

        previous_boxView = background;
    }


    private void previousMonthButton_Clicked(object sender, EventArgs e)
    {

    }

    private void nextMonthButton_Clicked(object sender, EventArgs e)
    {

    }

    private async void OnHomeButtonClicked(object sender, EventArgs e)
    {
        hook?.Dispose();
        stateContainerModel.CurrentState = "Calendar";
        await Navigation.PushAsync(new MainPage());
    }

    private void OnInfoButtonClicked(object sender, EventArgs e)
    {
        hook?.Dispose();
        stateContainerModel.CurrentState = "Info";
        //await Navigation.PushAsync(new MainPage());
    }

    private void OnPlayButtonClicked(object sender, EventArgs e)
    {
        //stateContainerModel.CurrentState = "Play";
        PlayCurrentLevel();
        //await Navigation.PushAsync(new MainPage());
    }

    private void OnCalendarButtonClicked(object sender, EventArgs e)
    {
        hook?.Dispose();
        stateContainerModel.CurrentState = "Calendar";
        //await Navigation.PushAsync(new MainPage());
    }





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
            labelTimer.Text = selected_dailyLevel.TimeNeeded.ToString() + ":  " + Math.Round(timePassed.TotalSeconds, 1).ToString();
            //moveNumberText.Text = Level.TwoStarMoves.ToString() + ":  " + numberOfMoves.ToString();

            if (Maze.Player.X == Maze.End.Item1 && Maze.Player.Y == Maze.End.Item2)
            {
                TotalTime = DateTime.Now - timeStarted;
                CompletedMaze();
            }
        }));
    }


    public void PlayCurrentLevel()
    {
        InitializeElements();

        drawer = new PlayerDrawable();
        mazeGraphicsView.Drawable = drawer;
        drawer.Initialize();

        InitializeMaze();

        DrawMaze("line");

        UpdatePlayerDrawerPosition();

        AddSwipeGestures();

        InitializeReactiveKeyboard();

        Task.Delay(1000).ContinueWith(t => StartPlay());
    }

    public void InitializeElements()
    {
        Maze = new MazeModel();

        PageAbsoluteLayout.BindingContext = stateContainerModel;
        stateContainerModel.CurrentState = "Loading";

        main_absolute_layout = new AbsoluteLayout() { };

        AbsoluteLayout.SetLayoutBounds(main_absolute_layout, new Rect(0.45, 0.6, MazeWindowWidth, MazeWindowHeight));
        AbsoluteLayout.SetLayoutFlags(main_absolute_layout, AbsoluteLayoutFlags.PositionProportional);

        BoxView mazeBackgroundBoxView = new BoxView
        {
            BackgroundColor = Colors.White,
        };
        AbsoluteLayout.SetLayoutBounds(mazeBackgroundBoxView, new Rect(0.5, 0.5, 1, 1));
        AbsoluteLayout.SetLayoutFlags(mazeBackgroundBoxView, AbsoluteLayoutFlags.All);

        Image mazeBackground = new Image
        {
            Aspect = Aspect.Fill,
            Opacity = 0.6
        };
        AbsoluteLayout.SetLayoutBounds(mazeBackground, new Rect(0.5, 0.5, 1, 1));
        AbsoluteLayout.SetLayoutFlags(mazeBackground, AbsoluteLayoutFlags.All);

        mazeGraphicsView = new MazeGraphicsView()
        {

        };
        AbsoluteLayout.SetLayoutBounds(mazeGraphicsView, new Rect(0.5, 0.5, 1, 1));
        AbsoluteLayout.SetLayoutFlags(mazeGraphicsView, AbsoluteLayoutFlags.All);

        main_absolute_layout.Add(mazeBackgroundBoxView);
        main_absolute_layout.Add(mazeBackground);
        main_absolute_layout.Add(mazeGraphicsView);

        playAbsoluteLayout.Add(main_absolute_layout);
    }

    public void StartPlay()
    {
        stateContainerModel.CurrentState = "Play";
        int time_to_wait = selected_dailyLevel.Width * selected_dailyLevel.Height * 5;
        Task.Delay(time_to_wait).ContinueWith(t => StartTimer());
    }

    public void InitializeMaze()
    {
        Maze.MazeGenerationDelegateList[selected_dailyLevel.LevelType](selected_dailyLevel.Width, selected_dailyLevel.Height);


        if (selected_dailyLevel.Width * selected_dailyLevel.Height < 100)
        {
            selected_dailyLevel.TimeNeeded = Maze.PathLength / 2;
        }
        else
        {
            selected_dailyLevel.TimeNeeded = Maze.PathLength / (2 + (int)(Maze.PathLength / 100));
        }

        drawer.WindowWidth = MazeWindowWidth;
        drawer.WindowHeight = MazeWindowHeight;

        drawer.MazeHeight = Maze.Height;
        drawer.MazeWidth = Maze.Width;
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
                //if (wall_type == "rect")
                //{
                //    mazeGrid.Add(new BoxView
                //    {
                //        Color = dict_int_to_color[Maze.Cells[h][w].Value]

                //    }, w, h);
                //}
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


    public void UpdatePlayerDrawerPosition()
    {
        drawer.XPos = Maze.Player.X;
        drawer.YPos = Maze.Player.Y;

        numberOfMoves++;
    }

    public void AddSwipeGestures()
    {
        if (selected_dailyLevel != null)
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

    public async void InitializeReactiveKeyboard()
    {
        hook = new SimpleReactiveGlobalHook(GlobalHookType.Keyboard, runAsyncOnBackgroundThread: true);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcUp).Subscribe(OnUpArrowKeyPressed);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcDown).Subscribe(OnDownArrowKeyPressed);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcLeft).Subscribe(OnLeftArrowKeyPressed);
        hook.KeyPressed.Where(e => e.Data.KeyCode == KeyCode.VcRight).Subscribe(OnRightArrowKeyPressed);

        await hook.RunAsync();
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







    public async void CompletedMaze()
    {
        // Score like time, number of moves, or amount of false steps
        // Award 0-3 stars
        // Button to retry Maze or exit back to previous screen
        running = false;
        mazeGraphicsView.IsGameOver = false;
        hook?.Dispose();

        double time = TotalTime.TotalSeconds;


        if (time < selected_dailyLevel.CompletetionTime)
        {
            selected_dailyLevel.CompletetionTime = time;
        }

        selected_dailyLevel.Status = "Completed";

        ImageButton winningStar = new ImageButton
        {
            Source = "full_star.png",
            Opacity = 0,
            Scale = 0.1,
        };
        AbsoluteLayout.SetLayoutBounds(winningStar, new Rect(0.5, 0.5, 1, 1));
        AbsoluteLayout.SetLayoutFlags(winningStar, AbsoluteLayoutFlags.All);

        playAbsoluteLayout.Add(winningStar);

        _ = main_absolute_layout.FadeTo(0.2, 500);

        _ = winningStar.FadeTo(1, 1000);
        await winningStar.ScaleTo(5, 1000);

        await PlayerData.dailyMazeDatabase.SaveLevelAsync(selected_dailyLevel);

        //try
        //{
        //    mazeGraphicsView.IsGameOver = false;
        //    var result = await this.ShowPopupAsync(new CampaignMazeFinishedPopupPage(TotalTime, numberOfMoves, Level, coinsEarned), CancellationToken.None);
        //    if (result == "Retry")
        //    {
        //        var page = new CampaignLevelPage(Level);
        //        page.LevelSaved += async (obj, copyOfLevel) => { // Any variables that may be changed
        //            Level.BestTime = copyOfLevel.BestTime;
        //            Level.BestMoves = copyOfLevel.BestMoves;
        //            Level.Completed = copyOfLevel.Completed;
        //            Level.Star1 = copyOfLevel.Star1;
        //            Level.Star2 = copyOfLevel.Star2;
        //            Level.Star3 = copyOfLevel.Star3;
        //            Level.NumberOfStars = copyOfLevel.NumberOfStars;
        //            await PlayerData.levelDatabase.SaveLevelAsync(Level);
        //        };
        //        await Navigation.PushAsync(page);
        //    }
        //    else if (result == "Close")
        //    {
        //        await Navigation.PushAsync(new CampaignPage());
        //    }
        //    else if (result == "Next Level")
        //    {
        //        CampaignLevel next_level = await PlayerData.levelDatabase.GetItemAsync(PlayerData.LevelConnectsToDictionary[Level.LevelNumber][0]);
        //        var page = new CampaignLevelPage(next_level);
        //        page.LevelSaved += async (obj, copyOfLevel) => { // Any variables that may be changed
        //            next_level.BestTime = copyOfLevel.BestTime;
        //            next_level.BestMoves = copyOfLevel.BestMoves;
        //            next_level.Completed = copyOfLevel.Completed;
        //            next_level.Star1 = copyOfLevel.Star1;
        //            next_level.Star2 = copyOfLevel.Star2;
        //            next_level.Star3 = copyOfLevel.Star3;
        //            next_level.NumberOfStars = copyOfLevel.NumberOfStars;
        //            await PlayerData.levelDatabase.SaveLevelAsync(next_level);
        //        };
        //        await Navigation.PushAsync(page);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    mazeGraphicsView.IsGameOver = true;
        //}

    }









    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        hook?.Dispose();
    }
}