namespace ASR.Core.M68K.Instructions;

public class Move : BaseInstruction
{
    private const int InstMask = 0b1100_0000_0000_0000;
    private const int InstMaskTarget = 0b0000_0000_0000_0000;

    private readonly int _sizeBits;

    public Move(ushort opcode, CPUContext cpuContext) : base(opcode)
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

    public override bool Execute(CPUContext cpuContext)
    {
        var byteCount = _sizeBits switch
        {
            0b01 => 1,
            0b11 => 2,
            0b10 => 4,
            _ => throw new Exception($"Unhandled size bits: {_sizeBits:b2}")
        };

        var source = GetFromSource(Opcode, cpuContext, byteCount);
        var result = PutAtDestination(Opcode, cpuContext, source, byteCount);

        var bitOffset = byteCount * 8 - 1;

        // why are these backwards? the value must be wrong..
        cpuContext.NFlag = ((result >> bitOffset) & 1) == 1;
        cpuContext.ZFlag = result == 0;
        cpuContext.VFlag = false;
        cpuContext.CFlag = false;

        return true;
    }
}
