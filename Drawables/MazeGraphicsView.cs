using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MazeEscape.Models;


namespace MazeEscape.Drawables
{
    internal class MazeGraphicsView : GraphicsView
    {

        private int _fpsElapsed;
        private int _fpsCount = 0;
        private const double _fps = 30;
        private readonly Stopwatch _stopWatch = new Stopwatch();

        //public static ICommand Up = new Command(() => Player.Up());
        //public static ICommand Down = new Command(() => Player.Down());
        //public static ICommand Left = new Command(() => Player.Left());
        //public static ICommand Right = new Command(() => Player.Right());

        public PlayerDrawable Player;

        public MazeModel Maze;

        public bool IsGameOver { get; set; }

        public double MazeWindowWidth = PlayerData.WindowWidth * 0.95;
        public double MazeWindowHeight = PlayerData.WindowHeight * 0.8;

        public MazeGraphicsView()
        {
            //base.Drawable = Player = new PlayerDrawable();
            //Player.Initialize();
            //Initializegestures();


            var ms = 1000.0 / _fps;
            var ts = TimeSpan.FromMilliseconds(ms);
            IsGameOver = false;
            //Device.StartTimer(ts, TimerLoop);

            IDispatcherTimer timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => TimerLoop();
            timer.Start();


        }

        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalSeconds;
            _stopWatch.Restart();
            // calculate current fps
            var fps = dt > 0 ? 1.0 / dt : 0;
            // when the fps is too low reduce the load by skipping the frame
            if (fps < _fps / 2)
                return true;

            _fpsCount++;
            _fpsElapsed++;
            if (_fpsCount == 20)
                _fpsCount = 0;

            //Its been a second
            if (_fpsElapsed == _fps)
            {
                _fpsElapsed = 0;
                if (IsGameOver)
                {
                    Navigation.PopModalAsync();
                }
            }

            //CheckIfGameOver();
            Invalidate();
            return true;
        }

        //private void Initializegestures()
        //{
        //    SwipeGestureRecognizer leftSwipeGesture = new() { Direction = SwipeDirection.Left };
        //    leftSwipeGesture.Swiped += OnSwiped;
        //    SwipeGestureRecognizer rightSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        //    rightSwipeGesture.Swiped += OnSwiped;
        //    SwipeGestureRecognizer upSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
        //    upSwipeGesture.Swiped += OnSwiped;
        //    SwipeGestureRecognizer downSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
        //    downSwipeGesture.Swiped += OnSwiped;

        //    base.GestureRecognizers.Add(leftSwipeGesture);
        //    base.GestureRecognizers.Add(rightSwipeGesture);
        //    base.GestureRecognizers.Add(upSwipeGesture);
        //    base.GestureRecognizers.Add(downSwipeGesture);
        //}

        //private void OnSwiped(object sender, SwipedEventArgs e)
        //{
        //    if (!IsGameOver)
        //    {
        //        switch (e.Direction)
        //        {
        //            case SwipeDirection.Left:
        //                MoveLeft();
        //                break;
        //            case SwipeDirection.Right:
        //                MoveRight();
        //                break;
        //            case SwipeDirection.Up:
        //                MoveUp();
        //                break;
        //            case SwipeDirection.Down:
        //                MoveDown();
        //                break;
        //        }
        //    }
        //}

        //public void MoveLeft()
        //{
        //    if (Player.XPos > 0 && !Maze.Cells[Player.YPos][Player.XPos - 1].East)
        //    {

        //        Player.XPos--;
        //    }
        //}
        //public void MoveRight()
        //{
        //    if (Player.XPos < Maze.Width - 1 && !Maze.Cells[Player.YPos][Player.XPos].East)
        //    {

        //        Player.XPos++;
        //    }
        //}
        //public void MoveUp()
        //{
        //    if (Player.YPos > 0 && !Maze.Cells[Player.YPos][Player.XPos].North)
        //    {

        //        Player.YPos--;
        //    }
        //}
        //public void MoveDown()
        //{
        //    if (Player.YPos < Maze.Height - 1 && !Maze.Cells[Player.YPos   + 1][Player.XPos].North)
        //    {

        //        Player.YPos++;
        //    }
        //}

        //private bool CheckIfGameOver()
        //{
        //    if (Maze.Player.X == Maze.End.Item1 && Maze.Player.Y == Maze.End.Item2)
        //    {
        //        IsGameOver = true;
        //        Navigation.PopModalAsync();
        //    }
        //    return IsGameOver;
        //}
    }
}
