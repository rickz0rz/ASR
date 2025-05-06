using System.Text.Json;
using ASR.Core.M68K;

namespace ASR.Core.Tests.M68K;

public class ContextScenario(string name, ProcessorContext processorContext, JsonElement jsonElement)
{
    public JsonElement JsonElement { get; } = jsonElement;
    private string Name { get; } = name;
    public ProcessorContext ProcessorContext { get; } = processorContext;

    public override string ToString()
    {
        return Name;
    }
}
