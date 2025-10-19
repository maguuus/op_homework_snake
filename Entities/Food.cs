using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Entities;

public class Food(Point position, FoodType type = FoodType.Normal)
{
    public Point Position { get; } = position;
    public FoodType Type { get; } = type;
}