using op_homework_snake_game.Entities;
using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Data;

public class MapInfo
{
    public string Name { get; set; } = "";
    public List<GameMode> AllowedModes { get; set; } = new();
    public List<Point> SnakeStartPositions { get; set; } = new();
    public string FilePath { get; set; } = " ";
}
