namespace ASR.Core.Amiga.Libraries;

// https://d0.se/autodocs/exec.library
public class ExecLibrary : BaseLibrary
{
    /*
    private const int OpenLibrary = -552;
    private const int CloseLibrary = -414;
    private const int SetSignal = -306;

    public ExecLibrary()
    {
        Commands.Add(OpenLibrary, (context) =>
        {
            if (context is not AmigaContext amigaContext)
                return;

            var lowestLibraryValue = amigaContext.Libraries.Keys.Min();
            var newLibraryOffset = amigaContext.Libraries[lowestLibraryValue].Commands.Keys.Min() - 36;

            BaseLibrary? newLibraryObject = null;

            var libraryName = amigaContext.ReadStringFromMemory(amigaContext.A[1]);
            amigaContext.D[0] = 0;

            switch (libraryName)
            {
                case "dos.library":
                    newLibraryObject = new DosLibrary();
                    break;
                default:
                    Console.WriteLine($"{typeof(ExecLibrary)} - Unsupported library: \"{libraryName}\"");
                    break;
            }

            if (newLibraryObject == null)
                return;

            if (EmulatorConfiguration.DebugPrint)
                Console.WriteLine($"{typeof(ExecLibrary)} - Loaded '{libraryName}' in at {newLibraryOffset}");

            amigaContext.Libraries.Add(newLibraryOffset, newLibraryObject);
            amigaContext.PopulateLibraryActions();
            amigaContext.D[0] = (uint)newLibraryOffset;
        });

        Commands.Add(CloseLibrary, (context) =>
        {
            Console.WriteLine($"{typeof(ExecLibrary)} - Closelibrary [Stub]");
        });

        Commands.Add(SetSignal, (context) =>
        {
            Console.WriteLine($"{typeof(ExecLibrary)} - SetSignal [D0: 0x{context.D[0]:X8}] [Stub]");
        });
    }
    */
}
