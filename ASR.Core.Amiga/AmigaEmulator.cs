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
        var hunkSectionAddressMap = new Dictionary<int, uint>();

        // Load the hunks into memory
        for (var hsi = 0; hsi < hunk.HunkSections.Count; hsi++)
        {
            if (hunk.DREL32Relocations.ContainsKey(hsi))
            {
                hunkStartingAddress = (uint)hunk.DREL32Relocations[hsi].offset;
            }

            hunkSectionAddressMap.Add(hsi, hunkStartingAddress);

            var hunkSection = hunk.HunkSections[hsi];

            for (var i = 0; i < hunkSection.Data.Count; i++)
                Context.Memory[i + hunkStartingAddress] = hunkSection.Data[i];

            hunkStartingAddress += (uint)(hunkSection.Data.Count);
        }

        // Adjust the memory locations from the relocation tables.
        for (var hsi = 0; hsi < hunk.HunkSections.Count; hsi++)
        {
            if (!hunk.DREL32Relocations.ContainsKey(hsi))
                continue;

            foreach (var addr in hunk.DREL32Relocations[hsi].addresses)
            {
                var address = addr + hunk.DREL32Relocations[hsi].offset;

                // Read byte.
                var v = ReadLongFromMemoryAddress((uint)address);

                // Add the offset.
                v += (uint)hunk.DREL32Relocations[hsi].offset;

                // Write the bytes back.
                WriteLongToMemoryAddress((uint)address, v);
            }
        }
    }

    private uint ReadLongFromMemoryAddress(uint address)
    {
        uint v = Context.Memory[address];
        v = (v << 8) + Context.Memory[address + 1];
        v = (v << 8) + Context.Memory[address + 2];
        v = (v << 8) + Context.Memory[address + 3];
        return v;
    }

    private void WriteLongToMemoryAddress(uint address, uint value)
    {
        Context.Memory[address + 3] = (byte)(value & 0xFF);
        Context.Memory[address + 2] = (byte)((value >> 8) & 0xFF);
        Context.Memory[address + 1] = (byte)((value >> 16) & 0xFF);
        Context.Memory[address] = (byte)((value >> 24) & 0xFF);
    }
}
