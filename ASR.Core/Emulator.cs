using ASR.Core.M68K;
using ASR.Core.M68K.Instructions;

namespace ASR.Core;

public class Emulator
{
    protected readonly CPUContext CpuContext;
    protected readonly InstructionCache InstructionCache;
    private readonly Dictionary<uint, BaseInstruction> _instructionMap;

    protected Emulator(CPUContext cpuContext)
    {
        CpuContext = cpuContext;
        InstructionCache = new InstructionCache();
        _instructionMap = new Dictionary<uint, BaseInstruction>();
    }

    /// <summary>
    /// Begin execution.
    /// </summary>
    /// <param name="address">The initial address to start execution at.</param>
    /// <param name="executionHook">A function that is called on each instruction's execution.
    /// If true, skip the rest of the instruction being processed.</param>
    protected void Execute(uint address, Func<CPUContext, BaseInstruction, bool> executionHook)
    {
        CpuContext.ProgramCounter = address;

        while (true)
        {
            CpuContext.PopulatePrefetch();

            try
            {
                var effectiveProgramCounter = CpuContext.ProgramCounter - 4;

                if (EmulatorConfiguration.DebugPrint)
                    Console.Write($"PC: 0x{effectiveProgramCounter:X6}");

                if (!_instructionMap.TryGetValue(effectiveProgramCounter, out var instruction))
                {
                    instruction = BaseInstruction.GetInstruction(CpuContext);
                    _instructionMap.Add(effectiveProgramCounter, instruction);
                }

                if (EmulatorConfiguration.DebugPrint)
                    Console.Write($" -> {instruction.Opcode:X4} ({instruction.GetType()}) ");

                if (EmulatorConfiguration.DebugPrint)
                    Console.WriteLine();

                if (executionHook(CpuContext, instruction))
                    continue;

                if (!instruction.Execute(CpuContext))
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex);
                Console.WriteLine($"PC: 0x{CpuContext.ProgramCounter:X6}");
                return;
            }
        }
    }
}
