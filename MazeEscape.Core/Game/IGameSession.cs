using System.Collections.Generic;

namespace MazeEscape.Core.Game;

public interface IGameSession
{
    (int X, int Y) Position { get; }
    (int X, int Y) Goal { get; }
    MoveResult TryMove(Direction direction);
    bool IsComplete();
    IReadOnlyList<Direction> FindPath(int fromX, int fromY, int toX, int toY);
}
