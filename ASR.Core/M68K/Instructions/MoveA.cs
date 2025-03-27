namespace ASR.Core.M68K.Instructions;

public class MoveA : BaseInstruction
{
    private const int InstMask = 0b1100_0001_1100_0000;
    private const int InstMaskTarget = 0b0000_0000_0100_0000;

    public static bool IsMoveAInstruction(ushort opcode)
    {
        var sizeModes = new List<byte> { 0b11, 0b10 };
        return (opcode & InstMask) == InstMaskTarget &&
               sizeModes.Contains((byte)((opcode >> 12) & 0b11));
    }

    public override bool IsInstruction(ushort opcode)
    {
        return IsMoveAInstruction(opcode);
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var size = opcode >> 12 & 0b11;
        switch (size)
        {
            case 0b10:
                // long;
                break;
            case 0b11:
                // word;
                break;
            default:
                throw new NotImplementedException($"Size: {size}");
        }

        var destinationRegister = (opcode >> 9) & 0b111;
        var register = opcode & 0b111;
        var mode = (opcode >> 3) & 0b111;

        switch (mode)
        {
            case 0b000:
                context.A[destinationRegister] = context.D[register];
                break;
            case 0b001:
                context.A[destinationRegister] = context.A[register];
                break;
            case 0b111 when register == 0b000:
                context.A[destinationRegister] = (uint)ReadWord(context);
                break;
            default:
                throw new NotImplementedException($"Mode: {mode:b3} with register {register:b3} @ 0x{context.ProgramCounter:X8}");
        }

        return true;
    }
}
