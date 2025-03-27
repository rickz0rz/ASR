namespace ASR.Core.M68K.Instructions;

public class Dbcc : BaseInstruction
{
    private const int InstMask = 0b1111_0000_1111_1000;
    private const int InstMaskTarget = 0b0101_0000_1100_1000;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(ushort opcode, Context context)
    {
        // stub.
        context.ProgramCounter += 2;
        return true;
    }
}
