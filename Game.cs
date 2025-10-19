namespace SnakeGame;

public class Game
{
    private GameState _gameState;
    private GameRenderer _renderer;
    private readonly GameMode _gameMode;
    private Thread? _inputThread;
    private bool _isRunning;
    private bool _isPaused;
    public readonly HighscoreManager HighscoreManager;
    public readonly Menu Menu;
    public Game(HighscoreManager highscoreManager, GameMode gameMode)
    {
        _gameMode = gameMode;
        _gameState = new GameState(_gameMode);
        _renderer = new GameRenderer(_gameState);
        HighscoreManager = highscoreManager;
        Menu = new Menu();
    }

    public void Start()
    {
        bool restartReq = false;
        do
        {
            _isRunning = true;
            _isPaused = false;
            _gameState = new GameState(_gameMode);
            Console.Clear();
            _renderer = new GameRenderer(_gameState);
            _renderer.RenderInitialScreen();

            _inputThread = new Thread(HandleInput)
            {
                IsBackground = true
            };
            _inputThread.Start();

            GameLoop();

            CheckHighscore();
            ShowGameOverScreen();

            Console.WriteLine("Press R to restart, any other key for menu");
            restartReq = Console.ReadKey(true).Key == ConsoleKey.R;
            _isRunning = false;
            _inputThread?.Join(100);
        } while (restartReq);
    }

    private void CheckHighscore()
    {
        int finalScore = _gameState.Score;
        if (HighscoreManager.IsHighscore(finalScore, _gameMode) && (_gameMode != GameMode.MultiPlayer || _gameState.GameResult == GameResult.Victory))
        {
            string? playerName = _gameMode == GameMode.SinglePlayer 
                ? Menu.GetPlayerName(finalScore, _gameState.WinnerLength ?? 0, _gameMode, 1) 
                : Menu.GetPlayerName(finalScore, _gameState.WinnerLength?? 0, _gameMode, _gameState.Snakes.First().PlayerId);;
            HighscoreManager.AddScore(playerName, finalScore, _gameState.WinnerLength?? 0, _gameMode); 
        }
    }

    private void ShowGameOverScreen()
    {
        Console.Clear();
        if (_gameMode == GameMode.MultiPlayer && _gameState.Snakes.Count <= 1)
        {
            if (_gameState.GameResult == GameResult.Victory)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"PLAYER {_gameState.WinnerPlayerId + 1} WINS!");
                Console.ResetColor();
                Console.WriteLine(
                    $"Player {_gameState.WinnerPlayerId + 1} survived with length: {_gameState.Snakes.First().Body.Count}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("DRAW GAME!");
                Console.ResetColor();
                Console.WriteLine("Both players were eliminated");
            }
        }
        else {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("GAME OVER");
            Console.ResetColor(); 
        }
        Console.WriteLine($"Final score: {_gameState.Score}");
        Console.WriteLine();
        if (HighscoreManager.IsHighscore(_gameState.Score, _gameMode))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NEW HIGHSCORE!");
            Console.ResetColor();
        }
        else
        {
            int minTopScore = HighscoreManager.GetMinimumTopScore(_gameMode);
            Console.WriteLine($"Top 10 minimum: {minTopScore} points");
        }
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
            ConsoleKey.W => new InputCommand(InputCommandType.MoveUp, 0),
            ConsoleKey.S => new InputCommand(InputCommandType.MoveDown, 0),
            ConsoleKey.A => new InputCommand(InputCommandType.MoveLeft, 0),
            ConsoleKey.D => new InputCommand(InputCommandType.MoveRight, 0),
            
            ConsoleKey.UpArrow => new InputCommand(InputCommandType.MoveUp, 1),
            ConsoleKey.DownArrow => new InputCommand(InputCommandType.MoveDown, 1),
            ConsoleKey.LeftArrow => new InputCommand(InputCommandType.MoveLeft, 1),
            ConsoleKey.RightArrow => new InputCommand(InputCommandType.MoveRight, 1),
            
            ConsoleKey.Escape => new InputCommand(InputCommandType.ExitGame, 0),
            ConsoleKey.P or ConsoleKey.Spacebar => new InputCommand(InputCommandType.PauseGame, 0),
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
            if (!_isPaused)
            {
                UpdateGame();
            }

            _renderer.Render(_isPaused);
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
        if (command.Type == InputCommandType.PauseGame)
        {
            _isPaused = !_isPaused;
            return true;
        }

        if (_isPaused && command.Type == InputCommandType.PauseGame)
        {
            return false;
        }
        
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
                _gameState.WinnerLength = _gameState.Snakes.First().Body.Count; 
                return true;
        }
        return false;
    }

    public int CalculateSleepTime()
    {
        int snakeLength = _gameState.Snakes.Count != 0 ? _gameState.Snakes.Max(s => s.Body.Count) : 0;
        
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
        foreach (var snake in _gameState.Snakes)
        {
            snake.UpdateDirection();
        }
        List<Snake> deadSnakes = new List<Snake>();
        bool gameShouldEnd = false;
        foreach (var snake in _gameState.Snakes)
        {
            if (snake.Body.Count == 0)
            {
                deadSnakes.Add(snake);
                continue;
            }
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

            bool wallCollision = newHead.X <= 0 || newHead.X >= _gameState.FieldWidth - 1 || 
                                 newHead.Y <= 0 || newHead.Y >= _gameState.FieldHeight - 1;
            bool selfCollision = snake.Body.Skip(1).Any(segment => segment == newHead);
            bool otherSnakesCollision = _gameState.Snakes.Where(s => s != snake).Any(other => other.Body.Any(segment => segment == newHead));
            if (wallCollision || selfCollision || otherSnakesCollision)
            {
                deadSnakes.Add(snake);
                continue;
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

        foreach (var deadSnake in deadSnakes)
        {
            _gameState.Snakes.Remove(deadSnake);
        }

        if (_gameMode == GameMode.MultiPlayer)
        {
            if (_gameState.Snakes.Count == 0)
            {
                _gameState.GameResult = GameResult.Draw;
                _gameState.WinnerPlayerId = null;
                gameShouldEnd = true;
            }
            else if (_gameState.Snakes.Count == 1)
            {
                _gameState.GameResult = GameResult.Victory;
                _gameState.WinnerPlayerId = _gameState.Snakes.First().PlayerId;
                _gameState.WinnerLength = _gameState.Snakes.First().Body.Count;
                gameShouldEnd = true;
            } 
        }
        else
        {
            if (deadSnakes.Any())
            {
                _gameState.WinnerLength = deadSnakes.First().Body.Count;
                gameShouldEnd = true;
            }
        }

        if (gameShouldEnd)
        {
            _isRunning = false;
        }
    }
}