using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Input;

public class InputCommand(InputCommandType type, int playerId = 0)
{
    public InputCommandType Type { get; set; } = type;
    public int PlayerId { get; set; } = playerId;
}