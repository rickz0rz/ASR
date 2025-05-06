namespace ASR.Core;

public interface IMemory
{
    public byte this[uint index] { get; set; }
}
