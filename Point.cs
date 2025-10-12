namespace SnakeGame;

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
        if (obj is null || !(obj is Point)) 
            return false;
        return X == ((Point)obj).X && Y == ((Point)obj).Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Point? a, Point? b)
    {
        if (a is null)
            return b is null;
        if (b is null)
            return false;
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Point? a, Point? b)
    {
        return !(a == b);
    }
}