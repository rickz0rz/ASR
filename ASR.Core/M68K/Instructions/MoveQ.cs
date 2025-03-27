namespace ASR.Core.M68K.Instructions;

public class MoveQ : BaseInstruction
{
    private const int InstMask = 0b1111_0001_0000_0000;
    private const int InstMaskTarget = 0b0111_0000_0000_0000;

    public static bool IsMoveQInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool IsInstruction(ushort opcode)
    {
        return IsMoveQInstruction(opcode);
    }

    public override bool Execute(ushort opcode, Context context)
    {
        context.D[(opcode >> 9) & 0b111] = (uint)(opcode & 0xFF);
        // N and Z set?
        context.VFlag = false;
        context.CFlag = false;
        return true;
    }
}
