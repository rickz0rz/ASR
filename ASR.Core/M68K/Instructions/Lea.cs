namespace ASR.Core.M68K.Instructions;

public class Lea : BaseInstruction
{
    private const int InstMask = 0b1111_0001_1100_0000;
    private const int InstMaskTarget = 0b0100_0001_1100_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var addressRegister = (opcode >> 9) & 0b111;
        context.A[addressRegister] = (uint)GetEffectiveAddress(opcode, context);
        return true;
    }
}
