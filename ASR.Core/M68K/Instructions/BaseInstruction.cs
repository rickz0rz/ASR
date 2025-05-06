namespace ASR.Core.M68K.Instructions;

public class BaseInstruction
{
    public ushort Opcode { get; }

    protected BaseInstruction(ushort opcode)
    {
        Opcode = opcode;
    }

    private static readonly InstructionCache InstructionCache = new();

    public static BaseInstruction GetInstruction(ProcessorContext processorContext)
    {
        var opcode = ReadOpcode(processorContext);
        var instructionType = InstructionCache.GetInstructionType(processorContext, opcode);

        if (instructionType == null)
        {
            throw new NotImplementedException($"No instruction type found for {opcode:X4} " +
                                              $"@ 0x{(processorContext.ProgramCounter - 2):X6}");
        }

        return (BaseInstruction) Activator.CreateInstance(instructionType, [opcode, processorContext]);
    }

    public static ushort ReadOpcode(ProcessorContext processorContext)
    {
        return processorContext.GetPrefetchWord();
    }

    public virtual bool Execute(ProcessorContext processorContext)
    {
        throw new NotImplementedException();
    }

    protected uint GetEffective(ushort opcode, ProcessorContext processorContext, int byteCount = 0)
    {
        var addressMode = (opcode >> 3) & 0b111;
        var register = (uint)opcode & 0b111;

        // https://www.thedigitalcatonline.com/blog/2019/03/04/motorola-68000-addressing-modes/
        // http://alanclements.org/68kaddressingmodes3.html
        switch (addressMode)
        {
            case 0b000: // Dn
                return processorContext.D[register];
            case 0b001: // An
                return processorContext.A[register];
            case 0b010 when byteCount == 1: // (An)
                return ReadByte(processorContext, processorContext.A[register] & 0x00FFFFFF);
            case 0b011 when byteCount == 1: // (An)+
                var m001Ba = processorContext.A[register];
                processorContext.A[register] += (uint)(register == 7 ? 2 : 1);
                return ReadByte(processorContext, m001Ba & 0x00FFFFFF);
            case 0b100 when byteCount == 1: // -(An)
                processorContext.A[register] -= (uint)(register == 7 ? 2 : 1);
                return ReadByte(processorContext, processorContext.A[register] & 0xFFFFFF);
            case 0b100 when byteCount == 2: // -(An)
                processorContext.A[register] -= (uint)(register == 7 ? 2 : 1);
                return ReadWord(processorContext, processorContext.A[register] & 0xFFFFFF);
            case 0b101 when byteCount == 1: // (d16,An)
                return ReadByte(processorContext, (uint)((processorContext.A[register] & 0x00FFFFFF) + ConvertWordToShort(processorContext.GetPrefetchWord())));
            case 0b110: // (d8,An,Xn)
                var b100AnRegister = processorContext.A[register];
                var b100XRegisterValue = GetB100ExtensionValue(processorContext);
                var b100Displacement = ReadSbyteFromPrefetch(processorContext);
                var b100Address = (uint)(b100AnRegister + b100XRegisterValue + b100Displacement);
                return byteCount switch
                {
                    1 => ReadByte(processorContext, b100Address & 0xFFFFFF),
                    2 => ReadWord(processorContext, b100Address & 0xFFFFFF),
                    4 => ReadLongWord(processorContext, b100Address & 0xFFFFFF),
                    _ => throw new NotImplementedException()
                };
            case 0b111 when register == 0b000: // (xxx).W
                var b111A = processorContext.GetPrefetchWord();
                return b111A >> 15 == 1
                    ? processorContext.Memory[(uint)(b111A | 0xFF0000) & 0xFFFFFF]
                    : processorContext.Memory[b111A];
            case 0b111 when register == 0b001: // (xxx).L
                return processorContext.Memory[processorContext.GetPrefetchLongWord() & 0xFFFFFF];
            case 0b111 when register == 0b100 && byteCount == 1: // #<data>
                return (uint)(processorContext.GetPrefetchWord() & 0xFF);
            case 0b111 when register == 0b100 && byteCount == 2: // #<data>
                return processorContext.GetPrefetchWord();
            case 0b111 when register == 0b100 && byteCount == 4: // #<data>
                return processorContext.GetPrefetchLongWord();
            case 0b111 when register == 0b010 && byteCount == 1: // (d16,PC)
                var pcB0111R010 = (processorContext.ProgramCounter - 4);
                return processorContext.Memory[(uint)(pcB0111R010 + ConvertWordToShort(processorContext.GetPrefetchWord()))];
            case 0b111 when register == 0b011 && byteCount == 1: // (d8,PC,Xn)
                var b111R011BC1 = (uint)((processorContext.ProgramCounter - 4)
                                         + GetB100ExtensionValue(processorContext)
                                         + ReadSbyteFromPrefetch(processorContext));
                return ReadByte(processorContext, b111R011BC1 & 0xFFFFFF);
            case 0b111 when register == 0b011 && byteCount == 2: // (d8,PC,Xn)
                var b111R011BC2 = (uint)((processorContext.ProgramCounter - 4)
                                         + GetB100ExtensionValue(processorContext)
                                         + ReadSbyteFromPrefetch(processorContext));
                return ReadWord(processorContext, b111R011BC2 & 0xFFFFFF);
            case 0b111 when register == 0b011 && byteCount == 4: // (d8,PC,Xn)
                var b111R011BC4 = (uint)(processorContext.ProgramCounter - 4 // why?
                                         + GetB100ExtensionValue(processorContext)
                                         + ReadSbyteFromPrefetch(processorContext));
                return ReadLongWord(processorContext, b111R011BC4 & 0xFFFFFF);
            default:
                throw new NotImplementedException($"Read: Byte count {byteCount}, address mode {addressMode:b3}, and register {register:b3} is not implemented.");
        }
    }

    protected uint GetFromSource(ushort opcode, ProcessorContext processorContext, int byteCount = 0)
    {
        return GetEffective(opcode, processorContext, byteCount);
    }

    protected uint PutAtDestination(ushort opcode, ProcessorContext processorContext, uint value, int byteCount)
    {
        var addressMode = (opcode >> 6) & 0b111;
        var register = (uint)(opcode >> 9) & 0b111;

        switch (addressMode)
        {
            case 0b000 when byteCount == 1: // Dn
                processorContext.D[register] = (processorContext.D[register] & 0xFFFFFF00) | (value & 0x000000FF);
                return processorContext.D[register];
            case 0b000 when byteCount == 2: // Dn
                processorContext.D[register] = (processorContext.D[register] & 0xFFFF0000) | (value & 0x0000FFFF);
                return processorContext.D[register];
            case 0b000 when byteCount == 4: // Dn
                processorContext.D[register] = value;
                return processorContext.D[register];
            case 0b010 when byteCount == 1: // (An)
                WriteByte(processorContext, processorContext.A[register] & 0xFFFFFF, (byte)value);
                return value;
            case 0b010 when byteCount == 4: // (An)
                WriteLongWord(processorContext, processorContext.A[register] & 0xFFFFFF, value);
                return value;
            case 0b011 when byteCount == 1: // (An)+
                WriteByte(processorContext, processorContext.A[register] & 0xFFFFFF, (byte)value);
                processorContext.A[register] += (uint)(register == 7 ? 2 : 1);
                return value;
            case 0b100 when byteCount == 1: // -(An)
                processorContext.A[register] -= (uint)(register == 7 ? 2 : 1);
                WriteByte(processorContext, processorContext.A[register] & 0xFFFFFF, (byte)value);
                return value & 0xFF;
            case 0b101: // (d16,An)
                var b101Address = (uint)(processorContext.A[register] + ReadShortFromPrefetch(processorContext));
                WriteByte(processorContext, b101Address & 0xFFFFFF, (byte)value);
                return value;
            case 0b110: // (d8,An,Xn)
                var b100AnRegister = processorContext.A[register];
                var b100XRegisterValue = GetB100ExtensionValue(processorContext);
                var b100Displacement = ReadSbyteFromPrefetch(processorContext);
                var b100Address = (uint)(b100AnRegister + b100XRegisterValue + b100Displacement);
                switch (byteCount)
                {
                    case 1:
                        WriteByte(processorContext, b100Address & 0xFFFFFF, (byte)value);
                        break;
                    case 2:
                        WriteWord(processorContext, b100Address & 0xFFFFFF, (ushort)value);
                        break;
                    case 4:
                        WriteLongWord(processorContext, b100Address & 0xFFFFFF, value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return value;
            case 0b111 when register == 0b000 && byteCount == 1: // (xxx).W
                var b111A = (uint)processorContext.GetPrefetchWord();
                b111A = (b111A >> 15 == 1)
                    ? (b111A | 0xFF0000) & 0xFFFFFF
                    : b111A;
                WriteByte(processorContext, b111A, (byte)value);
                return value & 0xFF;
            case 0b111 when register == 0b001 && byteCount == 1 : // (xxx).L
                WriteByte(processorContext, processorContext.GetPrefetchLongWord() & 0xFFFFFF, (byte)value);
                return value & 0xFF;
            default:
                throw new NotImplementedException($"Write: Byte count {byteCount}, address mode {addressMode:b3}, and register {register:b3} is not implemented.");
        }
    }

    private int GetB100ExtensionValue(ProcessorContext processorContext)
    {
        // https://www.cs.emory.edu/~cheung/Courses/255/Syllabus/Old-7-M68000/Docs/68K-manual.pdf
        // https://github.com/captain-amygdala/pistorm/blame/main/m68kcpu.h#L1558-L1560C52
        /*
        * Brief extension format:
        *  F  |  E D C   |  B  |  A 9  | 8 | 7 6 5 4 3 2 1 0
        * D/A | REGISTER | W/L | SCALE | 0 |  DISPLACEMENT
        */
        var flags = processorContext.GetPrefetchByte();
        var indexRegisterNumber = (uint)(flags >> 4) & 0b111;
        var indexRegisterType = flags >> 7;
        var responseSize = (flags >> 3) & 1;
        var response = indexRegisterType == 1
            ? processorContext.A[indexRegisterNumber]
            : processorContext.D[indexRegisterNumber];
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

    protected int ReadIntFromPrefetch(ProcessorContext processorContext)
    {
        return ConvertLongWordToInt(processorContext.GetPrefetchLongWord());
    }

    protected short ConvertWordToShort(ushort value)
    {
        return (short)(((value >> 15) & 1) == 1 ? -1 - (value ^ 0xFFFF) : value);
    }

    protected short ReadShortFromPrefetch(ProcessorContext processorContext)
    {
        return ConvertWordToShort(processorContext.GetPrefetchWord());
    }

    protected sbyte ConvertByteToSbyte(byte value)
    {
        return (sbyte)(((value >> 7) & 1) == 1 ? -1 - (value ^ 0xFF) : value);
    }

    protected sbyte ReadSbyteFromPrefetch(ProcessorContext processorContext)
    {
        return ConvertByteToSbyte(processorContext.GetPrefetchByte());
    }

    protected void WriteByte(ProcessorContext processorContext, uint address, byte value)
    {
        processorContext.Memory[address] = value;
    }

    protected uint ReadByte(ProcessorContext processorContext, uint address)
    {
        return processorContext.Memory[address];
    }

    protected void WriteWord(ProcessorContext processorContext, uint address, ushort value)
    {
        processorContext.Memory[address] = (byte)(value >> 8 & 0xFF);
        processorContext.Memory[address + 1] = (byte)(value & 0xFF);
    }

    protected uint ReadWord(ProcessorContext processorContext, uint address)
    {
        var result =
            (uint)((processorContext.Memory[address + 2] << 8) +
                   processorContext.Memory[address + 3]);
        return result;
    }

    protected uint ReadLongWord(ProcessorContext processorContext, uint address)
    {
        var result =
            (uint)((processorContext.Memory[address] << 24) +
            (processorContext.Memory[address + 1] << 16) +
            (processorContext.Memory[address + 2] << 8) +
            processorContext.Memory[address + 3]);
        return result;
    }

    protected void WriteLongWord(ProcessorContext processorContext, uint address, uint value)
    {
        processorContext.Memory[address] = (byte)(value >> 24 & 0xFF);
        processorContext.Memory[address + 1] = (byte)(value >> 16 & 0xFF);
        processorContext.Memory[address + 2] = (byte)(value >> 8 & 0xFF);
        processorContext.Memory[address + 3] = (byte)(value & 0xFF);
    }
}
