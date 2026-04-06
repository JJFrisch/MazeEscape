namespace MazeEscape.Core.Game;

public interface IGameSession
{
    (int X, int Y) Position { get; }
    (int X, int Y) Goal { get; }
    MoveResult TryMove(Direction direction);
    bool IsComplete();
}
