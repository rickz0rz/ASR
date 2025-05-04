namespace ASR.Core.Libraries;

public abstract class BaseLibrary
{
    public Dictionary<long, Action<CPUContext>> Commands { get; set; } = new();
}
