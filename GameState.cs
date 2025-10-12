namespace SnakeGame;

public class GameState
{
    private readonly System.Threading.Lock _gameStateLock = new();
    private bool _shouldExit = false;
    private readonly Queue<InputCommand> _commandQueue = new();
    private readonly ManualResetEvent _newCommandEvent = new(false);
    
    public List<Snake> Snakes { get; private set;  }
    public List<Food> Food { get; private set; }
    public int FieldWidth { get; private set; }
    public int FieldHeight { get; private set; }
    public int Score { get; private set; }
    
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
    
    public GameState()
    {
        FieldWidth = Math.Max(10, Console.WindowWidth - 10);
        FieldHeight = Math.Max(10, Console.WindowHeight - 5);
        Snakes = new List<Snake>();
        Food = new List<Food>();
        Score = 0;
        InitializeGame();
    }

    private void InitializeGame()
    {
        var playerSnake = new Snake(0, true);
        playerSnake.CurrentDirection = Direction.Right;
        playerSnake.NextDirection = Direction.Right;
        playerSnake.Color = ConsoleColor.Green;
        int startX = FieldWidth / 2;
        int startY = FieldHeight / 2;
        for (int i = 0; i < 5; i++)
        {
            playerSnake.Body.Add(new Point(startX - i, startY));
        }
        var player2Snake = new Snake(1, true);
        player2Snake.CurrentDirection = Direction.Up;
        player2Snake.NextDirection = Direction.Up;
        player2Snake.Color = ConsoleColor.Blue;
        for (int i = 0; i < 5; i++)
        {
            player2Snake.Body.Add(new Point(startX + 5 + i, startY));
        }
        Snakes.Add(playerSnake);
        Snakes.Add(player2Snake);
        GenerateFood(3);
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
        for (int i = 0; i < amount; i++)
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