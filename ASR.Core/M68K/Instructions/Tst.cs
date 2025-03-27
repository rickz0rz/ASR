namespace ASR.Core.M68K.Instructions;

public class Tst: BaseInstruction
{
    private const int InstMask = 0b1111_1111_0000_0000;
    private const int InstMaskTarget = 0b0100_1010_0000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var byteCount = (opcode >> 6 & 0b11) + 1;
        var val = GetEffectiveAddress(opcode, context);

        context.NFlag = val < 0;
        context.ZFlag = val == 0;
        context.VFlag = false;
        context.CFlag = false;

        return true;
    }
}
