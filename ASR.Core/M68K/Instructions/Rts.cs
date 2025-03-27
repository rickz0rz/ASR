namespace ASR.Core.M68K.Instructions;

public class Rts : BaseInstruction
{
    private const int InstMask = 0b1111_1111_1111_1111;
    private const int InstMaskTarget = 0b0100_1110_0111_0101;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(ushort opcode, Context context)
    {
        // If we have an address in the stack, jump to it.
        // otherwise, imma assume the program is terminating.
        return context.Stack.Count != 0;
    }
}
