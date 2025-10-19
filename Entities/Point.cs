namespace op_homework_snake_game.Entities;
public class Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public override bool Equals(object? obj)
    {
        if (!(obj is Point point)) 
            return false;
        return X == point.X && Y == point.Y;
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