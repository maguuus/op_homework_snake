using op_homework_snake_game.Core;
using op_homework_snake_game.Entities;
using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Rendering;

public class GameRenderer(GameState gameState)
{
    private readonly char[,] _previousFrame = new char[Console.WindowWidth, Console.WindowHeight];
    private bool _firstRender = true;
    private List<Point> _previousSnakePositions = [];
    private List<Point> _previousFoodPositions = [];

    public void RenderInitialScreen()
    {
        Console.Clear();
        Console.WriteLine("=== Snake Game ===");
        Console.WriteLine($"Field size: {gameState.FieldWidth}x{gameState.FieldHeight}");
        Console.WriteLine("Control: ESC - exit");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);

        InitialPreviousFrame();
    }

    private void InitialPreviousFrame()
    {
        for (var y = 0; y < gameState.FieldHeight; y++)
        {
            for (var x = 0; x < gameState.FieldWidth; x++)
            {
                _previousFrame[x, y] = ' ';
            }
        }
    }
    
    public void Render(bool isPaused = false)
    {
        if (_firstRender)
        {
            Console.Clear();
            _firstRender = false;
        }

        ClearOldPositions();
        DrawGameObjects(isPaused);
        SaveCurrentState();
    }

    private void ClearOldPositions()
    {
        foreach (var previousSnakePosition in _previousSnakePositions)
        {
            if (!IsPositionOccupied(previousSnakePosition) && IsInsideField(previousSnakePosition))
            {
                DrawPixel(previousSnakePosition.X, previousSnakePosition.Y, ConsoleColor.Black, ' ');
            }
        }

        foreach (var previousFoodPosition in _previousFoodPositions)
        {
            if (!IsPositionOccupied(previousFoodPosition) && IsInsideField(previousFoodPosition))
            {
                DrawPixel(previousFoodPosition.X, previousFoodPosition.Y, ConsoleColor.Black, ' ');
            }
        }
    }

    private void DrawGameObjects(bool isPaused = false)
    {
        DrawBorders();
        DrawAllSnake();
        DrawAllFood();
        DrawInfo(isPaused);
    }
    private void DrawBorders()
    {
        for (int x = 0; x < gameState.FieldWidth; x++)
        {
            DrawPixel(x, 0, ConsoleColor.White, '═');
            DrawPixel(x, gameState.FieldHeight - 1, ConsoleColor.White, '═');
        }

        for (int y = 0; y < gameState.FieldHeight; y++)
        {
            DrawPixel(0, y, ConsoleColor.White, '║');
            DrawPixel(gameState.FieldWidth - 1, y, ConsoleColor.White, '║');
        }
        
        DrawPixel(0, 0, ConsoleColor.White, '╔');
        DrawPixel(gameState.FieldWidth - 1, 0, ConsoleColor.White, '╗');
        DrawPixel(0, gameState.FieldHeight - 1, ConsoleColor.White, '╚');
        DrawPixel(gameState.FieldWidth - 1, gameState.FieldHeight - 1, ConsoleColor.White, '╝');
    }

    private void DrawInfo(bool isPaused = false)
    {
        var player1 = gameState.Snakes.FirstOrDefault(s => s.PlayerId == 0);
        var player2 = gameState.Snakes.FirstOrDefault(s => s.PlayerId == 1);
        var isMultiplayer = gameState.Snakes.Count > 1 && player2 != null;
        var info = isMultiplayer 
            ? "Snake game | ESC: exit | P/space: pause | P1: WASD | P2: arrows" 
            : "Snake game | ESC: exit | P/space: pause | Move: WASD";
        var player1Info = $"P1: Length: {player1?.Body.Count ?? 0} | Direction: {player1?.CurrentDirection ?? Direction.None}";
        var gameInfo = $"Score: {gameState.Score} | Speed: {GetSpeedDescription()}";
        var infoX = Math.Max(0, (gameState.FieldWidth - info.Length) / 2);
        var player1X = Math.Max(0, (gameState.FieldWidth - player1Info.Length) / 2);
        var gameInfoX = Math.Max(0, (gameState.FieldWidth - gameInfo.Length) / 2);
        for (var y = gameState.FieldHeight + 1; y <= gameState.FieldHeight + 5; y++)
        {
            ClearLine(y);
        }
        DrawText(info, infoX, gameState.FieldHeight + 1, ConsoleColor.Gray);
        DrawText(player1Info, player1X, gameState.FieldHeight + 2, ConsoleColor.Green);
        if (isMultiplayer)
        {
            var player2Info = $"P2: Length: {player2?.Body.Count ?? 0} | Direction: {player2?.CurrentDirection ?? Direction.None}";
            var player2X = Math.Max(0, (gameState.FieldWidth - player2Info.Length) / 2);    
            DrawText(player2Info, player2X, gameState.FieldHeight + 3, ConsoleColor.Blue);
            DrawText(gameInfo, gameInfoX, gameState.FieldHeight + 4, ConsoleColor.Cyan);
        }
        else
        {
            DrawText(gameInfo, gameInfoX, gameState.FieldHeight + 3, ConsoleColor.Cyan);
        }

        if (!isPaused) return;
        const string pauseInfo = "*** PAUSED ***";
        var pauseInfoX = Math.Max(0, (gameState.FieldWidth - pauseInfo.Length) / 2);
        var pauseInfoY = isMultiplayer ? gameState.FieldHeight + 5 : gameState.FieldHeight + 4;
        DrawText(pauseInfo, pauseInfoX, pauseInfoY, ConsoleColor.Yellow);
    }

    private void ClearLine(int y)
    {
        Console.SetCursorPosition(0, y);
        Console.Write(new string(' ', Console.WindowWidth));
        for (var x = 0; x < gameState.FieldWidth; x++)
        {
            _previousFrame[x, y] = ' ';
        }
    } 
    private string GetSpeedDescription()
    {
        var snakeLength = gameState.PlayerSnake?.Body.Count ?? 0;
        return snakeLength switch
        {
            <= 10 => "Slow",
            <= 20 => "Medium",
            <= 30 => "Fast",
            _ => "Very Fast"
        };
    }

    private void DrawAllSnake()
    {
        foreach (var snake in gameState.Snakes)
        {
            DrawSnake(snake);
        }
    }

    private void DrawSnake(Snake snake)
    {
        for (var i = 0; i < snake.Body.Count; i++)
        {
            var segment = snake.Body[i];
            var symbol = (i == 0) ? GetHeadSymbol(snake.CurrentDirection) : '●';
            ConsoleColor color = (i == 0) ? snake.Color : GetBodyColor(snake.Color);
            DrawPixel(segment.X, segment.Y, color, symbol);
        }
    }

    private static ConsoleColor GetBodyColor(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Green => ConsoleColor.DarkGreen,
            ConsoleColor.Red => ConsoleColor.DarkRed,
            ConsoleColor.Yellow => ConsoleColor.DarkYellow,
            ConsoleColor.Blue => ConsoleColor.DarkBlue,
            ConsoleColor.Cyan => ConsoleColor.DarkCyan,
            ConsoleColor.Magenta =>  ConsoleColor.DarkMagenta,
            _ => ConsoleColor.DarkGray,
        };
    }
    
    private static char GetHeadSymbol(Direction direction)
    {
        return direction switch
        {
            Direction.Up => '▲',
            Direction.Right => '►',
            Direction.Left => '◄',
            Direction.Down => '▼',
            _ => '●'
        };
    }

    private void DrawAllFood()
    {
        foreach (var food in gameState.Food)
        {
            DrawFood(food);
        }
    }

    private void DrawFood(Food food)
    {        
        var symbol = food.Type == FoodType.Normal ? '♦' : '★';
        ConsoleColor color = food.Type == FoodType.Normal ? ConsoleColor.Red : ConsoleColor.Magenta;
        DrawPixel(food.Position.X, food.Position.Y, color, symbol);
    }

    private void SaveCurrentState()
    {
        _previousSnakePositions = gameState.Snakes.SelectMany(snake => snake.Body).ToList();
        _previousFoodPositions = gameState.Food.Select(food => food.Position).ToList();
    }

    private void DrawText(string text, int startX, int y, ConsoleColor color)
    {
        for (var i = 0; i < text.Length && startX + i < Console.WindowWidth; i++)
        {
            DrawPixel(startX + i, y, color, text[i]);
        }
    }
    
    private void DrawPixel(int x, int y, ConsoleColor color, char symbol)
    {
        if (x < 0 || y < 0 || x >= Console.WindowWidth || y >= Console.WindowHeight) return;
        if (_previousFrame[x, y] == symbol) return;
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = color;
        Console.Write(symbol);
        _previousFrame[x, y] = symbol;
    }
    
    private bool IsPositionOccupied(Point position)
    {
        return gameState.Snakes.Any(snake => snake.Body.Contains(position));
    }

    private bool IsInsideField(Point position)
    {
        return position.X > 0 && position.X < gameState.FieldWidth - 1 && 
               position.Y > 0 && position.Y < gameState.FieldHeight - 1;
    }

}