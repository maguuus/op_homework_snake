using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Entities;

public class Snake(int playerId = 0, bool isPlayer = false)
{
    private readonly Lock _directionLock = new();
    private Direction _currentDirection = Direction.Right;
    private Direction _nextDirection = Direction.Right;
    
    public List<Point> Body { get; set; } = [];
    public bool IsPlayer { get; } = isPlayer;
    public int PlayerId { get; } = playerId;
    public ConsoleColor Color { get; init; } = isPlayer ? ConsoleColor.Green : ConsoleColor.Blue;

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

    public bool CanChangeDirection(Direction direction, bool isReversed = false)
    {
        lock (_directionLock)
        {
            if (isReversed)
            {
                return (direction == Direction.Up && _currentDirection != Direction.Up) ||
                       (direction == Direction.Down &&  _currentDirection != Direction.Down) ||
                       (direction == Direction.Right && _currentDirection != Direction.Right) ||
                       (direction == Direction.Left &&  _currentDirection != Direction.Left); 
            }
            return (direction == Direction.Up && _currentDirection != Direction.Down) ||
                   (direction == Direction.Down &&  _currentDirection != Direction.Up) ||
                   (direction == Direction.Right && _currentDirection != Direction.Left) ||
                   (direction == Direction.Left &&  _currentDirection != Direction.Right);
        }
    }
}