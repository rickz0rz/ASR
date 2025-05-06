namespace ASR.Core;

public class ArrayMemory(int length) : IMemory
{
    private readonly byte[] _array = new byte[length];

    public byte this[uint index]
    {
        get => _array[index];
        set => _array[index] = value;
    }
}
