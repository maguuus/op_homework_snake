namespace SnakeGame;

public enum InputCommandType
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    ExitGame
}

public class InputCommand
{
    public InputCommandType Type { get; set; }
    public int PlayerId { get; set; }

    public InputCommand(InputCommandType type, int playerId = 0)
    {
        Type = type;
        PlayerId = playerId;
    }
}