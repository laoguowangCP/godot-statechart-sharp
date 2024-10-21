using System.Runtime.InteropServices;

public class IntParser
{
    public int X;

    public IntParser() {}
    public IntParser(int x)
    {
        X = x;
    }

    public static IntParser operator ++(IntParser left)
    {
        left.X += 1;
        return left;
    }
}