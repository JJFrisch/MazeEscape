using System.Collections.Generic;
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

    public IReadOnlyList<Direction> FindPath(int fromX, int fromY, int toX, int toY)
    {
        if (fromX == toX && fromY == toY)
            return Array.Empty<Direction>();

        var allDirections = new[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
        var queue = new Queue<(int X, int Y)>();
        var parent = new Dictionary<(int, int), ((int, int) From, Direction Dir)>();

        queue.Enqueue((fromX, fromY));
        parent[(fromX, fromY)] = ((-1, -1), Direction.Left); // sentinel

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            if (cx == toX && cy == toY)
            {
                var path = new List<Direction>();
                var cur = (toX, toY);
                while (cur != (fromX, fromY))
                {
                    var (prev, dir) = parent[cur];
                    path.Add(dir);
                    cur = prev;
                }
                path.Reverse();
                return path;
            }

            foreach (var dir in allDirections)
            {
                if (!CanMoveFrom(cx, cy, dir)) continue;
                var (nx, ny) = dir switch
                {
                    Direction.Left  => (cx - 1, cy),
                    Direction.Right => (cx + 1, cy),
                    Direction.Up    => (cx, cy - 1),
                    Direction.Down  => (cx, cy + 1),
                    _               => (cx, cy)
                };
                if (!parent.ContainsKey((nx, ny)))
                {
                    parent[(nx, ny)] = ((cx, cy), dir);
                    queue.Enqueue((nx, ny));
                }
            }
        }

        return Array.Empty<Direction>();
    }

    private bool CanMoveFrom(int x, int y, Direction direction)
    {
        var (nextX, nextY) = direction switch
        {
            Direction.Left  => (x - 1, y),
            Direction.Right => (x + 1, y),
            Direction.Up    => (x, y - 1),
            Direction.Down  => (x, y + 1),
            _               => (x, y)
        };

        if (nextX < 0 || nextY < 0 || nextX >= _maze.Width || nextY >= _maze.Height)
            return false;

        return direction switch
        {
            Direction.Left  => !_maze.Cells[y][x - 1].East,
            Direction.Right => !_maze.Cells[y][x].East,
            Direction.Up    => !_maze.Cells[y][x].North,
            Direction.Down  => !_maze.Cells[y + 1][x].North,
            _               => false
        };
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
