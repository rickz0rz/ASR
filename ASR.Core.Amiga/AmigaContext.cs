using ASR.Core.Amiga.Libraries;

namespace ASR.Core.Amiga;

public class AmigaContext : Context
{
    private const int AbsExecBase = 0x4;

    public AmigaContext()
    {
        Libraries.Add(ProgramCounter + AbsExecBase, new ExecLibrary());
        PopulateLibraryActions();
    }
}
