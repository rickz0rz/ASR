using ASR.Core.Amiga.Libraries;
using ASR.Core.M68K;

namespace ASR.Core.Amiga;

public class AmigaCpuContext : CPUContext
{
    private const int AbsExecBase = 0x4;

    public AmigaCpuContext(IMemory memory) : base(memory)
    {
        //Libraries.Add(AbsExecBase, new ExecLibrary());
        //PopulateLibraryActions();
    }
}
