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

        if (PlayerData.MostRecentMonth != date_time.ToShortDateString())
        {
            RestartMonth = true;
            PlayerData.MostRecentMonth = date_time.ToShortDateString();
            PlayerData.Save();
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

            //DailyMazeLevel day_item = await PlayerData.dailyMazeDatabase.GetItemAsync(days_short_date);

            if (RestartMonth)
            {
                string mazeType = Maze.MazeTypes[rnd.Next(0, Maze.MazeTypes.Count)];

                //DateTime new_date_time = new DateTime(date_time.Year, date_time.Month, i);

                DailyMazeLevel that_day = new DailyMazeLevel()
                {
                    Width = rnd.Next(12, 35),
                    Height = rnd.Next(12, 35),
                    LevelType = mazeType,
                    Date = new_date_time,
                    ShortDate = new_date_time.ToString("d"),
                    Month_Year = new_date_time.ToString("MM-yyyy"),

                };

                await PlayerData.dailyMazeDatabase.AddNewLevelAsync(that_day);

                monthlyMazes.Add(that_day);
            }
            else
            {
                DailyMazeLevel day_item = await PlayerData.dailyMazeDatabase.GetItemAsync(days_short_date);

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

        if (!PlayerData.MonthPrize1_achieved && number_of_stars_won >= ((int)days_in_this_month / 2))
        {
            PlayerData.CoinCount += 200;
            PlayerData.MonthPrize1_achieved = true;
            await this.ShowPopupAsync(new CampaignChestOpenedPopupPage(200), CancellationToken.None);
            PlayerData.Save();
        }
        if (!PlayerData.MonthPrize2_achieved && number_of_stars_won >= days_in_this_month)
        {
            PlayerData.CoinCount += 500;
            PlayerData.MonthPrize2_achieved = true;
            await this.ShowPopupAsync(new CampaignChestOpenedPopupPage(500), CancellationToken.None);
            PlayerData.Save();
        }
        if (number_of_stars_won == 0)
        {
            PlayerData.MonthPrize1_achieved = false;
            PlayerData.MonthPrize2_achieved = false;
            PlayerData.Save();
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

        {"GenerateKruskals", "Kruskal’s algorithm generates a maze by constructing a minimum spanning tree (MST) over a grid graph. Each cell is a vertex, and walls between them " +
            "represent weighted edges. Initially, each cell is its own disjoint set. The algorithm sorts all walls randomly and iterates through them, removing a wall if it connects " +
            "vertices from different sets. This is efficiently managed using the union-find data structure. The process continues until a spanning tree forms, ensuring connectivity " +
            "without cycles. The resulting maze has uniform randomness, as edge selection is globally randomized rather than biased by a growing frontier, like Prim’s algorithm." },

        {"GeneratePrims", "Prim’s algorithm generates a maze using a growing tree approach, treating the grid as a graph where cells are vertices and walls are edges." +
            " It begins with a random single cell and tracks its frontier—walls separating it from unvisited cells. At each step, a random frontier wall is removed, and the adjacent cell " +
            "is added to the tree. This continues until all cells are connected. The algorithm’s selection method shapes the maze: purely random choices create sprawling, natural " +
            "mazes, while always selecting the newest frontier cell results in long, winding corridors with fewer short loops." },

        {"GenerateGrowingTree_50_0", "The GrowingTree_50_0 algorithm generates a maze using a growing tree approach with edges selected 50% of the time by random and 0% the newest edges," +
            " leaving the other 50% of edges chosen by being the oldest. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges—walls separating " +
            "the tree from unvisited cells—are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze’s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateGrowingTree_25_75", "The GrowingTree_25_75 algorithm generates a maze using a growing tree approach with edges selected 75% of the time by random and 25% of the time the newest edge" +
            " is choosen. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges—walls separating " +
            "the tree from unvisited cells—are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze’s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateGrowingTree_75_25", "The GrowingTree_75_25 algorithm generates a maze using a growing tree approach with edges selected 25% of the time by random and 75% of the time the newest edge" +
            " is choosen. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges—walls separating " +
            "the tree from unvisited cells—are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze’s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateGrowingTree_50_50", "The GrowingTree_50_50 algorithm generates a maze using a growing tree approach with edges selected 50% of the time by random and 50% of the time the newest edge" +
            " is choosen. Treating the grid as a graph, it starts with a single cell as the initial tree. Frontier edges—walls separating " +
            "the tree from unvisited cells—are stored in a priority queue or randomized list. At each step, a random frontier edge is selected following the above chances, its wall removed," +
            " and the new cell added to the tree. This continues until all cells are connected. The maze’s structure depends on edge " +
            "selection: a purely random choice creates a Prim-like maze, while preferring the newest edges mimics recursive backtracking." },

        {"GenerateBacktracking", "The backtracking algorithm generates a maze using depth-first search, treating the grid as a graph where cells are vertices and walls are edges. " +
            "It starts from a random cell, marks it as visited, and repeatedly selects an unvisited neighbor, removing the wall between them and moving forward. When no unvisited neighbors remain," +
            " it backtracks to the last cell with available paths and continues. This process repeats until all cells are visited. The result is a maze with long, winding corridors and " +
            "few short cycles, often featuring a bias toward deep, snaking paths rather than evenly distributed branching structures." },

    };

    public async void DisplayInfoOnMaze()
    {
        selected_dailyLevel = await PlayerData.dailyMazeDatabase.GetItemAsync(selected_dailyLevel.Date.ToString("d"));

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
                labelTimer.Text = selected_dailyLevel.TimeNeeded.ToString() + ":  " + Math.Round(timePassed.TotalSeconds, 1).ToString();
                //moveNumberText.Text = Level.TwoStarMoves.ToString() + ":  " + numberOfMoves.ToString();

                if (selected_dailyLevel.TimeNeeded - timePassed.TotalSeconds <= 5 && !countingDown)
                {
                    countingDown = true;
                    DrawCountDownTimer(5);
                }

                if (Maze.Player.X == Maze.End.Item1 && Maze.Player.Y == Maze.End.Item2)
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
        await PlayerData.dailyMazeDatabase.SaveLevelAsync(selected_dailyLevel);

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
        await PlayerData.dailyMazeDatabase.SaveLevelAsync(selected_dailyLevel);

        //stateContainerModel.CurrentState = "Calendar";
        await Navigation.PushAsync(new DailyMazePage());
    }






    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        hook?.Dispose();
    }
}