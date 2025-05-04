namespace ASR.Core.M68K.Instructions;

public class BaseInstruction
{
    public ushort Opcode { get; }

    protected BaseInstruction(ushort opcode)
    {
        Opcode = opcode;
    }

    private static readonly InstructionCache InstructionCache = new();

    public static BaseInstruction GetInstruction(CPUContext cpuContext)
    {
        var opcode = ReadOpcode(cpuContext);
        var instructionType = InstructionCache.GetInstructionType(cpuContext, opcode);

        if (instructionType == null)
        {
            throw new NotImplementedException($"No instruction type found for {opcode:X4} " +
                                              $"@ 0x{(cpuContext.ProgramCounter - 2):X6}");
        }

        return (BaseInstruction) Activator.CreateInstance(instructionType, [opcode, cpuContext]);
    }

    public static ushort ReadOpcode(CPUContext cpuContext)
    {
        return cpuContext.GetPrefetchWord();
    }

    public virtual bool Execute(CPUContext cpuContext)
    {
        throw new NotImplementedException();
    }

    // Conversion from uint to int is just a cast.
    // Conversion from int to uint requires math.
    protected uint GetEffective(ushort opcode, CPUContext cpuContext, int byteCount = 0)
    {
        var addressMode = (opcode >> 3) & 0b111;
        var register = (uint)opcode & 0b111;

        // https://www.thedigitalcatonline.com/blog/2019/03/04/motorola-68000-addressing-modes/
        // http://alanclements.org/68kaddressingmodes3.html
        switch (addressMode)
        {
            case 0b000: // Dn
                return cpuContext.D[register];
            case 0b001: // An (with size)
                return cpuContext.Memory[cpuContext.A[register]];
            case 0b010: // (An)
                // Todo: read full size of memory
                return cpuContext.Memory[cpuContext.A[register] & 0x00FFFFFF];
            case 0b011: // (An)+
                uint m001R = 0;
                for (var i = 0; i < byteCount; i++)
                {
                    var m001Ba = cpuContext.A[register] & 0x00FFFFFF;
                    cpuContext.A[register] += 1;
                    m001R = (m001R << 8) | cpuContext.Memory[m001Ba];
                }
                return m001R;
            case 0b100: // -(An)
                cpuContext.A[register] -= 1;
                return cpuContext.Memory[cpuContext.A[register] & 0xFFFFFF];
            case 0b101: // (d16,An)
                return cpuContext.Memory[(uint)((cpuContext.A[register] & 0x00FFFFFF) + ConvertWordToShort(cpuContext.GetPrefetchWord()))];
            case 0b110: // (d8,An,Xn)
                var b100AnRegister = cpuContext.A[register];
                var b100XRegisterValue = GetB100ExtensionValue(cpuContext);
                var b100Displacement = ReadSbyteFromPrefetch(cpuContext);
                var b100Address = (uint)(b100AnRegister + b100XRegisterValue + b100Displacement);
                switch (byteCount)
                {
                    case 1:
                        return ReadByte(cpuContext, b100Address & 0xFFFFFF);
                    case 4:
                        return ReadLongWord(cpuContext, b100Address & 0xFFFFFF);
                    default:
                        throw new NotImplementedException();
                }
            case 0b111 when register == 0b000: // (xxx).W
                throw new NotImplementedException();
            case 0b111 when register == 0b001: // (xxx).L
                return cpuContext.Memory[cpuContext.GetPrefetchLongWord() & 0xFFFFFF];
            case 0b111 when register == 0b100 && byteCount == 1: // #<data>
                return cpuContext.GetPrefetchByte();
            case 0b111 when register == 0b100 && byteCount == 2: // #<data>
                return cpuContext.GetPrefetchWord();
            case 0b111 when register == 0b100 && byteCount == 4: // #<data>
                return cpuContext.GetPrefetchLongWord();
            case 0b111 when register == 0b010: // (d16,PC)
                // This may have to take into effect that the prefetch is loading data...?
                return (uint)(cpuContext.ProgramCounter + ConvertWordToShort(cpuContext.GetPrefetchWord()));
            case 0b111 when register == 0b011: // (d8,PC,Xn)
                throw new  NotImplementedException();
            default:
                throw new NotImplementedException($"Address mode {addressMode:b3} and register {register:b3} is not implemented.");
        }
    }

    protected uint GetFromSource(ushort opcode, CPUContext cpuContext, int byteCount = 0)
    {
        return GetEffective(opcode, cpuContext, byteCount);
    }

    protected uint PutAtDestination(ushort opcode, CPUContext cpuContext, uint value, int byteCount)
    {
        var addressMode = (opcode >> 6) & 0b111;
        var register = (uint)(opcode >> 9) & 0b111;
        switch (addressMode)
        {
            case 0b000: // Dn
                switch (byteCount)
                {
                    case 1:
                        var result1 = (cpuContext.D[register] & 0xFFFFFF00) | (value & 0x000000FF);
                        cpuContext.D[register] = result1;
                        return cpuContext.D[register];
                    case 2:
                        var result2 = (cpuContext.D[register] & 0xFFFF0000) | (value & 0x0000FFFF);
                        cpuContext.D[register] = result2;
                        return cpuContext.D[register];
                    case 4:
                        cpuContext.D[register] = value;
                        return cpuContext.D[register];
                    default:
                        throw new NotImplementedException();
                }
            case 0b010: // (An)
                switch (byteCount)
                {
                    case 1:
                        WriteByte(cpuContext, cpuContext.A[register] & 0xFFFFFF, value);
                        return value;
                    case 4:
                        WriteLongWord(cpuContext, cpuContext.A[register] & 0xFFFFFF, value);
                        return value;
                    default:
                        throw new NotImplementedException();
                }
            case 0b011: // (An)+
                switch (byteCount)
                {
                    case 1:
                        WriteByte(cpuContext, cpuContext.A[register] & 0xFFFFFF, value);
                        cpuContext.A[register] += 1;
                        return value;
                    default:
                        throw new NotImplementedException();
                }
            case 0b100: // -(An)
                cpuContext.A[register] -= 1;
                WriteByte(cpuContext, cpuContext.A[register] & 0xFFFFFF, value);
                return value;
            case 0b101: // (d16,An)
                var address = (uint)(cpuContext.A[register] + ReadShortFromPrefetch(cpuContext));
                WriteLongWord(cpuContext, address, value);
                return value;
            case 0b110: // (d8,An,Xn)
                var b100AnRegister = cpuContext.A[register];
                var b100XRegisterValue = GetB100ExtensionValue(cpuContext);
                var b100Displacement = ReadSbyteFromPrefetch(cpuContext);
                var b100Address = (uint)(b100AnRegister + b100XRegisterValue + b100Displacement);
                switch (byteCount)
                {
                    case 1:
                        WriteByte(cpuContext, b100Address & 0xFFFFFF, value);
                        break;
                    case 4:
                        WriteLongWord(cpuContext, b100Address & 0xFFFFFF, value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return value;
            case 0b111 when register == 0b000: // (xxx).W
                throw new NotImplementedException();
            case 0b111 when register == 0b001: // (xxx).L
                var b111r001Addr = cpuContext.GetPrefetchLongWord();
                switch (byteCount)
                {
                    case 1:
                        var b111r001b = ReadByte(cpuContext, b111r001Addr);
                        cpuContext.A[b111r001Addr] = (cpuContext.A[b111r001Addr] & 0xFFFFFF00) | b111r001b;
                        break;
                    case 4:
                        cpuContext.A[register] = ReadLongWord(cpuContext, b111r001Addr);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return cpuContext.A[register];
            default:
                throw new NotImplementedException($"Address mode {addressMode:b3} and register {register:b3} is not implemented.");
        }
    }

    private int GetB100ExtensionValue(CPUContext cpuContext)
    {
        // https://www.cs.emory.edu/~cheung/Courses/255/Syllabus/Old-7-M68000/Docs/68K-manual.pdf
        // https://github.com/captain-amygdala/pistorm/blame/main/m68kcpu.h#L1558-L1560C52
        /*
        * Brief extension format:
        *  F  |  E D C   |  B  |  A 9  | 8 | 7 6 5 4 3 2 1 0
        * D/A | REGISTER | W/L | SCALE | 0 |  DISPLACEMENT
        */
        var flags = cpuContext.GetPrefetchByte();
        var indexRegisterNumber = (uint)(flags >> 4) & 0b111;
        var indexRegisterType = flags >> 7;
        var responseSize = (flags >> 3) & 1;
        var response = indexRegisterType == 1
            ? cpuContext.A[indexRegisterNumber]
            : cpuContext.D[indexRegisterNumber];
        if (responseSize == 0)
        {
            // word
            return (short)(ushort)(response & 0xFFFF);
        }

        return (int)response;
    }

    protected int ConvertLongWordToInt(uint value)
    {
        return (int)(((value >> 31) & 1) == 1 ? -1 - (value ^ 0xFFFFFFFF) : value);
    }

    protected int ReadIntFromPrefetch(CPUContext cpuContext)
    {
        return ConvertLongWordToInt(cpuContext.GetPrefetchLongWord());
    }

    protected short ConvertWordToShort(ushort value)
    {
        return (short)(((value >> 15) & 1) == 1 ? -1 - (value ^ 0xFFFF) : value);
    }

    protected short ReadShortFromPrefetch(CPUContext cpuContext)
    {
        return ConvertWordToShort(cpuContext.GetPrefetchWord());
    }

    protected sbyte ConvertByteToSbyte(byte value)
    {
        return (sbyte)(((value >> 7) & 1) == 1 ? -1 - (value ^ 0xFF) : value);
    }

    protected sbyte ReadSbyteFromPrefetch(CPUContext cpuContext)
    {
        return ConvertByteToSbyte(cpuContext.GetPrefetchByte());
    }

    protected void WriteByte(CPUContext cpuContext, uint address, uint value)
    {
        cpuContext.Memory[address] = (byte)(value & 0xFF);
    }

    protected uint ReadByte(CPUContext cpuContext, uint address)
    {
        return cpuContext.Memory[address];
    }

    protected uint ReadLongWord(CPUContext cpuContext, uint address)
    {
        uint result =
            (uint)((cpuContext.Memory[address] << 24) +
            (cpuContext.Memory[address + 1] << 16) +
            (cpuContext.Memory[address + 2] << 8) +
            cpuContext.Memory[address + 3]);
        return result;
    }

    protected void WriteLongWord(CPUContext cpuContext, uint address, uint value)
    {
        cpuContext.Memory[address] = (byte)(value >> 24 & 0xFF);
        cpuContext.Memory[address + 1] = (byte)(value >> 16 & 0xFF);
        cpuContext.Memory[address + 2] = (byte)(value >> 8 & 0xFF);
        cpuContext.Memory[address + 3] = (byte)(value & 0xFF);
    }
}
