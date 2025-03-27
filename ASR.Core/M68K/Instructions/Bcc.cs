namespace ASR.Core.M68K.Instructions;

public class Bcc : BaseInstruction
{
    private const int InstMask = 0b1111_0000_0000_0000;
    private const int InstMaskTarget = 0b0110_0000_0000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return ((opcode & InstMask) == InstMaskTarget) && !MoveQ.IsMoveQInstruction(opcode);
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var condition = (opcode >> 8) & 0b1111;
        var displacement = opcode & 0xFF;

        if (displacement == 0)
        {
            displacement = ReadWord(context);
        }

        displacement = ConvertWordToTwosCompliment(displacement);

        switch (condition)
        {
            case 0b0000: // BRA
                context.ProgramCounter += (uint)displacement;
                break;
            case 0b0111: // BEQ
                if (context.ZFlag)
                {
                    context.ProgramCounter += (uint)displacement;
                }
                break;
            default:
                throw new Exception($"Unhandled Bcc case: {condition:B4}");
        }

        return true;
    }
}
