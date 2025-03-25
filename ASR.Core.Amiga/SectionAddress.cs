namespace ASR.Core.Amiga;

public sealed record SectionAddress
{
    public required int SectionNumber { get; init; }
    public required int Address { get; init; }

    public override string ToString()
    {
        return $"#$[{SectionNumber}]{Address:X8}";
    }
}
