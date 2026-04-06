using MazeEscape.Core.Game;
using MazeEscape.Models;

namespace MazeEscape.Services;

public sealed class MazeGameSession : IGameSession
{
    private readonly MazeModel _maze;

    public MazeGameSession(MazeModel maze)
    {
        _maze = maze;
    }

    public (int X, int Y) Position => (_maze.Player.X, _maze.Player.Y);

    public (int X, int Y) Goal => (_maze.End.Item1, _maze.End.Item2);

    public MoveResult TryMove(Direction direction)
    {
        var (nextX, nextY) = direction switch
        {
            Direction.Left => (_maze.Player.X - 1, _maze.Player.Y),
            Direction.Right => (_maze.Player.X + 1, _maze.Player.Y),
            Direction.Up => (_maze.Player.X, _maze.Player.Y - 1),
            Direction.Down => (_maze.Player.X, _maze.Player.Y + 1),
            _ => (_maze.Player.X, _maze.Player.Y)
        };

        if (!CanMove(direction, nextX, nextY))
        {
            return new MoveResult(false, _maze.Player.X, _maze.Player.Y);
        }

        _maze.Player.X = nextX;
        _maze.Player.Y = nextY;
        return new MoveResult(true, _maze.Player.X, _maze.Player.Y);
    }

    public bool IsComplete()
    {
        return _maze.Player.X == _maze.End.Item1 && _maze.Player.Y == _maze.End.Item2;
    }

    private bool CanMove(Direction direction, int nextX, int nextY)
    {
        if (nextX < 0 || nextY < 0 || nextX >= _maze.Width || nextY >= _maze.Height)
        {
            return false;
        }

        return direction switch
        {
            Direction.Left => !_maze.Cells[_maze.Player.Y][_maze.Player.X - 1].East,
            Direction.Right => !_maze.Cells[_maze.Player.Y][_maze.Player.X].East,
            Direction.Up => !_maze.Cells[_maze.Player.Y][_maze.Player.X].North,
            Direction.Down => !_maze.Cells[_maze.Player.Y + 1][_maze.Player.X].North,
            _ => false
        };
    }
}
