namespace SnakeGame;

public enum FoodType
{
    Normal,
    Bonus
}

public class Food
{
    public Point Position { get; }
    public FoodType Type { get; } = FoodType.Normal;

    public Food(Point position, FoodType type = FoodType.Normal)
    {
        Position = position;
        Type = type;
    }
}