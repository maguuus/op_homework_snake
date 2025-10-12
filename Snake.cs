namespace SnakeGame;

public enum Direction
{
    Up,
    Down,
    Right,
    Left,
    None
}

public class Snake
{
    private readonly System.Threading.Lock _directionLock = new();
    private Direction _currentDirection;
    private Direction _nextDirection;
    
    public List<Point> Body { get; set; } = new List<Point>();
    public bool IsPlayer { get; }
    public int PlayerId { get; }
    public ConsoleColor Color { get; set; }

    public Snake(int playerId = 0, bool isPlayer = false)
    {
        PlayerId = playerId;
        IsPlayer = isPlayer;
        Color = isPlayer ? ConsoleColor.Green : ConsoleColor.Blue;
        _currentDirection = Direction.Right;
        _nextDirection = Direction.Right;
    }

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