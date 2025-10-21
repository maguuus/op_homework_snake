using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Entities;

public class Food(Point position, FoodType type = FoodType.Normal)
{
    public Point Position { get; } = position;
    public FoodType Type { get; } = type;
    public int ScoreValue => GetScoreValue();
    public ConsoleColor Color => GetColor();
    
    private int GetScoreValue()
    {
        return Type switch
        {
            FoodType.Normal => 1,
            FoodType.Bonus => 5,
            FoodType.Speed => 1,
            FoodType.Slow => 1,
            FoodType.Reverse => 3,
            FoodType.Shield => 2,
            FoodType.Double => 2,
            FoodType.Shrink => 5,
            _ => 1
        };
    }

    private ConsoleColor GetColor()
    {
        return Type switch
        {
            FoodType.Normal => ConsoleColor.Red,
            FoodType.Bonus => ConsoleColor.Magenta,
            FoodType.Speed => ConsoleColor.Yellow,
            FoodType.Slow => ConsoleColor.Blue,
            FoodType.Reverse => ConsoleColor.Cyan,
            FoodType.Shield => ConsoleColor.White,
            FoodType.Double => ConsoleColor.Green,
            FoodType.Shrink => ConsoleColor.DarkRed,
            _ => ConsoleColor.Red
        };
    }

    public char GetSymbol()
    {
        return Type switch
        {
            FoodType.Normal => '♦',
            FoodType.Bonus => '✪',  
            FoodType.Speed => '⭍',
            FoodType.Slow => '❄',
            FoodType.Reverse => '↻',
            FoodType.Shield => '⛊', 
            FoodType.Double => '2',
            FoodType.Shrink => '↓',
            _ => '♦' 
        };
    }
}