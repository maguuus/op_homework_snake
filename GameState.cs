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
    
    public Snake Snake { get; set; }
    
    public int FieldWidth { get; private set; }
    public int FieldHeight { get; private set; }

    public GameState()
    {
        FieldWidth = Math.Max(10, Console.WindowWidth - 10);
        FieldHeight = Math.Max(10, Console.WindowHeight - 5);
        Snake = new Snake();
        Snake.CurrentDirection = Direction.Right;
        Snake.NextDirection = Direction.Right;

        InitialSnake();
    }

    private void InitialSnake()
    {
        int startX = FieldWidth / 2;
        int startY = FieldHeight / 2;
        for (int i = 0; i < 5; i++)
        {
            Snake.Body.Add(new Point(startX - i, startY));
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

    public override bool Equals(object obj)
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
    public List<Point> Body { get; set; } = new List<Point>();
    public Direction CurrentDirection { get; set; }
    public Direction NextDirection { get; set; }
}