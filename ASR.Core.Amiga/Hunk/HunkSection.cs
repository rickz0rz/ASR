namespace ASR.Core.Amiga.Hunk;

public class HunkSection
{
    public int SectionType { get; set; }
    public List<byte> Data { get; set; }
    public int SectionMemoryFlag { get; set; }

    // Commenting this out until I can figure out how I want this to look.
    //public Dictionary<int, List<int>> RelocationTables { get; }

    public HunkSection()
    {
        Data = [];
        //RelocationTables = new Dictionary<int, List<int>>();
    }

    public HunkSection(Dictionary<int, List<int>> relocationTables)
    {
        Data = [];
        //RelocationTables = relocationTables;
    }
}
