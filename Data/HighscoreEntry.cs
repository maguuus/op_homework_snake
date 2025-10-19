namespace op_homework_snake_game.Data;

public class HighscoreEntry
{
    public string PlayerName { get; init; } = string.Empty;
    public int Score { get; init; }
    public DateTime Time { get; init; }
    public int SnakeLength { get; init; }
}