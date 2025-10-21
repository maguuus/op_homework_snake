using op_homework_snake_game.Entities;
using op_homework_snake_game.Enums;
using op_homework_snake_game.Input;

namespace op_homework_snake_game.Core;

public class GameState
{
    private readonly Lock _gameStateLock = new();
    private bool _shouldExit;
    private readonly GameMode _gameMode;
    private readonly Queue<InputCommand> _commandQueue = new();
    private readonly ManualResetEvent _newCommandEvent = new(false);
    
    public List<Snake> Snakes { get; private set;  }
    public List<Food> Food { get; private set; }
    public List<Effect> ActiveEffects { get; } = [];
    public int FieldWidth { get; private set; }
    public int FieldHeight { get; private set; }
    public int Score { get; private set; }
    public GameResult GameResult { get; set; } = GameResult.InProgress;
    public int? WinnerPlayerId { get; set; }
    public int? WinnerLength { get; set; }
    
    public Snake? PlayerSnake
    {
        get
        {
            lock (_gameStateLock)
            {
                return Snakes.FirstOrDefault(s => s.IsPlayer);
            }
        }
    }

    public bool ShouldEndGame
    {
        get
        {
            lock (_gameStateLock) return _shouldExit;
        }
        set
        {
            lock (_gameStateLock) _shouldExit = value; 
        }
    }
    
    public GameState(GameMode gameMode)
    {
        _gameMode = gameMode;
        FieldWidth = Math.Max(GameConfig.MinFieldWidth, Console.WindowWidth - GameConfig.FieldWidthBuffer);
        FieldHeight = Math.Max(GameConfig.MinFieldHeight, Console.WindowHeight - GameConfig.FieldHeightBuffer);
        Snakes = new List<Snake>();
        Food = new List<Food>();
        Score = 0;
        InitializeGame();
    }

    private void InitializeGame()
    {
        Snakes.Clear();
        Food.Clear();
        var playerSnake = new Snake(0, true)
        {
            CurrentDirection = Direction.Left,
            NextDirection = Direction.Left,
            Color = ConsoleColor.Green
        };
        int startX = FieldWidth / 2;
        int startY = FieldHeight / 2;
        for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
        {
            playerSnake.Body.Add(new Point(startX - 5 + i, startY));
        }
        Snakes.Add(playerSnake);
        if (_gameMode == GameMode.MultiPlayer)
        {
            var player2Snake = new Snake(1, true)
            {
                CurrentDirection = Direction.Right,
                NextDirection = Direction.Right,
                Color = ConsoleColor.Blue
            };
            for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
            {
                player2Snake.Body.Add(new Point(startX + 5 + GameConfig.InitialSnakeLength - i, startY));
            }
            Snakes.Add(player2Snake);
        }
        GenerateFood(GameConfig.InitialFoodAmount);
    }

    public void EnqueueCommand(InputCommand command)
    {
        lock (_commandQueue)
        {
            _commandQueue.Enqueue(command);
        }
        _newCommandEvent.Set();
    }

    public List<InputCommand> WaitForCommands(int timeoutMilliseconds)
    {
        _newCommandEvent.WaitOne(timeoutMilliseconds);
        lock (_commandQueue)
        {
            var commands =  _commandQueue.ToList();
            _commandQueue.Clear();
            _newCommandEvent.Reset();
            return commands;
        }
    }

    public void SignalNewCommand()
    {
        _newCommandEvent.Set();
    }
    
    public void GenerateFood(int amount)
    {
        int foodToGenerate = Math.Min(amount, GameConfig.MaxFoodCount - Food.Count);
        for (int i = 0; i < foodToGenerate; i++)
        {
            Point? foodPosition = FindValidFoodPosition();
            if (foodPosition != null)
            {
                FoodType foodType = GetRandomFood();
                Food.Add(new Food(foodPosition, foodType));
            }   
        }
    }

    private FoodType GetRandomFood()
    {
        Random random = new();
        int roll = random.Next(100);
        return roll switch
        {
            < 50 => FoodType.Normal,
            < 51 => FoodType.Bonus,
            < 52 => FoodType.Speed,
            < 53 => FoodType.Slow,
            < 54 => FoodType.Reverse,
            < 55 => FoodType.Shield,
            < 80 => FoodType.Double,
            _ => FoodType.Shrink
        };  
    }

    private Point? FindValidFoodPosition()
    {
        Random random = new();
        int attempts = 0;
        while (attempts < 100)
        {
            Point candidate = new Point(random.Next(1, FieldWidth - 1), random.Next(1, FieldHeight - 1));
            bool collision = Snakes.Any(snake => snake.Body.Contains(candidate)) || Food.Any(f => f.Position == candidate);
            if (!collision)
                return candidate;
            attempts++;
        }

        return null;
    }

    public FoodType TryEatFood(Point position, int playerId)
    {
        var foodToEat = Food.FirstOrDefault(f => f.Position == position);

        if (foodToEat != null)
        {
            Food.Remove(foodToEat);
            Score += foodToEat.ScoreValue;
            if (HasEffect(FoodType.Double, playerId))
            {
                Score += foodToEat.ScoreValue;
            }
            
            ApplyFoodEffect(foodToEat.Type, playerId);
            return foodToEat.Type;
        }
        return FoodType.None;
        
    }

    private void ApplyFoodEffect(FoodType foodType, int playerId)
    {
        switch (foodType) {
            case FoodType.Speed:
                ActiveEffects.Add(new Effect(foodType, 100, playerId));
                break;
            case FoodType.Slow:
                ActiveEffects.Add(new Effect(foodType, 80, playerId));
                break;
            case FoodType.Reverse:
                ActiveEffects.Add(new Effect(foodType, 60, playerId));
                break;
            case FoodType.Shield:
                ActiveEffects.Add(new Effect(foodType, 120, playerId));
                break;
            case FoodType.Double:
                ActiveEffects.Add(new Effect(foodType, 1, playerId));
                break;
            case FoodType.Shrink:
                var snake = Snakes.FirstOrDefault(s => s.PlayerId == playerId);
                if (snake != null && snake.Body.Count > 2)
                {
                    int segmentsToRemove = Math.Min(snake.Body.Count - 2, 4);
                    snake.Body.RemoveRange(snake.Body.Count - segmentsToRemove, segmentsToRemove);
                }
                break;
        }
    }

    public void UpdateEffects()
    {
        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            if (ActiveEffects[i].EffectType != FoodType.Double)
                ActiveEffects[i].Duration--;
            if (ActiveEffects[i].Duration <= 0)
            {
                ActiveEffects.RemoveAt(i);
            }
        }
    }
    
    public bool HasEffect(FoodType effectType, int playerId)
    {
        return ActiveEffects.Any(effect => effect.EffectType == effectType && effect.PlayerId == playerId);
    }
}