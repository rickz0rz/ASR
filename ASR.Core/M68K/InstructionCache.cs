using ASR.Core.M68K.Instructions;

namespace ASR.Core.M68K;

public class InstructionCache
{
    private readonly List<Type> _instructionTypes;

    public InstructionCache()
    {
        _instructionTypes = [];

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var list = assembly.GetTypes().Where(t => typeof(BaseInstruction).IsAssignableFrom(t));
            foreach (var type in list)
            {
                if (type == typeof(BaseInstruction))
                    continue;

                _instructionTypes.Add(type);
            }
        }
    }

    public Type GetInstructionType(CPUContext cpuContext, ushort opcode)
    {
        var match = _instructionTypes.FirstOrDefault(i =>
        {
            var methodInfo = i.GetMethod("IsInstruction");
            return (bool)methodInfo.Invoke(null, [opcode]);
        });

        return match;
    }
}
