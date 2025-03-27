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

    public override bool Execute(ushort opcode, Context context)
    {
        var sizeBits = opcode >> 12 & 0b11;
        var byteCount = sizeBits switch
        {
            0b01 => 1,
            0b11 => 2,
            0b10 => 4,
            _ => throw new Exception($"Unhandled size bits: {sizeBits:b2}")
        };

        PutDestinationAddress(opcode, context, [(uint)GetSourceAddress(opcode, context, byteCount)]);

        return true;
    }
}
