namespace SnakeGame;

public class Game
{
    private GameState _gameState;
    private GameRenderer _renderer;
    private Thread? _inputThread;
    private bool _isRunning;
    public readonly HighscoreManager _highscoreManager;
    public readonly Menu _menu;
    public Game(HighscoreManager highscoreManager)
    {
        _gameState = new GameState();
        _renderer = new GameRenderer(_gameState);
        _highscoreManager = highscoreManager;
        _menu = new Menu();
    }

    public void Start()
    {
        _isRunning = true;

        Console.Clear();
        _renderer.RenderInitialScreen();

        _inputThread = new Thread(HandleInput)
        {
            IsBackground = true
        };
        _inputThread.Start();

        GameLoop();
        
        CheckHighscore();
        ShowGameOverScreen();
    }

    private void CheckHighscore()
    {
        int finalScore = _gameState.Score;
        int finalLength = _gameState.PlayerSnake.Body.Count;
        if (_highscoreManager.IsHighscore(finalScore))
        {
            string? playerName = _menu.GetPlayerName(finalScore, finalLength);
            if (playerName != null)
            {
                _highscoreManager.AddScore(playerName, finalScore, finalLength);
            }
            
        }
    }

    private void ShowGameOverScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("GAME OVER");
        Console.ResetColor();
        Console.WriteLine($"Final score: {_gameState.Score}");
        Console.WriteLine($"Final length: {_gameState.PlayerSnake.Body.Count}");
        Console.WriteLine();
        if (_highscoreManager.IsHighscore(_gameState.Score))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NEW HIGHSCORE!");
            Console.ResetColor();
        }
        else
        {
            int minTopScore = _highscoreManager.GetMinimumTopScore();
            Console.WriteLine($"Top 10 minimum: {minTopScore} points");
        }
        Console.WriteLine();
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey(true);
    }

    private void HandleInput()
    {
        while (_isRunning && !_gameState.ShouldEndGame)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                ProcessKeyPress(key.Key);
            }
            Thread.Sleep(GameConfig.InputPollingInterval);
        }
    }

    private void ProcessKeyPress(ConsoleKey key)
    {
        InputCommand? command = key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => new InputCommand(InputCommandType.MoveUp, 0),
            ConsoleKey.DownArrow or ConsoleKey.S => new InputCommand(InputCommandType.MoveDown, 0),
            ConsoleKey.LeftArrow or ConsoleKey.A => new InputCommand(InputCommandType.MoveLeft, 0),
            ConsoleKey.RightArrow or ConsoleKey.D => new InputCommand(InputCommandType.MoveRight, 0),
            ConsoleKey.Escape => new InputCommand(InputCommandType.ExitGame, 0),
            _ => null
        };
        if (command != null)
        {
            _gameState.EnqueueCommand(command);
        }
    }

    private void GameLoop()
    {
        while (_isRunning && !_gameState.ShouldEndGame)
        {
            DateTime frameStart = DateTime.Now;
            ProcessInputCommands();
            UpdateGame();
            _renderer.Render();
            TimeSpan frameTime = DateTime.Now - frameStart;
            int sleepTime = CalculateSleepTime();
            int sleep = Math.Max(0, sleepTime - (int)frameTime.TotalMilliseconds);
            Thread.Sleep(sleep);
        }
    }

    private void ProcessInputCommands()
    {
        var commands = _gameState.WaitForCommands(GameConfig.CommandProcessingTimeout);
        var commandsByPlayer = commands.GroupBy(c => c.PlayerId);

        foreach (var playerCommands in commandsByPlayer)
        {
            var snake = _gameState.Snakes.FirstOrDefault(s => s.PlayerId == playerCommands.Key);
            if (snake != null)
            {
                ProcessCommandsForSnake(playerCommands.ToList(), snake);
            }
        }
    }

    private void ProcessCommandsForSnake(List<InputCommand> commands, Snake snake)
    {
        foreach (var command in commands)
        {
            if (TryCommandToSnake(command, snake))
            {
                break;
            }
        }
    }
    private bool TryCommandToSnake(InputCommand command, Snake snake)
    {
        switch (command.Type)
        {
            case InputCommandType.MoveUp:
                if (snake.CanChangeDirection(Direction.Up))
                {
                    snake.NextDirection = Direction.Up;
                    return true;
                }

                break;
            case InputCommandType.MoveDown:
                if (snake.CanChangeDirection(Direction.Down))
                {
                    snake.NextDirection = Direction.Down;
                    return true;
                }

                break;
            case InputCommandType.MoveLeft:
                if (snake.CanChangeDirection(Direction.Left))
                {
                    snake.NextDirection = Direction.Left;
                    return true;
                }

                break;
            case InputCommandType.MoveRight:
                if (snake.CanChangeDirection(Direction.Right)) {
                    snake.NextDirection = Direction.Right;
                    return true;
                }
                break;
            case InputCommandType.ExitGame:
                _gameState.ShouldEndGame = true;
                _isRunning = false;
                return true;
        }
        return false;
    }

    public int CalculateSleepTime()
    {
        var playerSnake = _gameState.PlayerSnake;
        if (playerSnake == null) return GameConfig.MinSpeed;
        
        int snakeLength = playerSnake.Body.Count;
        if (snakeLength < 10)
        {
            return GameConfig.MinSpeed;
        }
        else if (snakeLength > 30)
        {
            return GameConfig.MaxSpeed;
        }
        return GameConfig.MinSpeed - (snakeLength - 10) * (GameConfig.MinSpeed - GameConfig.MaxSpeed) / 20;
    }
    
    private void UpdateGame()
    {
        _gameState.PlayerSnake?.UpdateDirection();
        foreach (var snake in _gameState.Snakes)
        {
            if (snake.Body.Count == 0) continue;
            var head = snake.Body[0];
            Point newHead = head;

            switch (snake.CurrentDirection)
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
            bool selfCollision = snake.Body.Skip(1).Any(segment => segment == newHead);
            bool otherSnakesCollision = _gameState.Snakes.Where(s => s != snake).Any(other => other.Body.Any(segment => segment == newHead));
            if (selfCollision || otherSnakesCollision)
            {
                _isRunning = false;
                return;
            }
        
            bool ateFood = _gameState.TryEatFood(newHead);
            snake.Body.Insert(0, newHead);
            if (!ateFood)
            {
                snake.Body.RemoveAt(snake.Body.Count - 1);
            }
            else
            {
                _gameState.GenerateFood(1);
            } 
        }

    }
}