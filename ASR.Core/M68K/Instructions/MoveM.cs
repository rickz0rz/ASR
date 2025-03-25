namespace ASR.Core.M68K.Instructions;

public class MoveM : BaseInstruction
{
    private const int InstMask = 0b1111_1011_1000_0000;
    private const int InstMaskTarget = 0b0100_1000_1000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override void Execute(ushort opcode, Context context)
    {
        // Stub.
        context.ProgramCounter += 2;
    }
}
