using ASR.Core.M68K;

namespace ASR.Core.Tests;

public class DictionaryMemory(IDictionary<uint, byte> dictionary) : IMemory
{
    /// <summary>
    /// If true, throw an exception when a test attempts to read a value that's
    /// not present in the dictionary.
    /// </summary>
    private const bool StrictMemoryMode = false;

    public byte this[uint index]
    {
        get => StrictMemoryMode
            ? dictionary[index]
            : dictionary.TryGetValue(index, out var value)
                ? value
                : (byte)0;
        set => dictionary[index] = value;
    }

    public void Clear()
    {
        dictionary.Clear();
    }
}
