namespace ASR.Core.M68K;

public interface IMemory
{
    public byte this[uint index] { get; set; }
}
