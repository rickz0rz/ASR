namespace ASR.Core.M68K.Instructions;

public class Move : BaseInstruction
{
    private const int InstMask = 0b1100_0000_0000_0000;
    private const int InstMaskTarget = 0b0000_0000_0000_0000;

    private readonly int _sizeBits;

    public Move(ushort opcode, ProcessorContext processorContext) : base(opcode)
    {
        _sizeBits = opcode >> 12 & 0b11;
    }

    public static bool IsInstruction(ushort opcode)
    {
        var sizeModes = new List<byte> { 0b01, 0b11, 0b10 };
        return (opcode & InstMask) == InstMaskTarget
               // && !MoveA.IsMoveAInstruction(opcode) // Has bits 001 in 876
               && sizeModes.Contains((byte)((opcode >> 12) & 0b11));
    }

    public override bool Execute(ProcessorContext processorContext)
    {
        var byteCount = _sizeBits switch
        {
            0b01 => 1,
            0b11 => 2,
            0b10 => 4,
            _ => throw new Exception($"Unhandled size bits: {_sizeBits:b2}")
        };

        var source = GetFromSource(Opcode, processorContext, byteCount);
        var result = PutAtDestination(Opcode, processorContext, source, byteCount);

        processorContext.NFlag = ((result >> (byteCount * 8 - 1)) & 1) == 1;
        processorContext.ZFlag = result == 0;
        processorContext.VFlag = false;
        processorContext.CFlag = false;

        return true;
    }
}
