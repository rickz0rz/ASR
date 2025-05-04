using System.Text.Json;

namespace ASR.Core.Tests;

public class ContextScenario(string name, CPUContext cpuContext, JsonElement jsonElement)
{
    public JsonElement JsonElement { get; } = jsonElement;
    private string Name { get; } = name;
    public CPUContext CpuContext { get; } = cpuContext;

    public override string ToString()
    {
        return Name;
    }
}
