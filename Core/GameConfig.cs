namespace op_homework_snake_game.Core;

public static class GameConfig
{
    public const int MaxFoodCount = 10;
    public const int InitialFoodAmount = 3;
    public const int InitialSnakeLength = 5;
    public const int FieldWidthBuffer = 10;
    public const int FieldHeightBuffer = 8;
    public const int MinFieldWidth = 20;
    public const int MinFieldHeight = 15;

    public const int MaxSpeed = 300;
    public const int MinSpeed = 600;

    public const int CommandProcessingTimeout = 16;
    public const int InputPollingInterval = 10;
}
