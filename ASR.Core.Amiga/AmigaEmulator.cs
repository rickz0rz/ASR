using ASR.Core.Amiga.Hunk;

namespace ASR.Core.Amiga;

public class AmigaEmulator : Emulator
{
    private const uint DefaultEntryPoint = 0x10000;

    public AmigaEmulator(string programPath) : base(new AmigaContext())
    {
        var hunk = HunkParser.Parse(programPath);

        Context.ProgramCounter = DefaultEntryPoint;
        var hunkStartingAddress = DefaultEntryPoint;
        var codeSectionIndices = new Dictionary<int, uint>();

        // Load the hunks into memory
        for (var hsi = 0; hsi < hunk.HunkSections.Count; hsi++)
        {
            var hunkSection = hunk.HunkSections[hsi];
            codeSectionIndices.Add(hsi, hunkStartingAddress);

            for (var i = 0; i < hunkSection.Data.Count; i++)
                Context.Memory[i + hunkStartingAddress] = hunkSection.Data[i];

            // The 0x10000 buffer between hunks is completely arbitrary.
            hunkStartingAddress += (uint)(hunkSection.Data.Count + 0x10000);
        }
    }
}
