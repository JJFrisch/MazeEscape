using MazeEscape.Drawables;
using MazeEscape.Models;
using Microsoft.Maui.Layouts;
using SharpHook;
using SharpHook.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Numerics;
using SharpHook.Native;
using CommunityToolkit.Maui.Core;


namespace MazeEscape;

public partial class CampaignLevelPage : ContentPage
{
    public event EventHandler<CampaignLevel>? LevelSaved;
    CampaignLevel Level;

    public double MazeWindowWidth = PlayerData.WindowWidth * 0.95;
    public double MazeWindowHeight = PlayerData.WindowHeight * 0.8;

    MazeModel Maze = new MazeModel();

    PlayerDrawable drawer;
    SimpleReactiveGlobalHook? hook;

    TimeSpan Time;
    public int Moves;


    public CampaignLevelPage(CampaignLevel level)
    {
        InitializeComponent();

        Level = level;

        AbsoluteLayout.SetLayoutBounds(main_absolute_layout, new Rect(0.5, 0.5, MazeWindowWidth, MazeWindowHeight));
        AbsoluteLayout.SetLayoutFlags(main_absolute_layout, AbsoluteLayoutFlags.PositionProportional);

        InitializeReactiveKeyboard();

        drawer = new PlayerDrawable();
        PlayerGraphicsView.Drawable = drawer;
        drawer.Initialize();

        InitializeMaze();

        DrawMaze("line");

        UpdatePlayerDrawerPosition();

        AddSwipeGestures();

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

        int line_thickness = 2;

        for (var h = 0; h < Maze.Height; h++)
        {
            for (var w = 0; w < Maze.Width; w++)
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
                    if (Maze.Cells[h][w].Value != 0)
                    {
                        main_absolute_layout.Add(new BoxView
                        {
                            Color = dict_int_to_color[Maze.Cells[h][w].Value]

                        }, new Rect(w * cell_width + (line_thickness / 2), h * cell_height + (line_thickness / 2), cell_width - line_thickness, cell_height - line_thickness));
                    }


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

            PlayerGraphicsView.GestureRecognizers.Add(leftSwipeGesture);
            PlayerGraphicsView.GestureRecognizers.Add(rightSwipeGesture);
            PlayerGraphicsView.GestureRecognizers.Add(upSwipeGesture);
            PlayerGraphicsView.GestureRecognizers.Add(downSwipeGesture);
        }
    }

    public void RedrawPlayer()
    {
        PlayerGraphicsView.Invalidate();
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
        drawer.PlayerX = Maze.Player.X;
        drawer.PlayerY = Maze.Player.Y;

        if (Maze.Player.X == Maze.End.Item1 && Maze.Player.Y == Maze.End.Item2)
        {
            RedrawPlayer();
            CompletedMaze();
        }
    }

    public void CompletedMaze()
    {
        // Score like time, number of moves, or amount of false steps
        // Award 0-3 stars
        // Button to retry Maze or exit back to previous screen

        int timeTemp = 20;
        int movesTemp = 20;

        Level.Star1 = true;

        if (timeTemp <= Level.ThreeStarTime)
        {
            Level.Star3 = true;
        }
        if (movesTemp <= Level.TwoStarMoves)
        {
            Level.Star2 = true;
        }
        
        if (timeTemp < Level.BestTime)
        {
            Level.BestTime = timeTemp;
        }
        if (movesTemp < Level.BestMoves)
        {
            Level.BestMoves = movesTemp;
        }

        LevelSaved?.Invoke(this, Level);
        Navigation.PopAsync();
        
        //await Navigation.PushModalAsync(new FinishedMazePage(Level, timeTemp, movesTemp));

    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        hook?.Dispose();
    }

}