namespace ASR.Core.Amiga.Hunk;

public class HunkParser
{
    public static HunkFile Parse(string filename)
    {
        // Console.WriteLine($"Parsing file: {filename}");

        var offset = 0;
        var hunk = new HunkFile();

        var fileData = File.ReadAllBytes(filename);

        hunk.Magic.AddRange(fileData.Take(4));
        offset += 4;

        // A number of resident library names
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

        var inHunkLoop = true;
        var currentSectionIndex = 0;
        do
        {
            var sectionType = ConvertBytesToInt(fileData.Skip(offset).Take(4));
            offset += 4;

            var hunkSection = new HunkSection
            {
                SectionType = sectionType & 0x00FFFFFF,
                SectionMemoryFlag = sectionType >> 29
            };

            switch (hunkSection.SectionType)
            {
                case 0x3E9:
                case 0x3EA:
                    var numberOfLongWords = ConvertBytesToInt(fileData.Skip(offset).Take(4)) * 4;
                    offset += 4;
                    hunkSection.Data = fileData.Skip(offset).Take(numberOfLongWords).ToList();
                    offset += numberOfLongWords;
                    break;
                case 0x3EC:
                    while (true)
                    {
                        var numberOfOffsets = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                        offset += 4;

                        if (numberOfOffsets == 0)
                        {
                            break;
                        }

                        var relocationSectionId = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                        offset += 4;

                        var relocationAddresses = new List<uint>();

                        for (var i = 0; i < numberOfOffsets; i++)
                        {
                            relocationAddresses.Add((uint)ConvertBytesToInt(fileData.Skip(offset).Take(4)));
                            offset += 4;
                        }

                        hunk.RELOC32Relocations.Add((currentSectionIndex, relocationSectionId), relocationAddresses);
                    }
                    break;
                case 0x3F0:
                    // HUNK_SYMBOL
                    // just cheat and read this until it's zero.
                    while (true)
                    {
                        var t = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                        offset += 4;
                        if (t == 0) break;
                    }
                    break;
                case 0x3F2:
                    currentSectionIndex++;
                    if (currentSectionIndex >= numberOfSections)
                    {
                        inHunkLoop = false;
                    }
                    break;
                case 0x3F7:
                    // hunk_drel32 - More relocation tables.
                {
                    for (var sectionNumber = 0; sectionNumber < numberOfSections; sectionNumber++)
                    {
                        // Only do this for sections that are code or data sections.
                        if (hunk.HunkSections[sectionNumber].SectionType is not (0x3E9 or 0x3EA))
                            continue;

                        var baseAddress = ConvertBytesToInt(fileData.Skip(offset).Take(4));
                        offset += 4;

                        var listOfAddresses = new List<uint>();
                        while (true)
                        {
                            var g = ConvertBytesToInt(fileData.Skip(offset).Take(2));
                            offset += 2;

                            if (g == 0)
                                break;

                            listOfAddresses.Add((uint)g);
                        }

                        hunk.DREL32Relocations.Add(sectionNumber, (baseAddress, listOfAddresses));
                    }
                }
                    break;
                default:
                    throw new Exception($"Unknown section type value: 0x{sectionType:X8}");
            }

            hunk.HunkSections.Add(hunkSection);
        } while (inHunkLoop);

        return hunk;
    }

    private static int ConvertBytesToInt(IEnumerable<byte> bytes)
    {
        var t = 0;
        foreach (var b in bytes)
        {
            t = (t << 8) | b;
        }
        return t;
    }
}
