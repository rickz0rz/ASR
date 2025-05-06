namespace ASR.Core.Amiga.Libraries;

// https://d0.se/autodocs/dos.library
public class DosLibrary : BaseLibrary
{
    private const int PutStr = -948;

    public DosLibrary()
    {
        Commands.Add(PutStr, (context) =>
        {
            var addr = context.D[1];
            while (context.Memory[addr] != 0)
            {
                Console.Write((char)context.Memory[addr]);
                addr++;
            }
        });
    }
}
