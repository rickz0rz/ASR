using System.Text;
using ASR.Core.M68K;

namespace ASR.Core;

public class CPUContext
{
    public uint ProgramCounter { get; set; }

    public AddressRegisters A { get; set; }
    public uint[] D { get; set; }

    public uint UserStackPointer { get; set; }
    public uint SupervisorStackPointer { get; set; }

    public ushort StatusRegister { get; set; }

    // A7 aliases to either USP or SSP depending on the mode you're in.
    // https://stackoverflow.com/q/48826709 see here in status register for mode.

    public uint StackPointer
    {
        get => IsInSupervisorMode ? SupervisorStackPointer : UserStackPointer;
        set
        {
            if (IsInSupervisorMode)
            {
                SupervisorStackPointer = value;
            }
            else
            {
                UserStackPointer = value;
            }
        }
    }

    public void PopulatePrefetch()
    {
        while (Prefetch.Count < 4)
        {
            Prefetch.Enqueue(Memory[ProgramCounter++]);
        }
    }

    public byte GetPrefetchByte()
    {
        Prefetch.Enqueue(Memory[ProgramCounter++]);
        return Prefetch.Dequeue();
    }

    public ushort GetPrefetchWord()
    {
        var hi = GetPrefetchByte();
        var lo =  GetPrefetchByte();
        return (ushort)(hi << 8 | lo);
    }

    public uint GetPrefetchLongWord()
    {
        var a = GetPrefetchByte();
        var b = GetPrefetchByte();
        var c = GetPrefetchByte();
        var d = GetPrefetchByte();
        return (uint)((a << 24) | (b << 16) | (c << 8) | d);
    }

    public bool IsInSupervisorMode
    {
        get => (StatusRegister & 0b0010_0000_0000_0000) == 0b0010_0000_0000_0000;
        set
        {
            if (value)
            {
                StatusRegister |= 0b0010_0000_0000_0000;
            }
            else
            {
                StatusRegister &= 0b1101_1111_1111_1111;
            }
        }
    }

    public bool NFlag
    {
        get => (StatusRegister & 8) == 8;
        set
        {
            if (value)
                StatusRegister |= 8;
            else
                StatusRegister &= 0b1111_1111_1111_0111;
        }
    }

    public bool ZFlag
    {
        get => (StatusRegister & 4) == 4;
        set
        {
            if (value)
                StatusRegister |= 4;
            else
                StatusRegister &= 0b1111_1111_1111_1011;
        }
    }

    public bool VFlag
    {
        get => (StatusRegister & 2) == 2;
        set
        {
            if (value)
                StatusRegister |= 2;
            else
                StatusRegister &= 0b1111_1111_1111_1101;
        }
    }

    public bool CFlag
    {
        get => (StatusRegister & 1) == 1;
        set
        {
            if (value)
                StatusRegister |= 1;
            else
                StatusRegister &= 0b1111_1111_1111_1110;
        }
    }

    // 16MB is the maximum addressable size on the m68k
    private const int HeapSize = 0xFFFFFF;

    // public readonly Dictionary<long, BaseLibrary?> Libraries = new();
    // public readonly Dictionary<long, Action<Context>> LibraryActions = new();

    public Stack<byte> Stack { get; set; } // Use A7 here instead?
    public IMemory Memory { get; set; }
    public Queue<byte> Prefetch { get; set; }

    public CPUContext(IMemory memory)
    {
        Stack = new Stack<byte>();
        Memory = memory;
        Prefetch = new Queue<byte>();

        A = new AddressRegisters(this);
        D = new uint[8];
    }

    public void PushLongToStack(uint value)
    {
        Stack.Push((byte)(value >> 24 & 0xFF));
        Stack.Push((byte)(value >> 16 & 0xFF));
        Stack.Push((byte)(value >> 8 & 0xFF));
        Stack.Push((byte)(value & 0xFF));
    }

    public void AllocateStringObject(string str, uint address)
    {
        var bytes = Encoding.ASCII.GetBytes(str);
        var addr = address;
        foreach (var b in bytes)
        {
            Memory[addr] = b;
            addr++;
        }
        Memory[addr] = 0x00;
    }

    public string ReadStringFromMemory(uint address)
    {
        var currentAddress = address;
        var stringBuilder = new StringBuilder();

        while (Memory[currentAddress] != 0)
        {
            stringBuilder.Append((char)Memory[currentAddress]);
            currentAddress += 1;
        }

        return stringBuilder.ToString();
    }

    /*
    public void PopulateLibraryActions()
    {
        LibraryActions.Clear();

        foreach (var library in Libraries)
        {
            if (library.Value == null)
                continue;

            foreach (var libraryAction in library.Value.Commands)
            {
                LibraryActions.Add(library.Key + libraryAction.Key, libraryAction.Value);
            }
        }
    }
    */
}
