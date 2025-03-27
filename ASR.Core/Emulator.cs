using System.Globalization;
using ASR.Core.M68K;

namespace ASR.Core;

public class Emulator
{
    protected readonly Context Context;
    protected readonly InstructionCache InstructionCache;

    protected Emulator()
    {
        Context = new Context();
        InstructionCache = new InstructionCache();
    }

    protected Emulator(Context context)
    {
        Context = context;
        InstructionCache = new InstructionCache();
    }

    private ushort ReadOpcode()
    {
        var byte1 = Context.Memory[Context.ProgramCounter];
        Context.ProgramCounter++;
        var byte2 = Context.Memory[Context.ProgramCounter];
        Context.ProgramCounter++;
        return (ushort)(byte1 << 8 | byte2);
    }

    public void Execute()
    {
        while (true)
        {
            try
            {
                // Console.Write($"{Context.ProgramCounter:X6}");
                var opcode = ReadOpcode();
                // Console.Write($" -> {opcode:X4}");
                var instruction = InstructionCache.GetInstruction(Context, opcode);
                if (instruction == null)
                {
                    Console.WriteLine();
                }
                else
                {
                    // Console.WriteLine($" ({instruction.GetType()})");
                    if (!instruction.Execute(opcode, Context))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex);
                Console.WriteLine($"PC: 0x{Context.ProgramCounter:X6}");
                return;
            }
        }
    }
}
