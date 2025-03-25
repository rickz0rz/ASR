namespace ASR.Core.Amiga.Hunk;

public class HunkParser
{
    public static HunkFile Parse(string filename)
    {
        Console.WriteLine($"Parsing file: {filename}");

        var offset = 0;
        var hunk = new HunkFile();

        var fileData = File.ReadAllBytes(filename);

        hunk.Magic.AddRange(fileData.Take(4));
        offset += 4;

        // Skip 4 for the strings, assume it's a dummy 0x00000000 byte value for now 'cus I'm lazy.
        hunk.Strings = [];
        offset += 4;

        var numberOfSections = ConvertBytesToInt(fileData.Skip(offset).Take(4));
        offset += 4;

        hunk.FirstHunkSection = ConvertBytesToInt(fileData.Skip(offset).Take(4));
        offset += 4;

        hunk.LastHunkSection = ConvertBytesToInt(fileData.Skip(offset).Take(4));
        offset += 4;

        for (var sectionIndex = 0; sectionIndex < numberOfSections; sectionIndex++)
        {
            var sectionSize = ConvertBytesToInt(fileData.Skip(offset).Take(4));
            offset += 4;
            hunk.HunkSectionSizes.Add(sectionSize);
        }

        for (var sectionIndex = 0; sectionIndex < numberOfSections; sectionIndex++)
        {
            var sectionType = ConvertBytesToInt(fileData.Skip(offset).Take(4));
            offset += 4;

            var hunkSectionSize = ConvertBytesToInt(fileData.Skip(offset).Take(4)) * 4;
            offset += 4;

            var hunkSection = new HunkSection
            {
                SectionType = sectionType & 0x00FFFFFF,
                SectionMemoryFlag = sectionType >> 29,
                Data = fileData.Skip(offset).Take(hunkSectionSize).ToList()
            };
            offset += hunkSectionSize;

            var inHunkLoop = true;
            do
            {
                var subsectionValue = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                offset += 4;

                switch (subsectionValue)
                {
                    case 0x3EC:
                        // Relocation tables.
                        while (true)
                        {
                            var numberOfRelocationEntries = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                            offset += 4;

                            if (numberOfRelocationEntries == 0)
                            {
                                break;
                            }

                            var relocationSectionId = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                            offset += 4;

                            var relocationAddresses = new List<int>();

                            for (var i = 0; i < numberOfRelocationEntries; i++)
                            {
                                relocationAddresses.Add(ConvertBytesToInt(fileData.Skip(offset).Take(4)));
                                offset += 4;
                            }

                            hunkSection.RelocationTables.Add(relocationSectionId, relocationAddresses);
                        }
                        break;
                    case 0x3F2:
                        inHunkLoop = false;
                        break;
                    default:
                        throw new Exception($"Unknown subsection value: 0x{subsectionValue:X8}");
                }
            } while (inHunkLoop);

            hunk.HunkSections.Add(hunkSection);
        }

        return hunk;
    }

    private static int ConvertBytesToInt(IEnumerable<byte> bytes)
    {
        var byteList = bytes.ToList();
        return (byteList[0] << 24) | (byteList[1] << 16) | (byteList[2] << 8) | byteList[3];
    }
}
