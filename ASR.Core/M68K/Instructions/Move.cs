namespace ASR.Core.M68K.Instructions;

public class Move : BaseInstruction
{
    private const int InstMask = 0b1100_0000_0000_0000;
    private const int InstMaskTarget = 0b0000_0000_0000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        var sizeModes = new List<byte> { 0b01, 0b11, 0b10 };
        return (opcode & InstMask) == InstMaskTarget
               && !MoveA.IsMoveAInstruction(opcode) // Has bits 001 in 876
               && sizeModes.Contains((byte)((opcode >> 12) & 0b11));
    }

    public override void Execute(ushort opcode, Context context)
    {
        // Stub
    }
}
