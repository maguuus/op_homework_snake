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
    
    public List<Wall> Walls { get; private set; } = [];
    public string CurrentMap {get; private set;} = "arena";
    
    public List<Snake> Snakes { get; private set;  }
    public List<Food> Food { get; private set; }
    public List<Effect> ActiveEffects { get; } = [];
    
    public int FieldWidth { get; private set; }
    public int FieldHeight { get; private set; }
    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }
    public int MapOffsetX { get; private set; }
    public int MapOffsetY { get; private set; }
    
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
    
    public GameState(GameMode gameMode, string mapName = "arena")
    {
        _gameMode = gameMode;
        FieldWidth = Math.Max(GameConfig.MinFieldWidth, Console.WindowWidth - GameConfig.FieldWidthBuffer);
        FieldHeight = Math.Max(GameConfig.MinFieldHeight, Console.WindowHeight - GameConfig.FieldHeightBuffer);
        Snakes = new List<Snake>();
        Food = new List<Food>();
        Score = 0;
        
        LoadMap(mapName);
        InitializeGame();
    }

    public void LoadMap(string mapName)
    {
        MapWidth = FieldWidth;
        MapHeight = FieldHeight;
        Walls.Clear();
        CurrentMap = mapName;
        string mapPath = $"Data/Maps/{mapName}.txt";
        if (!File.Exists(mapPath))
        {
            Console.WriteLine($"Map file {mapName} not found, using empty map");
            MapWidth = 0;
            MapHeight = 0;
            return;
        }

        try
        {
            string[] allLines = File.ReadAllLines(mapPath);
            var lines = allLines.Where(l => !l.TrimStart().StartsWith("/")).ToArray();
            MapHeight = lines.Length;
            MapWidth = lines[0].Length;
            if (MapHeight > FieldHeight - 2 || MapWidth > FieldWidth - 2)
            {
                Console.WriteLine($"Map file {mapPath} is too large, using empty map");
                return;
            }

            MapOffsetX = (FieldWidth - MapWidth) / 2;
            MapOffsetY = (FieldHeight - MapHeight) / 2;

            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    if (lines[y][x] == '#')
                        Walls.Add(new Wall(new Point(x + MapOffsetX, y + MapOffsetY)));
                }
            }
            Console.WriteLine($"Loaded map: {mapName} with {Walls.Count} walls");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading map {mapName}: {ex.Message}");
            MapWidth = 0;
            MapHeight = 0;
        }
    }

    public bool IsWallCollision(Point position)
    {
        return Walls.Any(w => w.Position == position);
    }

    public bool IsPositionFree(Point position)
    {
        bool wallCollision = IsWallCollision(position);
        bool snakeCollision = Snakes.Any(s => s.Body.Contains(position));
        bool foodCollision = Food.Any(f => f.Position == position);
        
        return !(wallCollision ||  snakeCollision || foodCollision);
    }
    
    private void InitializeGame()
    {
        var mapInfo = MapLoader.ParseMapMetadata($"Data/Maps/{CurrentMap}.txt");
        var starts = mapInfo?.SnakeStartPositions ?? [];
        
        Snakes.Clear();
        Food.Clear();
        if (MapWidth < 4 || MapHeight < 4)
        {
            Console.WriteLine($"Map file {CurrentMap} is too small, using empty map");
            MapOffsetX = 1;
            MapOffsetY = 1;
            MapWidth = FieldWidth - 2;
            MapHeight = FieldHeight - 2;
        }
        var playerSnake = new Snake(0, true)
        {
            CurrentDirection = Direction.Left,
            NextDirection = Direction.Left,
            Color = ConsoleColor.Green
        };
        int start1X = starts.Count > 0 ? MapOffsetX + starts[0].X : MapOffsetX + MapWidth / 2 - 5;
        int start1Y = starts.Count > 0 ? MapOffsetY + starts[0].Y : MapOffsetY + MapHeight / 2;
        for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
        {
            playerSnake.Body.Add(new Point(start1X + i, start1Y));
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
            var start2X = starts.Count > 1 ? MapOffsetX + starts[1].X : MapOffsetX + MapWidth / 2 + 5 + GameConfig.InitialSnakeLength;
            var start2Y = starts.Count > 1 ? MapOffsetY + starts[1].Y : MapOffsetY + MapHeight / 2;
            for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
            {
                player2Snake.Body.Add(new Point(start2X - i, start2Y));
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
            < 70 => FoodType.Bonus,
            < 80 => FoodType.Speed,
            < 85 => FoodType.Slow,
            < 90 => FoodType.Reverse,
            < 95 => FoodType.Shield,
            < 98 => FoodType.Double,
            _ => FoodType.Shrink
        };  
    }

    private Point? FindValidFoodPosition()
    {
        Random random = new();
        int attempts = 0;
        while (attempts < 1000)
        {
            int x = random.Next(MapOffsetX + 1, MapOffsetX + MapWidth - 1);
            int y = random.Next(MapOffsetY + 1, MapOffsetY + MapHeight - 1);
            Point candidate = new Point(x, y);
            if (IsPositionFree(candidate))
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