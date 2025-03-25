namespace ASR.Core.M68K.Instructions;

public class BaseInstruction
{
    public virtual bool IsInstruction(ushort opcode)
    {
        throw new NotImplementedException();
    }

    public virtual void Execute(ushort opcode, Context context)
    {
        throw new NotImplementedException();
    }

    protected static uint GetEffectiveAddress(ushort opcode, Context context, int bitOffset = 0)
    {
        var addressMode = (opcode >> (3 + bitOffset)) & 0b111;
        var register = (opcode + bitOffset) & 0b111;
        switch (addressMode)
        {
            case 0b010:
                return context.A[register];
            case 0b111 when register == 0b001:
                var result = 0;
                for (var i = 0; i < 4; i++)
                {
                    result = (result << 8) + context.Memory[context.ProgramCounter];
                    context.ProgramCounter++;
                }
                return (uint)result;
            default:
                throw new NotImplementedException($"Address mode {addressMode:b3} and register {register:b3} is not implemented.");
        }
    }

    protected static uint GetSourceAddress(ushort opcode, Context context)
    {
        return GetEffectiveAddress(opcode, context);
    }
}
