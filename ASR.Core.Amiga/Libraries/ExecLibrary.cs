using ASR.Core.Libraries;

namespace ASR.Core.Amiga.Libraries;

// https://d0.se/autodocs/exec.library
public class ExecLibrary : BaseLibrary
{
    private const int OpenLibrary = -552;
    private const int CloseLibrary = -414;

    public ExecLibrary()
    {
        Commands.Add(OpenLibrary, (context) =>
        {
            if (context is not AmigaContext amigaContext)
                return;

            var newLibraryOffset = 30000 + (amigaContext.Libraries.Count * 10000);

            BaseLibrary? newLibraryObject = null;

            var libraryName = amigaContext.ReadStringFromMemory(amigaContext.A[1]);
            amigaContext.D[0] = 0;

            switch (libraryName)
            {
                case "dos.library":
                    newLibraryObject = new DosLibrary();
                    break;
                default:
                    Console.WriteLine($"{typeof(ExecLibrary)} - Unknown library name: {libraryName}");
                    break;
            }

            if (newLibraryObject == null)
                return;

            amigaContext.Libraries.Add(newLibraryOffset + 0x10000, (BaseLibrary)newLibraryObject);
            amigaContext.PopulateLibraryActions();
            amigaContext.D[0] = (uint)newLibraryOffset;
        });

        Commands.Add(CloseLibrary, (context) =>
        {
            // Stub.
        });
    }
}
