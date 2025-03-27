using ASR.Core.M68K.Instructions;

namespace ASR.Core.M68K;

public class InstructionCache
{
    private readonly List<BaseInstruction> _instructions;

    public InstructionCache()
    {
        _instructions = [];

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var list = assembly.GetTypes().Where(t => typeof(BaseInstruction).IsAssignableFrom(t));
            foreach (var type in list)
            {
                if (type == typeof(BaseInstruction))
                    continue;

                var instruction = (BaseInstruction)Activator.CreateInstance(type);
                if (instruction != null)
                    _instructions.Add(instruction);
            }
        }
    }

    public BaseInstruction? GetInstruction(Context context, ushort opcode)
    {
        var match = _instructions.FirstOrDefault(i => i.IsInstruction(opcode));

        if (match == null)
            throw new Exception($"No instruction found for {opcode:X4}");

        return match;
    }
}
