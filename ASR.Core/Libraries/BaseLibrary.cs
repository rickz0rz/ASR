namespace ASR.Core.Libraries;

public abstract class BaseLibrary
{
    public Dictionary<long, Action<Context>> Commands { get; set; } = new();
}
