using ASR.Core.M68K;
using ASR.Core.M68K.Instructions;

namespace ASR.Core;

public class Emulator
{
    protected readonly ProcessorContext ProcessorContext;
    protected readonly InstructionCache InstructionCache;
    private readonly Dictionary<uint, BaseInstruction> _instructionMap;

    protected Emulator(ProcessorContext processorContext)
    {
        ProcessorContext = processorContext;
        InstructionCache = new InstructionCache();
        _instructionMap = new Dictionary<uint, BaseInstruction>();
    }

    /// <summary>
    /// Begin execution.
    /// </summary>
    /// <param name="address">The initial address to start execution at.</param>
    /// <param name="executionHook">A function that is called on each instruction's execution.
    /// If true, skip the rest of the instruction being processed.</param>
    protected void Execute(uint address, Func<ProcessorContext, BaseInstruction, bool> executionHook)
    {
        ProcessorContext.ProgramCounter = address;

        while (true)
        {
            ProcessorContext.PopulatePrefetch();

            try
            {
                var effectiveProgramCounter = ProcessorContext.ProgramCounter - 4;

                if (EmulatorConfiguration.DebugPrint)
                    Console.Write($"PC: 0x{effectiveProgramCounter:X6}");

                if (!_instructionMap.TryGetValue(effectiveProgramCounter, out var instruction))
                {
                    instruction = BaseInstruction.GetInstruction(ProcessorContext);
                    _instructionMap.Add(effectiveProgramCounter, instruction);
                }

                if (EmulatorConfiguration.DebugPrint)
                    Console.Write($" -> {instruction.Opcode:X4} ({instruction.GetType()}) ");

                if (EmulatorConfiguration.DebugPrint)
                    Console.WriteLine();

                if (executionHook(ProcessorContext, instruction))
                    continue;

                if (!instruction.Execute(ProcessorContext))
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex);
                Console.WriteLine($"PC: 0x{ProcessorContext.ProgramCounter:X6}");
                return;
            }
        }
    }
}
