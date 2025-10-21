using op_homework_snake_game.Entities;
using op_homework_snake_game.Enums;
using op_homework_snake_game.Input;
using op_homework_snake_game.Rendering;
using op_homework_snake_game.UI;

namespace op_homework_snake_game.Core;

public class Game
{
    private GameState _gameState;
    private GameRenderer _renderer;
    private readonly GameMode _gameMode;
    private Thread? _inputThread;
    private bool _isRunning;
    private bool _isPaused;
    private readonly HighscoreManager _highscoreManager;
    private readonly Menu _menu;
    private readonly string _currentMap;
    
    public Game(HighscoreManager highscoreManager, GameMode gameMode, string currentMap)
    {
        _gameMode = gameMode;
        _gameState = new GameState(_gameMode, currentMap);
        _currentMap = currentMap;
        _renderer = new GameRenderer(_gameState);
        _highscoreManager = highscoreManager;
        _menu = new Menu();
    }

    public void Start()
    {
        bool restartReq;
        do
        {
            _isRunning = true;
            _isPaused = false;
            _gameState = new GameState(_gameMode, _currentMap);
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
        if (_highscoreManager.IsHighscore(finalScore, _gameMode) && (_gameMode != GameMode.MultiPlayer || _gameState.GameResult == GameResult.Victory))
        {
            string playerName = _gameMode == GameMode.SinglePlayer 
                ? _menu.GetPlayerName(finalScore, _gameState.WinnerLength ?? 0, _gameMode, 1) 
                : _menu.GetPlayerName(finalScore, _gameState.WinnerLength?? 0, _gameMode, _gameState.Snakes.First().PlayerId);
            _highscoreManager.AddScore(playerName, finalScore, _gameState.WinnerLength?? 0, _gameMode); 
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
        if (_highscoreManager.IsHighscore(_gameState.Score, _gameMode))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NEW HIGHSCORE!");
            Console.ResetColor();
        }
        else
        {
            int minTopScore = _highscoreManager.GetMinimumTopScore(_gameMode);
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
            ConsoleKey.W => new InputCommand(InputCommandType.MoveUp),
            ConsoleKey.S => new InputCommand(InputCommandType.MoveDown),
            ConsoleKey.A => new InputCommand(InputCommandType.MoveLeft),
            ConsoleKey.D => new InputCommand(InputCommandType.MoveRight),
            
            ConsoleKey.UpArrow => new InputCommand(InputCommandType.MoveUp, 1),
            ConsoleKey.DownArrow => new InputCommand(InputCommandType.MoveDown, 1),
            ConsoleKey.LeftArrow => new InputCommand(InputCommandType.MoveLeft, 1),
            ConsoleKey.RightArrow => new InputCommand(InputCommandType.MoveRight, 1),
            
            ConsoleKey.Escape => new InputCommand(InputCommandType.ExitGame),
            ConsoleKey.P or ConsoleKey.Spacebar => new InputCommand(InputCommandType.PauseGame),
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

        bool isReversed = _gameState.HasEffect(FoodType.Reverse, snake.PlayerId);
        switch (command.Type)
        {
            case InputCommandType.MoveUp:
                if (snake.CanChangeDirection(Direction.Up, isReversed))
                {
                    snake.NextDirection = isReversed ? Direction.Down : Direction.Up;
                    return true;
                }

                break;
            case InputCommandType.MoveDown:
                if (snake.CanChangeDirection(Direction.Down, isReversed))
                {
                    snake.NextDirection = isReversed ? Direction.Up : Direction.Down;
                    return true;
                }

                break;
            case InputCommandType.MoveLeft:
                if (snake.CanChangeDirection(Direction.Left, isReversed))
                {
                    snake.NextDirection = isReversed ? Direction.Right : Direction.Left;
                    return true;
                }

                break;
            case InputCommandType.MoveRight:
                if (snake.CanChangeDirection(Direction.Right, isReversed)) {
                    snake.NextDirection = isReversed ? Direction.Left : Direction.Right;
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

    private int CalculateSleepTime()
    {
        var player1Snake = _gameState.Snakes.FirstOrDefault(s => s.PlayerId == 0);
        if (player1Snake == null) return GameConfig.MinSpeed;
        int baseSleepTime = GetBaseSleepTime(player1Snake.Body.Count);
        
        if (_gameState.HasEffect(FoodType.Speed, 0))
            baseSleepTime = (int) (baseSleepTime * 0.6);
        if (_gameState.HasEffect(FoodType.Slow, 0))
            baseSleepTime = (int) (baseSleepTime * 1.4);
        return Math.Max(100, baseSleepTime);
    }

    private int GetBaseSleepTime(int snakeLength)
    {
        return snakeLength switch
        {
            < 10 => GameConfig.MinSpeed,
            > 30 => GameConfig.MaxSpeed,
            _ => GameConfig.MinSpeed - (snakeLength - 10) * (GameConfig.MinSpeed - GameConfig.MaxSpeed) / 20
        };
    }
    private void UpdateGame()
    {
        _gameState.UpdateEffects();
        
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

            bool wallCollision = _gameState.IsWallCollision(newHead);
            bool selfCollision = snake.Body.Skip(1).Any(segment => segment == newHead);
            bool otherSnakesCollision = _gameState.Snakes.Where(s => s != snake).Any(other => other.Body.Any(segment => segment == newHead));
            bool hasShield = _gameState.HasEffect(FoodType.Shield, snake.PlayerId);
            bool shouldDieToWall = wallCollision && !hasShield;
            
            if (shouldDieToWall || selfCollision || otherSnakesCollision)
            {
                deadSnakes.Add(snake);
                continue;
            }
            bool outOfBounds = newHead.X <= _gameState.MapOffsetX || newHead.X >= _gameState.MapOffsetX + _gameState.MapWidth - 1 || 
                               newHead.Y <= _gameState.MapOffsetY || newHead.Y >= _gameState.MapOffsetY + _gameState.MapHeight - 1;

            if (outOfBounds)
            {
                if (hasShield)
                {
                    newHead = TeleportToOppositeSide(newHead);
                }
                else
                {
                    deadSnakes.Add(snake);
                    continue;
                }
            }
            
            
            FoodType ateFood = _gameState.TryEatFood(newHead, snake.PlayerId);
            snake.Body.Insert(0, newHead);
            if (ateFood == FoodType.None)
            {
                snake.Body.RemoveAt(snake.Body.Count - 1);
            }
            else
            {
                _gameState.GenerateFood(1);
                if ((_gameState.ActiveEffects.Count(effect => effect.EffectType == FoodType.Double && effect.PlayerId == snake.PlayerId) == 1 && ateFood != FoodType.Double) || 
                    (_gameState.ActiveEffects.Count(effect => effect.EffectType == FoodType.Double && effect.PlayerId == snake.PlayerId) > 1))
                {
                    var tail = snake.Body[^1];
                    snake.Body.Add(new Point(tail.X, tail.Y));
                    _gameState.ActiveEffects.Remove(_gameState.ActiveEffects.First(e => e.EffectType == FoodType.Double));
                }
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

    private Point TeleportToOppositeSide(Point position)
    {
        var x = position.X;
        var y = position.Y;
        int left = _gameState.MapOffsetX;
        int right = left + _gameState.MapWidth - 1;
        int top = _gameState.MapOffsetY;
        int bottom = top + _gameState.MapHeight - 1;
        
        if (x < left) x = right;
        else if (x > right) x = left;
        
        if (y < top) y = bottom;
        else if (y > bottom) y = top;
        
        return new Point(x, y);
    }
}