namespace SnakeGame;

public class GameRenderer
{
    private GameState _gameState;
    private char[,] _previousFrame;
    private bool _firstRender = true;
    private List<Point> _previousSnakePositions = new List<Point>();
    private List<Point> _previousFoodPositions = new List<Point>();
    public GameRenderer(GameState gameState)
    {
        _gameState = gameState;
        _previousFrame = new char[Console.WindowWidth, Console.WindowHeight];
    }

    public void RenderInitialScreen()
    {
        Console.Clear();
        Console.WriteLine("=== Snake Game ===");
        Console.WriteLine($"Field size: {_gameState.FieldWidth}x{_gameState.FieldHeight}");
        Console.WriteLine("Control: ESC - exit");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);

        InitialPreviousFrame();
    }

    private void InitialPreviousFrame()
    {
        for (int y = 0; y < _gameState.FieldHeight; y++)
        {
            for (int x = 0; x < _gameState.FieldWidth; x++)
            {
                _previousFrame[x, y] = ' ';
            }
        }
    }
    
    public void Render()
    {
        if (_firstRender)
        {
            Console.Clear();
            _firstRender = false;
        }

        ClearOldPositions();
        DrawGameObjects();
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

    private void DrawGameObjects()
    {
        DrawBorders();
        DrawAllSnake();
        DrawAllFood();
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
        string info = "Snake game | ESC to exit | Movement - ↑↓→← arrows/WASD";
        string snakeInfo = $"Length: {_gameState.PlayerSnake?.Body.Count} | Direction: {_gameState.PlayerSnake?.CurrentDirection}";
        string gameInfo = $"Score: {_gameState.Score} | Speed: {GetSpeedDescription()}";
        int infoX = Math.Max(0, (_gameState.FieldWidth - info.Length) / 2);
        int snakeInfoX = Math.Max(0, (_gameState.FieldWidth - snakeInfo.Length) / 2);
        int gameInfoX = Math.Max(0, (_gameState.FieldWidth - gameInfo.Length) / 2);
        ClearLine(_gameState.FieldHeight + 1);
        ClearLine(_gameState.FieldHeight + 2);
        ClearLine(_gameState.FieldHeight + 3);
        DrawText(info, infoX, _gameState.FieldHeight + 1, ConsoleColor.Gray);
        DrawText(snakeInfo, snakeInfoX, _gameState.FieldHeight + 2, ConsoleColor.Yellow);
        DrawText(gameInfo, gameInfoX, _gameState.FieldHeight + 3, ConsoleColor.Cyan);
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < _gameState.FieldWidth; x++)
        {
            DrawPixel(x, y, ConsoleColor.Black, ' ');
        }
    } 
    private string GetSpeedDescription()
    {
        int snakeLength = _gameState.PlayerSnake?.Body.Count ?? 0;
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

    private void DrawAllSnake()
    {
        foreach (var snake in _gameState.Snakes)
        {
            DrawSnake(snake);
        }
    }

    private void DrawSnake(Snake snake)
    {
        for (int i = 0; i < snake.Body.Count; i++)
        {
            var segment = snake.Body[i];
            char symbol = (i == 0) ? GetHeadSymbol(snake.CurrentDirection) : '●';
            ConsoleColor color = (i == 0) ? ConsoleColor.Green : ConsoleColor.DarkGreen;
            DrawPixel(segment.X, segment.Y, color, symbol);
        }
    }

    private char GetHeadSymbol(Direction direction)
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
        foreach (var food in _gameState.Food)
        {
            DrawFood(food);
        }
    }

    private void DrawFood(Food food)
    {        
        char symbol = food.Type == FoodType.Normal ? '♦' : '★';
        ConsoleColor color = food.Type == FoodType.Normal ? ConsoleColor.Red : ConsoleColor.Magenta;
        DrawPixel(food.Position.X, food.Position.Y, color, symbol);
    }

    private void SaveCurrentState()
    {
        _previousSnakePositions = _gameState.Snakes.SelectMany(snake => snake.Body).ToList();
        _previousFoodPositions = _gameState.Food.Select(food => food.Position).ToList();
    }

    private void DrawText(string text, int startX, int y, ConsoleColor color)
    {
        for (int i = 0; i < text.Length && startX + i < Console.WindowWidth; i++)
        {
            DrawPixel(startX + i, y, color, text[i]);
        }
    }
    
    private void DrawPixel(int x, int y, ConsoleColor color, char symbol)
    {
        if (x >= 0 && y >= 0 && x < Console.WindowWidth && y < Console.WindowHeight)
        {
            if (_previousFrame[x, y] != symbol)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = color;
                Console.Write(symbol);
                _previousFrame[x, y] = symbol;
            }
        }
    }
    
    private bool IsPositionOccupied(Point position)
    {
        return _gameState.Snakes.Any(snake => snake.Body.Contains(position));
    }

    private bool IsInsideField(Point position)
    {
        return position.X > 0 && position.X < _gameState.FieldWidth - 1 && 
               position.Y > 0 && position.Y < _gameState.FieldHeight - 1;
    }

}