namespace SnakeGame;

public class GameState
{
    private readonly System.Threading.Lock _gameStateLock = new();
    private bool _shouldExit = false;
    private readonly GameMode _gameMode;
    private readonly Queue<InputCommand> _commandQueue = new();
    private readonly ManualResetEvent _newCommandEvent = new(false);
    
    public List<Snake> Snakes { get; private set;  }
    public List<Food> Food { get; private set; }
    public int FieldWidth { get; private set; }
    public int FieldHeight { get; private set; }
    public int Score { get; set; }
    public GameResult GameResult { get; set; } = GameResult.InProgress;
    public int? WinnerPlayerId { get; set; }
    public int? WinnerLength { get; set; }
    public Snake? PlayerSnake
    {
        get
        {
            lock (_gameStateLock)
            {
                return Snakes?.FirstOrDefault(s => s.IsPlayer);
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
        // GameRende
        var playerSnake = new Snake(0, true);
        playerSnake.CurrentDirection = Direction.Right;
        playerSnake.NextDirection = Direction.Right;
        playerSnake.Color = ConsoleColor.Green;
        int startX = FieldWidth / 2;
        int startY = FieldHeight / 2;
        for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
        {
            playerSnake.Body.Add(new Point(startX - i, startY));
        }
        Snakes.Add(playerSnake);
        if (_gameMode == GameMode.MultiPlayer)
        {
            var player2Snake = new Snake(1, true);
            player2Snake.CurrentDirection = Direction.Up;
            player2Snake.NextDirection = Direction.Up;
            player2Snake.Color = ConsoleColor.Blue;
            for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
            {
                player2Snake.Body.Add(new Point(startX + 5 + i, startY));
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
                Food.Add(new Food(foodPosition));
            }   
        }
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

    public bool TryEatFood(Point position)
    {
        var foodToEat = Food.FirstOrDefault(f => f.Position == position);

        if (foodToEat != null)
        {
            Food.Remove(foodToEat);
            Score++;
            return true;
        }
        return false;
        
    }
}