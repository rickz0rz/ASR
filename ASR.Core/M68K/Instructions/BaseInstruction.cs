namespace ASR.Core.M68K.Instructions;

public class BaseInstruction
{
    public virtual bool IsInstruction(ushort opcode)
    {
        throw new NotImplementedException();
    }

    public virtual bool Execute(ushort opcode, Context context)
    {
        throw new NotImplementedException();
    }

    protected int GetEffectiveAddress(ushort opcode, Context context, int byteCount = 0)
    {
        var addressMode = (opcode >> 3) & 0b111;
        var register = opcode & 0b111;

        switch (addressMode)
        {
            case 0b000:
                return (int)context.D[register];
            case 0b010:
                return (int)context.A[register];
            case 0b101:
                return (int)context.A[register] + ReadTwosComplimentWord(context);
            case 0b111 when register == 0b001:
                var resultM111R001 = 0;
                for (var i = 0; i < 4; i++)
                {
                    resultM111R001 = (resultM111R001 << 8) + context.Memory[context.ProgramCounter++];
                }
                return resultM111R001;
            case 0b111 when register == 0b010:
                // word plus pc
                uint resultM111R010 = context.Memory[context.ProgramCounter++];
                resultM111R010 = (resultM111R010 << 8) + context.Memory[context.ProgramCounter++];
                resultM111R010 -= 2; // i think we have to factor in the PC before we increment here.
                return (int)(context.ProgramCounter + resultM111R010);
            case 0b111 when register == 0b100:
                uint resultM111R100 = 0;
                for (var i = 0; i < byteCount; i++)
                {
                    resultM111R100 = (resultM111R100 << 8) + context.Memory[context.ProgramCounter];
                    context.ProgramCounter++;
                }
                return (int)resultM111R100;
            default:
                throw new NotImplementedException($"Address mode {addressMode:b3} and register {register:b3} is not implemented.");
        }
    }

    protected static void PutDestinationAddress(ushort opcode, Context context, uint[] value)
    {
        var addressMode = (opcode >> 6) & 0b111;
        var register = (opcode >> 9) & 0b111;
        switch (addressMode)
        {
            case 0b000:
                context.D[register] = value[0];
                break;
            //case 0b011 when register == 0b011:
            //    break;
            default:
                throw new NotImplementedException($"Address mode {addressMode:b3} and register {register:b3} is not implemented.");
        }
    }

    protected int GetSourceAddress(ushort opcode, Context context, int byteCount = 0)
    {
        return GetEffectiveAddress(opcode, context, byteCount);
    }

    protected int ConvertWordToTwosCompliment(int value)
    {
        return ((value >> 15) & 1) == 1 ? -1 - (value ^ 0xFFFF) : value;
    }

    protected long ConvertLongToTwosCompliment(int value)
    {
        return ((value >> 31) & 1) == 1 ? -1 - (value ^ 0xFFFFFFFF) : value;
    }

    protected int ReadTwosComplimentWord(Context context)
    {
        return ConvertWordToTwosCompliment(ReadWord(context));
    }

    protected int ReadWord(Context context)
    {
        var b = context.Memory[context.ProgramCounter++];
        return ((b << 8) | context.Memory[context.ProgramCounter++]);
    }
}
