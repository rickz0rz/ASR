using ASR.Core.M68K;

namespace ASR.Core.Amiga.Libraries;

public abstract class BaseLibrary
{
    public Dictionary<long, Action<ProcessorContext>> Commands { get; set; } = new();
}
