namespace ASR.Core.M68K.Instructions;

public class Clr : BaseInstruction
{
    private const int InstMask = 0b1111_1111_0000_0000;
    private const int InstMaskTarget = 0b0100_0010_0000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return ((opcode & InstMask) == InstMaskTarget) && !MoveQ.IsMoveQInstruction(opcode);
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var size = opcode >> 6 & 0b11;
        var effectiveAddress = GetEffectiveAddress(opcode, context);
        switch (size)
        {
            case 0b00: // byte
                break;
            case 0b01: // word
                break;
            case 0b10: // long
                break;
            default:
                throw new NotImplementedException($"CLR size {size:2B} is not implemented.");
        }

        return true;
    }
}
