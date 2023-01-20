namespace Core;

public static class HashCodeHelper
{
    private const int Prime1 = 1647293437;
    public const int Prime2 = 5597243;
    
    public static int Get<T1, T2>(T1 arg1, T2 arg2)
    {
        ulong asd = 23;
        var hashcode = Prime1;
        hashcode = (hashcode * Prime2) + arg1.GetHashCode();
        hashcode = (hashcode * Prime2) + arg2.GetHashCode();
        return hashcode;
        
        // var hashCode = arg1.GetHashCode();
        // var num2 = arg2.GetHashCode() << 2;
        // return hashCode ^ num2;
    }
}