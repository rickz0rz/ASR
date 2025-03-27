using System.Text;
using ASR.Core.Libraries;

namespace ASR.Core;

public class Context
{
    public uint ProgramCounter { get; set; }
    public long[] A { get; set; }
    public uint[] D { get; set; }

    public bool XFlag { get; set; }
    public bool NFlag { get; set; }
    public bool ZFlag { get; set; }
    public bool VFlag { get; set; }
    public bool CFlag { get; set; }

    private const uint HeapSize = 16 * 1024 * 1024; // 16MB

    public readonly Dictionary<long, BaseLibrary?> Libraries = new();
    public readonly Dictionary<long, Action<Context>> LibraryActions = new();

    public Stack<byte> Stack { get; set; } // Use A7 here instead?
    public byte[] Memory { get; set; }

    public Context()
    {
        Stack = new Stack<byte>();
        Memory = new byte[HeapSize];
        for (var i = 0; i < HeapSize; i++)
        {
            Memory[i] = 0;
        }

        A = new long[8];
        D = new uint[8];

        NFlag = false;
        ZFlag = false;
    }

    public void PushLongToStack(uint value)
    {
        Stack.Push((byte)(value >> 24 & 0xFF));
        Stack.Push((byte)(value >> 16 & 0xFF));
        Stack.Push((byte)(value >> 8 & 0xFF));
        Stack.Push((byte)(value & 0xFF));
    }

    public void AllocateStringObject(string str, long address)
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

    public string ReadStringFromMemory(long address)
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
}
