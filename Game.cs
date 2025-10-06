namespace SnakeGame;

public class Game
{
    private GameState _gameState;
    private GameRenderer _renderer;
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
        while (_isRunning)
        {
            DateTime frameStart = DateTime.Now;
            
            HandleInput();
            UpdateGame();
            _renderer.Render();

            TimeSpan frameTime = DateTime.Now - frameStart;
            int sleep = Math.Max(0, 600 - (int)frameTime.TotalMilliseconds);
            Thread.Sleep(sleep);
        }

        Console.Clear();
        Console.WriteLine("Game Over");
        Thread.Sleep(1000);
    }

    private void HandleInput()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    if (_gameState.Snake.CurrentDirection != Direction.Down)
                        _gameState.Snake.NextDirection = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    if (_gameState.Snake.CurrentDirection != Direction.Up)
                        _gameState.Snake.NextDirection = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    if (_gameState.Snake.CurrentDirection != Direction.Right)
                        _gameState.Snake.NextDirection = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    if (_gameState.Snake.CurrentDirection != Direction.Left)
                        _gameState.Snake.NextDirection = Direction.Right;
                    break;
                case ConsoleKey.Escape:
                    _isRunning = false;
                    break;
            }
        }
    }


    private void UpdateGame()
    {
        _gameState.Snake.CurrentDirection = _gameState.Snake.NextDirection;

        var head = _gameState.Snake.Body[0];
        Point newHead = head;

        switch (_gameState.Snake.CurrentDirection)
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

        if (_gameState.Snake.Body.Skip(1).Any(segment => segment == newHead))
        {
            _isRunning = false;
            return;
        }
        
        _gameState.Snake.Body.Insert(0, newHead); 
        _gameState.Snake.Body.RemoveAt(_gameState.Snake.Body.Count - 1);
    }
}