namespace SnakeGame;

public class GameRenderer
{
    private GameState _gameState;
    public GameRenderer(GameState gameState)
    {
        _gameState = gameState;
    }

    public void RenderInitialScreen()
    {
        Console.Clear();
        Console.WriteLine("=== Snake Game ===");
        Console.WriteLine($"Field size: {_gameState.FieldWidth}x{_gameState.FieldHeight}");
        Console.WriteLine("Control: ESC - exit");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }

    public void Render()
    {
        Console.Clear();
        DrawBorders();
        DrawSnake();
        DrawFood();
        DrawInfo();
    }
    private void DrawBorders()
    {
        for (int x = 0; x < _gameState.FieldWidth; x++)
        {
            DrawPixel(x, 0, ConsoleColor.White, '═');
            DrawPixel(x, _gameState.FieldHeight - 1, ConsoleColor.White, '═');
        }

        for (int y = 0; y < _gameState.FieldHeight; y++)
        {
            DrawPixel(0, y, ConsoleColor.White, '║');
            DrawPixel(_gameState.FieldWidth - 1, y, ConsoleColor.White, '║');
        }
        
        DrawPixel(0, 0, ConsoleColor.White, '╔');
        DrawPixel(_gameState.FieldWidth - 1, 0, ConsoleColor.White, '╗');
        DrawPixel(0, _gameState.FieldHeight - 1, ConsoleColor.White, '╚');
        DrawPixel(_gameState.FieldWidth - 1, _gameState.FieldHeight - 1, ConsoleColor.White, '╝');
    }

    private void DrawInfo()
    {
        string info = "Snake game | ESC to exit | Movement - arrows/WASD";
        string snakeInfo = $"Length: {_gameState.PlayerSnake.Body.Count} | Direction: {_gameState.PlayerSnake.CurrentDirection}";
        string gameInfo = $"Score: {_gameState.Score} | Speed: {GetSpeedDesription()}";
        int infoX = Math.Max(0, (_gameState.FieldWidth - info.Length) / 2);
        int snakeInfoX = Math.Max(0, (_gameState.FieldWidth - snakeInfo.Length) / 2);
        int gameInfoX = Math.Max(0, (_gameState.FieldWidth - gameInfo.Length) / 2);
        for (int i = 0; i < info.Length && infoX + i < _gameState.FieldWidth; i++)
        {
            DrawPixel(infoX + i, _gameState.FieldHeight + 1, ConsoleColor.Gray, info[i]);
        }

        for (int i = 0; i < snakeInfo.Length && snakeInfoX + i < _gameState.FieldWidth; i++)
        {
            DrawPixel(snakeInfoX + i, _gameState.FieldHeight + 2, ConsoleColor.Yellow, snakeInfo[i]);
        }
        
        for (int i = 0; i < gameInfo.Length && gameInfoX + i < _gameState.FieldWidth; i++)
        {
            DrawPixel(gameInfoX + i, _gameState.FieldHeight + 3, ConsoleColor.Cyan, gameInfo[i]);
        }
    }

    private string GetSpeedDesription()
    {
        int snakeLength = _gameState.PlayerSnake.Body.Count;
        if (snakeLength <= 10)
        {
            return "Slow";
        }
        else if (snakeLength <= 20)
        {
            return "Medium";
        }
        else if (snakeLength <= 30)
        {
            return "Fast";
        }

        return "Very Fast";
    }

    private void DrawSnake()
    {
        for (int i = 0; i < _gameState.PlayerSnake.Body.Count; i++)
        {
            var segment = _gameState.PlayerSnake.Body[i];
            char symbol = (i == 0) ? GetHeadSymbol() : '●';
            ConsoleColor color = (i == 0) ? ConsoleColor.Green : ConsoleColor.DarkGreen;
            DrawPixel(segment.X, segment.Y, color, symbol);
        }
    }

    private char GetHeadSymbol()
    {
        return _gameState.PlayerSnake.CurrentDirection switch
        {
            Direction.Up => '▲',
            Direction.Right => '►',
            Direction.Left => '◄',
            Direction.Down => '▼',
            _ => '●'
        };
    }

    private void DrawFood()
    {
        foreach (var food in _gameState.Food)
        {
            DrawPixel(food.X, food.Y, ConsoleColor.Red, '♦');
        }
    }
    
    private void DrawPixel(int x, int y, ConsoleColor color, char symbol)
    {
        if (x >= 0 && y >= 0 && x < Console.WindowWidth && y < Console.WindowHeight)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }
    }
}