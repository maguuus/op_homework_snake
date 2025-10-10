namespace SnakeGame;

public enum Direction
{
    Up,
    Down,
    Right,
    Left,
    None
}

public class GameState
{
    private readonly System.Threading.Lock _gameStateLock = new();
    private bool _shouldExit = false;
    
    public Snake PlayerSnake { get; set; }
    public int FieldWidth { get; private set; }
    public int FieldHeight { get; private set; }

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
        PlayerSnake = new Snake();
        PlayerSnake.CurrentDirection = Direction.Right;
        PlayerSnake.NextDirection = Direction.Right;

        InitialSnake();
    }

    private void InitialSnake()
    {
        int startX = FieldWidth / 2;
        int startY = FieldHeight / 2;
        for (int i = 0; i < 5; i++)
        {
            PlayerSnake.Body.Add(new Point(startX - i, startY));
        }
    }
}

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Point && X == ((Point)obj).X && Y == ((Point)obj).Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Point a, Point b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Point a, Point b)
    {
        return !a.Equals(b);
    }
}

public class Snake
{
    private readonly System.Threading.Lock _directionLock = new();
    private Direction _currentDirection;
    private Direction _nextDirection;
    
    public List<Point> Body { get; set; } = new List<Point>();

    public Direction CurrentDirection
    {
        get { lock (_directionLock) return _currentDirection; }
        set { lock (_directionLock) _currentDirection = value; }
    }

    public Direction NextDirection
    {
        get { lock (_directionLock) return _nextDirection; } 
        set { lock (_directionLock) _nextDirection = value; }
    }

    public void UpdateDirection()
    {
        lock (_directionLock)
        {
            _currentDirection = _nextDirection;
        }
    }

    public bool CanChangeDirection(Direction direction)
    {
        lock (_directionLock)
        {
            return (direction == Direction.Up && _currentDirection != Direction.Down) ||
                   (direction == Direction.Down &&  _currentDirection != Direction.Up) ||
                   (direction == Direction.Right && _currentDirection != Direction.Left) ||
                   (direction == Direction.Left &&  _currentDirection != Direction.Right);
        }
    }
}