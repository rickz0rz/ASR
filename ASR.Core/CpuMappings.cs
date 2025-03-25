namespace ASR.Core;

public static class CpuMappings
{
    /// <summary>
    /// Compares the value with zero.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="val"></param>
    public static void Tst(Context context, uint val)
    {
        context.NFlag = val < 0;
        context.ZFlag = val == 0;
    }
}
