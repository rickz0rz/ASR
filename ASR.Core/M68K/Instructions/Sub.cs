namespace ASR.Core.M68K.Instructions;

public class Sub : BaseInstruction
{
    private const int InstMask = 0b1111_0000_0000_0000;
    private const int InstMaskTarget = 0b1001_0000_0000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var opmode = opcode >> 6 & 0b111;
        var register = opcode >> 9 & 0b111;

        var ea = GetEffectiveAddress(opcode, context);
        var result = (opmode & 0b100) == 0b100
            ? ea - context.D[register]
            : context.D[register] - ea;

        // Store the result.

        return true;
    }
}
