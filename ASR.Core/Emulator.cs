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
            var opcode = ReadOpcode();
            var instruction = InstructionCache.GetInstruction(Context, opcode);

            if (instruction != null)
            {
                instruction.Execute(opcode, Context);
                continue;
            }

            // parse instruction -- steal the work done here on the disassembler
            // execute instruction as opposed to "generate c# code equivalent"
            // jsrs and such will just adjust the PC
            switch (opcode)
            {
                case 0x203C:
                    // MOVE.L #$00001729,D0 ;0x00001A: 203C00001729
                    Context.ProgramCounter += 4;
                    break;
                case 0x2400:
                    // MOVE.L D0,D2 ;0x000006: 2400
                    Context.D[2] = Context.D[0];
                    break;
                case 0x2448:
                    // MOVEA.L A0,A2 ;0x000004: 2448
                    Context.A[2] = Context.A[0];
                    break;
                case 0x2C78:
                    // MOVEA.L $0004,A6 ;0x00000E: 2C780004
                    Context.ProgramCounter += 2;
                    break;
                case 0x47F9:
                    // LEA $00007D68,A3 ;0x000012: 47F900007D68
                    Context.ProgramCounter += 4;
                    break;
                case 0x48E7:
                    // MOVEM.L D1-D6/A0-A6,-(A7) ;0x000000: 48E77EFE
                    Context.ProgramCounter += 2;
                    break;
                case 0x49F9:
                    // LEA $00008000,A4 ;0x000008: 49F900008000
                    Context.ProgramCounter += 4;
                    break;
                case 0x6002:
                    // BRA.S 2 ;0x000020: 6002
                    break;
                case 0x7200:
                    // MOVEQ #0,D1 ;0x000018: 7200
                    Context.D[1] = 0;
                    break;
                default:
                    throw new Exception($"Unknown opcode 0x{opcode:X4}");
            }
        }
    }
}
