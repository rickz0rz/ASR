using ASR.Core.Amiga.Hunk;
using ASR.Core.M68K.Instructions;

namespace ASR.Core.Amiga;

public class AmigaEmulator : Emulator
{
    private const uint DefaultEntryPoint = 0x10000;

    public AmigaEmulator(string programPath) : this(File.ReadAllBytes(programPath))
    {
    }

    public AmigaEmulator(byte[] programData) : base(new AmigaCpuContext(new ArrayMemory(0xFFFFFF)))
    {
        var hunk = HunkParser.Parse(programData);
        var hunkStartingAddress = DefaultEntryPoint;

        for (var hsi = 0; hsi < hunk.HunkSections.Count; hsi++)
        {
            if (hunk.DREL32Relocations.TryGetValue(hsi, out var relocation))
            {
                hunkStartingAddress = (uint)relocation.offset;
            }

            var hunkSection = hunk.HunkSections[hsi];

            for (var i = 0; i < hunkSection.Data.Count; i++)
                CpuContext.Memory[(uint)i + hunkStartingAddress] = hunkSection.Data[i];

            hunkStartingAddress += (uint)(hunkSection.Data.Count);
        }

        for (var hunkSectionIndex = 0; hunkSectionIndex < hunk.HunkSections.Count; hunkSectionIndex++)
        {
            if (!hunk.DREL32Relocations.ContainsKey(hunkSectionIndex))
                continue;

            foreach (var relocationAddress in hunk.DREL32Relocations[hunkSectionIndex].addresses)
            {
                var offsetAddress = relocationAddress + hunk.DREL32Relocations[hunkSectionIndex].offset;
                var memoryAddress = ReadLongFromMemoryAddress((uint)offsetAddress);
                memoryAddress += (uint)hunk.DREL32Relocations[hunkSectionIndex].offset;
                WriteLongToMemoryAddress((uint)offsetAddress, memoryAddress);
            }
        }
    }

    public void Execute()
    {
        Execute(DefaultEntryPoint, AmigaSystemExecutionHook);
    }

    private bool AmigaSystemExecutionHook(CPUContext cpuContext, BaseInstruction instruction)
    {
        // This is a special hook to handle Amiga-specific functionality.
        // Specifically, this will be used to handle library calls as they use
        // a negative offset when doing a JSR.
        return false;
    }

    // Might not need this with reading from the Prefetch...?
    private uint ReadLongFromMemoryAddress(uint address)
    {
        uint longValue = CpuContext.Memory[address];
        longValue = (longValue << 8) + CpuContext.Memory[address + 1];
        longValue = (longValue << 8) + CpuContext.Memory[address + 2];
        longValue = (longValue << 8) + CpuContext.Memory[address + 3];
        return longValue;
    }

    private void WriteLongToMemoryAddress(uint address, uint value)
    {
        CpuContext.Memory[address + 3] = (byte)(value & 0xFF);
        CpuContext.Memory[address + 2] = (byte)((value >> 8) & 0xFF);
        CpuContext.Memory[address + 1] = (byte)((value >> 16) & 0xFF);
        CpuContext.Memory[address] = (byte)((value >> 24) & 0xFF);
    }
}
