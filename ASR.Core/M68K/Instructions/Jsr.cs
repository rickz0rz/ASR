namespace ASR.Core.M68K.Instructions;

public class Jsr : BaseInstruction
{
    private const int InstMask = 0b1111_1111_1100_0000;
    private const int InstMaskTarget = 0b0100_1110_1000_0000;

    public override bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(ushort opcode, Context context)
    {
        var address = GetEffectiveAddress(opcode, context);

        if (context.LibraryActions.ContainsKey(address))
        {
            // Execute the intercepted action. Since we're executing it in the context
            // of a hijacked call, we don't need to override the PC.
            context.LibraryActions[address](context);
        }
        else
        {
            context.PushLongToStack((uint)context.ProgramCounter);
            context.ProgramCounter = (uint)address;
        }

        return true;
    }
}
