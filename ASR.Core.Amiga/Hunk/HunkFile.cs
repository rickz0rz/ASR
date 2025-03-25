namespace ASR.Core.Amiga.Hunk;

public class HunkFile
{
    // http://amiga-dev.wikidot.com/file-format:hunk
    public List<byte> Magic { get; set; }
    public List<string> Strings { get; set; }
    public Dictionary<SectionAddress, string> Labels { get; set; }
    public int FirstHunkSection { get; set; }
    public int LastHunkSection { get; set; }
    public List<int> HunkSectionSizes { get; set; } // These may not reflect the actual hunks' sizes.. pad with zeros. Is this even necessary?
    public List<HunkSection> HunkSections { get; set; }

    public HunkFile()
    {
        Magic = new List<byte>();
        Strings = new List<string>();
        Labels = new Dictionary<SectionAddress, string>();
        HunkSectionSizes = new List<int>();
        HunkSections = new List<HunkSection>();
    }
}
