using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Timers;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using MazeEscape.Core.Game;
using MazeEscape.Drawables;
using MazeEscape.Models;
using MazeEscape.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using SharpHook;
using SharpHook.Reactive;
using SharpHook.Native;
using System.Collections.Generic;



namespace MazeEscape;

public partial class DailyMazePage : ContentPage
{

    static ObservableCollection<DailyMazeLevel> monthlyMazes = new ObservableCollection<DailyMazeLevel>();

    public bool loading = false;

    DateTime date_time = DateTime.Now;

    string date;

    int number_of_stars_won = 0;

    int days_in_this_month; 

    Random rnd = new Random();

    StateContainerViewModel stateContainerModel = new StateContainerViewModel();


    // For Play State
    public event EventHandler<CampaignLevel>? LevelSaved;

    public double MazeWindowWidth = Application.Current.MainPage.Width * 0.95;
    public double MazeWindowHeight = Application.Current.MainPage.Height * 0.83;

    MazeModel Maze = new MazeModel();
    private IGameSession? _gameSession;
    MazeGraphicsView mazeGraphicsView;

    PlayerDrawable drawer;
    SimpleReactiveGlobalHook? hook;

    //private DateTime timeStarted;
    public int numberOfMoves = 0;
    private bool running;
    TimeSpan TotalTime;

    private System.Timers.Timer _timer;
    private DateTime timeStarted;

    private bool RestartMonth = false;

    public DailyMazePage()
	{
		InitializeComponent();

        stateContainerModel.CurrentState = "Loading";

        //stateContainerModel.CurrentState = "Calendar";
        PageAbsoluteLayout.BindingContext = stateContainerModel;


        date = date_time.ToString("d");
        //month_year = date_time.ToString("MM-yyyy");
        days_in_this_month = DateTime.DaysInMonth(date_time.Year, date_time.Month);
        streakNumberLabel.Text = number_of_stars_won.ToString();
        prizeLabel1.Text = ((int)days_in_this_month / 2).ToString();
        prizeLabel2.Text = ((int)days_in_this_month).ToString();


    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (App.PlayerData.MostRecentMonth != date_time.ToString("MM-yyyy"))
        {
            RestartMonth = true;
            App.PlayerData.MostRecentMonth = date_time.ToString("MM-yyyy");
            App.PlayerData.MonthPrize1_achieved = false;
            App.PlayerData.MonthPrize2_achieved = false;
            App.PlayerData.Save();
        }

        if (monthlyMazes.Count == 0)
        {
            await InitializeDays();
        }

        await DrawLevels(true);

        stateContainerModel.CurrentState = "Calendar";
    }

    BoxView previous_boxView;
    DailyMazeLevel selected_dailyLevel;

    public async Task InitializeDays()
    {
        if (RestartMonth)
        {
            string mazeType = PickDailyMazeAlgorithm();

            DailyMazeLevel today = new DailyMazeLevel()
            {
                Width = rnd.Next(15, 28),
                Height = rnd.Next(15, 28),
                LevelType = mazeType,
            };

            await App.PlayerData.dailyMazeDatabase.AddNewLevelAsync(today);
        }

        for (int i = 1; i <= days_in_this_month; i++)
        {
            DateTime new_date_time = new DateTime(date_time.Year, date_time.Month, i);
            string days_short_date = new_date_time.ToString("d");

            //DailyMazeLevel day_item = await App.PlayerData.dailyMazeDatabase.GetItemAsync(days_short_date);

            if (RestartMonth)
            {
                string mazeType = PickDailyMazeAlgorithm();

                //DateTime new_date_time = new DateTime(date_time.Year, date_time.Month, i);

                DailyMazeLevel that_day = new DailyMazeLevel()
                {
                    Width = rnd.Next(15, 28),
                    Height = rnd.Next(15, 28),
                    LevelType = mazeType,
                    Date = new_date_time,
                    ShortDate = new_date_time.ToString("d"),
                    Month_Year = new_date_time.ToString("MM-yyyy"),

                };

                await App.PlayerData.dailyMazeDatabase.AddNewLevelAsync(that_day);

                monthlyMazes.Add(that_day);
            }
            else
            {
                DailyMazeLevel day_item = await App.PlayerData.dailyMazeDatabase.GetItemAsync(days_short_date);

                monthlyMazes.Add(day_item);
            }
        }
    }

    Dictionary<string, int> dayOfWeekDict = new Dictionary<string, int>() ;

    public async Task DrawLevels(bool isFirstTime)
    {
        number_of_stars_won = 0;

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
                if (date == dailyLevel.ShortDate && isFirstTime)
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
                

                if (dailyLevel.Status == "Completed"){
                    number_of_stars_won++;

                    ImageButton star = new ImageButton()
                    {
                        Aspect = Aspect.AspectFit,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Center,
                        MinimumHeightRequest = 50,
                        Source = "full_star.png",
                        Opacity = 0.75,
                        Background = Colors.Transparent,
                    };
                    star.Clicked += async (s, e) =>
                    {
                        await DailyLevelClicked(dailyLevel, boxView);
                    };
                    Grid.SetRow(star, row);
                    Grid.SetColumn(star, dayOfWeekDict[dailyLevel.Date.DayOfWeek.ToString()]);
                    monthGrid.Add(star);
                }
                monthGrid.Add(label);
            }
            else
            {
                Label label = new()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.DarkGray,
                    Text = dailyLevel.Date.Day.ToString(),
                    FontSize = 16,
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
        streakNumberLabel.Text = number_of_stars_won.ToString();
        progressStarSlider.WidthRequest = 220 * number_of_stars_won / days_in_this_month;

        if (!App.PlayerData.MonthPrize1_achieved && number_of_stars_won >= ((int)days_in_this_month / 2))
        {
            App.PlayerData.CoinCount += 200;
            App.PlayerData.MonthPrize1_achieved = true;
            await this.ShowPopupAsync(new CampaignChestOpenedPopupPage(200), CancellationToken.None);
            App.PlayerData.Save();
        }
        if (!App.PlayerData.MonthPrize2_achieved && number_of_stars_won >= days_in_this_month)
        {
            App.PlayerData.CoinCount += 500;
            App.PlayerData.MonthPrize2_achieved = true;
            await this.ShowPopupAsync(new CampaignChestOpenedPopupPage(500), CancellationToken.None);
            App.PlayerData.Save();
        }

        // Update claimed-badge visibility
        prize1ClaimedLabel.IsVisible = App.PlayerData.MonthPrize1_achieved;
        prize2ClaimedLabel.IsVisible = App.PlayerData.MonthPrize2_achieved;
        prizeLabel1.TextColor = App.PlayerData.MonthPrize1_achieved ? Colors.Gray : Colors.White;
        prizeLabel2.TextColor = App.PlayerData.MonthPrize2_achieved ? Colors.Gray : Colors.White;

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
        _ = background.BackgroundColorTo(Color.FromArgb("588157"), 16, 500);

        playLabelCalendar.Text = $"Play {level.Date.Day}";
        playLabelInfo.Text = $"Play {level.Date.Day}";

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
        running = false;
        hook?.Dispose();
        stateContainerModel.CurrentState = "Calendar";
        await Navigation.PushAsync(new MainPage());
    }

    private void OnInfoButtonClicked(object sender, EventArgs e)
    {
        running = false;
        hook?.Dispose();
        stateContainerModel.CurrentState = "Info";

        DisplayInfoOnMaze();
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
        running = false;
        hook?.Dispose();
        stateContainerModel.CurrentState = "Calendar";
        //await Navigation.PushAsync(new MainPage());
    }

    Dictionary<string, string> GenerationMethodDescriptions = new Dictionary<string, string>() 
    {
        {"GenerateHuntAndKill", "The Hunt and Kill algorithm is a method for generating mazes that operates by progressively carving pathways through a grid of walls. " +
            "The process begins with the selection of a random starting point within the grid. From there, the algorithm \"hunts\" for an adjacent unvisited cell, breaking " +
            "down the wall to create a passage. This step is repeated until the hunter reaches a dead-end, where further progress is impossible. When the hunter is blocked " +
            "by a dead-end, the algorithm transitions to the \"kill\" phase, where the search for a new point to continue the maze generation begins. This new point is typically " +
            "an unvisited cell that is connected to the previously visited cells. The process of hunting and killing continues until all cells in the grid have been visited and a " +
            "fully connected maze has been created." },

        {"GenerateKruskals", "Kruskal�s algorithm generates a maze by constructing a minimum spanning tree (MST) over a grid graph. Each cell is a vertex, and walls between them " +
            "represent weighted edges. Initially, each cell is its own disjoint set. The algorithm sorts all walls randomly and iterates through them, removing a wall if it connects " +
            "vertices from different sets. This is efficiently managed using the union-find data structure. The process continues until a spanning tree forms, ensuring connectivity " +
            "without cycles. The resulting maze has uniform randomness, as edge selection is globally randomized rather than biased by a growing frontier, like Prim�s algorithm." },

        {"GeneratePrims", "Prim�s algorithm generates a maze using a growing tree approach, treating the grid as a graph where cells are vertices and walls are edges." +
            " It begins with a random single cell and tracks its frontier�walls separating it from unvisited cells. At each step, a random frontier wall is removed, and the adjacent cell " +
            "is added to the tree. This continues until all cells are connected. The algorithm�s selection method shapes the maze: purely random choices create sprawling, natural " +
            "mazes, while always selecting the newest frontier cell results in long, winding corridors with fewer short loops." },

        {"GenerateGrowingTree_50_0", "The GrowingTree_50_0 algorithm generates a maze using a growing tree approach with edges selected 50% of the time by random and 0% the newest edges," +
            " leaving the other 50% of edges chosen by being the oldest. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges�walls separating " +
            "the tree from unvisited cells�are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze�s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateGrowingTree_25_75", "The GrowingTree_25_75 algorithm generates a maze using a growing tree approach with edges selected 75% of the time by random and 25% of the time the newest edge" +
            " is choosen. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges�walls separating " +
            "the tree from unvisited cells�are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze�s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateGrowingTree_75_25", "The GrowingTree_75_25 algorithm generates a maze using a growing tree approach with edges selected 25% of the time by random and 75% of the time the newest edge" +
            " is choosen. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges�walls separating " +
            "the tree from unvisited cells�are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze�s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateGrowingTree_50_50", "The GrowingTree_50_50 algorithm generates a maze using a growing tree approach with edges selected 50% of the time by random and 50% of the time the newest edge" +
            " is choosen. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges�walls separating " +
            "the tree from unvisited cells�are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze�s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateBacktracking", "The backtracking algorithm generates a maze using depth-first search, treating the grid as a graph where cells are vertices and walls are edges. " +
            "It starts from a random cell, marks it as visited, and repeatedly selects an unvisited neighbor, removing the wall between them and moving forward. When no unvisited neighbors remain," +
            " it backtracks to the last cell with available paths and continues. This process repeats until all cells are visited. The result is a maze with long, winding corridors and " +
            "few short cycles, often featuring a bias toward deep, snaking paths rather than evenly distributed branching structures." },

        {"GenerateWilsons", "Wilson's algorithm builds a maze through loop-erased random walks. Starting from a random seed cell, it picks any unvisited cell and begins a random walk. " +
            "Whenever the walk revisits a cell already on its own current path, the resulting loop is erased and only the path from the revisit point onward is retained. When the walk reaches " +
            "a cell already incorporated into the maze, the surviving path is carved in and the process repeats. Because every possible perfect maze is produced with equal probability " +
            "(a provably uniform spanning tree), Wilson's mazes have balanced, unbiased branching with evenly distributed dead ends throughout. Visually there are no dominant corridors " +
            "and no directional bias, making every region of the maze equally complex. Designed by David Bruce Wilson (1996)." },

        {"GenerateAldousBroder", "The Aldous-Broder algorithm produces a uniform random spanning tree through pure random wandering. Starting from a random cell, the algorithm walks " +
            "to any neighbor at random. If that neighbor is unvisited, a passage is carved and the cell is marked visited. The walk always moves to the neighbor regardless of whether it was " +
            "already visited. This continues until every cell has been visited. Because the algorithm makes no directional decisions, every possible perfect maze is equally likely, " +
            "identical in distribution to Wilson's and Kruskal's. The result is a balanced, unbiased maze with even dead-end density and no structural shortcuts. " +
            "Independently developed by David Aldous and Andrei Broder around 1990." },

        {"GenerateBinaryTree", "Binary Tree is the simplest possible maze algorithm: for every cell in the grid, flip a coin and carve either north or east (ignoring directions that go out of bounds). " +
            "No visited set, no stack, no bookkeeping beyond the grid itself. Each cell is processed exactly once. The simplicity comes at the cost of a pronounced structural bias: " +
            "the top row becomes one unbroken east corridor and the right column becomes one unbroken north shaft. The maze has a strong northeast-grain texture " +
            "because every passage flows north or east. Solving is almost trivial once this bias is recognized. Popularized by Jamis Buck in Mazes for Programmers (2015)." },

        {"GenerateSidewinder", "Sidewinder processes the grid row by row, left to right. It accumulates a run of cells along the current row. At each step it either extends the run eastward " +
            "or closes the run by carving north from one randomly chosen cell within the run. The top row is always a full east corridor since no north carving is possible there. " +
            "The result has a distinctive horizontal banding texture: passages form left-right clusters each with exactly one northern exit. Visually similar to a river delta " +
            "when viewed from above. Sidewinder eliminates the unbroken top-row bias of Binary Tree while retaining row-based structure, making it slightly harder to solve. " +
            "Popularized by Jamis Buck; no single inventor is known." },

        {"GenerateEllers", "Eller's algorithm generates a maze one row at a time using only O(width) memory, making it the first known algorithm capable of generating " +
            "arbitrarily tall mazes in streaming fashion. Each cell in a row belongs to a numbered connected-component set. Adjacent cells from different sets are randomly merged " +
            "(east wall removed). Then, for each set, at least one cell must receive a south-opening passage; others may be chosen randomly. " +
            "Cells without south openings begin fresh sets in the next row. The final row merges all remaining different adjacent sets. " +
            "Despite working one row at a time, the output is statistically indistinguishable from a global minimum spanning tree: balanced branching, no bias, even dead-end density. " +
            "Invented by Marlin Eller in 1982; unpublished until rediscovered in his archive." },

        {"GenerateRecursiveDivision", "Recursive Division is the only algorithm here that adds walls rather than removes them. The grid starts as a completely open floor plan. " +
            "A rectangular region is split by a wall running the full width or height of the region, with exactly one gap left for passage. Both sub-regions are then subdivided recursively. " +
            "Region orientation is chosen based on aspect ratio: tall regions are split horizontally, wide regions vertically, square regions randomly. " +
            "The result is a fractal maze structure with characteristically long straight walls, rectangular chambers nested within chambers, and a small number of mandatory bottleneck passages. " +
            "Solving strategy: identify the gap in each dividing wall and trace the tree of regions from exit back to start. Conceptually related to BSP tree generation; " +
            "popularized for mazes by Jamis Buck." },

        {"GenerateSpiralBacktracker", "The Spiral Backtracker is a directionally biased variant of recursive backtracking designed by Jake Frischmann. " +
            "Standard DFS picks a completely random unvisited neighbor at each step. The Spiral Backtracker instead gives a 70% probability to continuing in the same direction " +
            "as the previous move, causing the path to spiral outward in long looping corridors rather than wandering erratically. When the preferred direction is blocked or " +
            "the 30% random chance triggers, a new direction is chosen and becomes the next preference. Backtracking resets the direction. " +
            "The result is a maze dominated by sweeping curved corridors that loop and wrap around themselves, creating a distinctive pinwheel appearance. " +
            "The spiraling structure means the exit may appear visually close but be many corridor-lengths away. Designed by Jake Frischmann." },

    };

    public async void DisplayInfoOnMaze()
    {
        selected_dailyLevel = await App.PlayerData.dailyMazeDatabase.GetItemAsync(selected_dailyLevel.Date.ToString("d"));

        dateInfoLabel.Text = selected_dailyLevel.Date.ToString("D");
        generationTypeInfoLabel.Text = selected_dailyLevel.LevelType;
        sizeInfoLabel.Text = $"{selected_dailyLevel.Width} x {selected_dailyLevel.Height}";
        descriptionInfoLabel.Text = GenerationMethodDescriptions[selected_dailyLevel.LevelType];
        statusInfoLabel.Text = selected_dailyLevel.Status;

        if (selected_dailyLevel.Status == "Not Attempted")
        {
            timeInfoLabel.Text = "";
        }
        else
        {
            timeInfoLabel.Text = $"{Math.Round(selected_dailyLevel.CompletetionTime, 1)}s / {selected_dailyLevel.TimeNeeded}s";
        }

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

    bool countingDown = false;
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
            if (selected_dailyLevel.TimeNeeded >= timePassed.TotalSeconds)
            {
                labelTimer.Text = $"{Math.Round(timePassed.TotalSeconds, 1).ToString("N1")} / {selected_dailyLevel.TimeNeeded.ToString()}";
                //moveNumberText.Text = Level.TwoStarMoves.ToString() + ":  " + numberOfMoves.ToString();

                if (selected_dailyLevel.TimeNeeded - timePassed.TotalSeconds <= 5 && !countingDown)
                {
                    countingDown = true;
                    DrawCountDownTimer(5);
                }

                if (_gameSession?.IsComplete() == true)
                {
                    TotalTime = DateTime.Now - timeStarted;
                    CompletedMaze();
                }
            }
            else
            {
                mazeGraphicsView.IsGameOver = true;
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
        _gameSession = new MazeGameSession(Maze);

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

        AbsoluteLayout.SetLayoutBounds(main_absolute_layout, new Rect(0.5, 0.5, MazeWindowWidth, MazeWindowHeight));
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

        if (selected_dailyLevel.Status != "Completed")
        {
            selected_dailyLevel.Status = "Attempted";
        }
    }

    public void InitializeMaze()
    {
        Maze.MazeGenerationDelegateList[selected_dailyLevel.LevelType](selected_dailyLevel.Width, selected_dailyLevel.Height);


        if (selected_dailyLevel.Width * selected_dailyLevel.Height < 100)
        {
            selected_dailyLevel.TimeNeeded = Maze.PathLength / 2;
        }else if (Maze.PathLength > 100)
        {
            selected_dailyLevel.TimeNeeded = (int)(Maze.PathLength * .3);
        }
        else
        {
            selected_dailyLevel.TimeNeeded = Maze.PathLength / (int)Math.Max(1.3 + 0.15 * (Maze.PathLength / 10), 0.5);
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
        dict_int_to_color.Add(1, App.PlayerData.WallColor);
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
            Color = dict_int_to_color[3],
            Opacity = 0.8,

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
        TryMove(Direction.Left);
    }
    public void MoveRight()
    {
        TryMove(Direction.Right);
    }
    public void MoveUp()
    {
        TryMove(Direction.Up);
    }
    public void MoveDown()
    {
        TryMove(Direction.Down);
    }

    private void TryMove(Direction direction)
    {
        if (_gameSession == null)
        {
            return;
        }

        var result = _gameSession.TryMove(direction);
        if (result.Moved)
        {
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
        mazeGraphicsView.IsGameOver = true;
        hook?.Dispose();

        double time = TotalTime.TotalSeconds;


        if (time < selected_dailyLevel.CompletetionTime || selected_dailyLevel.CompletetionTime == 0)
        {
            selected_dailyLevel.CompletetionTime = time;
        }

        selected_dailyLevel.Status = "Completed";

        ImageButton winningStar = new ImageButton
        {
            Source = "full_star.png",
            Opacity = 0,
            Scale = 0.1,
            IsVisible = true,
            Background = Colors.Transparent,
        };
        AbsoluteLayout.SetLayoutBounds(winningStar, new Rect(0.5, 0.5, 1, 1));
        AbsoluteLayout.SetLayoutFlags(winningStar, AbsoluteLayoutFlags.All);

        playAbsoluteLayout.Add(winningStar);

        _ = main_absolute_layout.FadeTo(0.2, 500);

        _ = winningStar.FadeTo(1, 1000);
        await winningStar.ScaleTo(3, 3000);
        _ = winningStar.ScaleTo(0.1, 100);
        winningStar.IsVisible = false;

        monthlyMazes[selected_dailyLevel.Date.Day - 1].Status = "Completed";
        await App.PlayerData.dailyMazeDatabase.SaveLevelAsync(selected_dailyLevel);

        //await DrawLevels(false);

        //stateContainerModel.CurrentState = "Calendar";

        await Navigation.PushAsync(new DailyMazePage());
    }

    Label numlabel;
    public async void DrawCountDownTimer(int secondsLeft)
    {
        if (running)
        {
            if (secondsLeft == 5)
            {
                numlabel = new Label()
                {
                    Text = secondsLeft.ToString(),
                    TextColor = Colors.Red,
                    Opacity = 0.6,
                    FontSize = 100,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    IsVisible = true,
                };

                AbsoluteLayout.SetLayoutBounds(numlabel, new Rect(0.5, 0.5, 0.6, 0.6));
                AbsoluteLayout.SetLayoutFlags(numlabel, AbsoluteLayoutFlags.All);

                playAbsoluteLayout.Add(numlabel);
            }
            numlabel.Text = secondsLeft.ToString();

            _ = numlabel.FadeTo(1, 500);
            await numlabel.ScaleXTo(1.2, 500);

            _ = numlabel.FadeTo(0.6, 500);
            await numlabel.ScaleXTo(0.8, 500);

            if (secondsLeft == 0)
            {
                _ = numlabel.FadeTo(0, 500);
                await numlabel.ScaleTo(0.5, 500);
                numlabel.IsVisible = false;
                countingDown = false;
                FailedMaze();
            }
            else
            {
                DrawCountDownTimer(--secondsLeft);
            }
        }
        else
        {
            countingDown = false;
        }
    }


    public async void FailedMaze()
    {
        // Button to retry Maze or exit back to previous screen
        running = false;
        mazeGraphicsView.IsGameOver = true;
        hook?.Dispose();

        double time = TotalTime.TotalSeconds;

        ImageButton losingStar = new ImageButton
        {
            Source = "empty_star.png",
            Opacity = 0,
            Scale = 0.1,
            IsVisible = true,
            Background = Colors.Transparent,
        };
        AbsoluteLayout.SetLayoutBounds(losingStar, new Rect(0.5, 0.5, 1, 1));
        AbsoluteLayout.SetLayoutFlags(losingStar, AbsoluteLayoutFlags.All);

        playAbsoluteLayout.Add(losingStar);

        _ = main_absolute_layout.FadeTo(0, 500);

        _ = losingStar.FadeTo(1, 500);
        await losingStar.ScaleTo(3, 2500);
        _ = losingStar.ScaleTo(0.1, 100);
        losingStar.IsVisible = false;

        monthlyMazes[selected_dailyLevel.Date.Day - 1].Status = selected_dailyLevel.Status;
        await App.PlayerData.dailyMazeDatabase.SaveLevelAsync(selected_dailyLevel);

        //stateContainerModel.CurrentState = "Calendar";
        await Navigation.PushAsync(new DailyMazePage());
    }






    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        hook?.Dispose();
    }

    private string PickDailyMazeAlgorithm()
    {
        var primsTypes = new HashSet<string>
        {
            "GeneratePrims", "GenerateGrowingTree_50_50", "GenerateGrowingTree_75_25",
            "GenerateGrowingTree_25_75", "GenerateGrowingTree_50_0"
        };
        var pool = new List<string>();
        foreach (var t in Maze.MazeTypes)
        {
            if (t == "GenerateBacktracking") continue;
            pool.Add(t);
            if (primsTypes.Contains(t)) pool.Add(t); // double weight for Prim's variants
        }
        return pool[rnd.Next(0, pool.Count)];
    }
}