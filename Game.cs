namespace SnakeGame;

public class Game
{
    private GameState _gameState;
    private GameRenderer _renderer;
    private Thread? _inputThread;
    private bool _isRunning;

    public Game()
    {
        _gameState = new GameState();
        _renderer = new GameRenderer(_gameState);
    }

    public void Start()
    {
        _isRunning = true;

        Console.Clear();
        _renderer.RenderInitialScreen();

        _inputThread = new Thread(HandleInput);
        _inputThread.Start();

        GameLoop();
        
        _inputThread.Join();
        
        Console.Clear();
        Console.WriteLine($"Game Over! Your score: {_gameState.Score}");
        Thread.Sleep(1000);
    }

    private void HandleInput()
    {
        while (_isRunning && !_gameState.ShouldEndGame)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                Direction newDirection = _gameState.PlayerSnake.NextDirection;
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        if (_gameState.PlayerSnake.CanChangeDirection(Direction.Up))
                            newDirection = Direction.Up;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        if (_gameState.PlayerSnake.CanChangeDirection(Direction.Down))
                            newDirection = Direction.Down;
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        if (_gameState.PlayerSnake.CanChangeDirection(Direction.Left))
                            newDirection = Direction.Left;
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (_gameState.PlayerSnake.CanChangeDirection(Direction.Right))
                            newDirection = Direction.Right;
                        break;
                    case ConsoleKey.Escape:
                        _gameState.ShouldEndGame = true;
                        _isRunning = false;
                        break;
                }
                
                _gameState.PlayerSnake.NextDirection = newDirection;
            }
            
            Thread.Sleep(10);
        }
    }

    private void GameLoop()
    {
        while (_isRunning && !_gameState.ShouldEndGame)
        {
            DateTime frameStart = DateTime.Now;
            UpdateGame();
            _renderer.Render();
            TimeSpan frameTime = DateTime.Now - frameStart;
            int sleepTime = CalculateSleepTime();
            int sleep = Math.Max(0, sleepTime - (int)frameTime.TotalMilliseconds);
            Thread.Sleep(sleep);
        }
    }

    public int CalculateSleepTime()
    {
        int snakeLength = _gameState.PlayerSnake.Body.Count;
        if (snakeLength < 10)
        {
            return 600;
        }
        else if (snakeLength > 30)
        {
            return 300;
        }
        return 750 - snakeLength * 15;
    }
    
    private void UpdateGame()
    {
        _gameState.PlayerSnake.UpdateDirection();

        var head = _gameState.PlayerSnake.Body[0];
        Point newHead = head;

        switch (_gameState.PlayerSnake.CurrentDirection)
        {
            case Direction.Up:
                newHead = new Point(head.X, head.Y - 1);
                break;
            case Direction.Down:
                newHead = new Point(head.X, head.Y + 1);
                break;
            case Direction.Left:
                newHead = new Point(head.X - 1, head.Y);
                break;
            case Direction.Right:
                newHead = new Point(head.X + 1, head.Y);
                break;
        }

        if (newHead.X <= 0 || newHead.X >= _gameState.FieldWidth - 1 ||
            newHead.Y <= 0 || newHead.Y >= _gameState.FieldHeight - 1)
        {
            _isRunning = false;
            return;
        }

        if (_gameState.PlayerSnake.Body.Skip(1).Any(segment => segment == newHead))
        {
            _isRunning = false;
            return;
        }
        
        bool ateFood = _gameState.TryEatFood(newHead);
        _gameState.PlayerSnake.Body.Insert(0, newHead);
        if (!ateFood)
        {
            _gameState.PlayerSnake.Body.RemoveAt(_gameState.PlayerSnake.Body.Count - 1);
        }
        else
        {
            _gameState.GenerateFood(1);
        }
    }
}