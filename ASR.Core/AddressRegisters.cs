namespace ASR.Core;

public class AddressRegisters(CPUContext cpuContext)
{
    private readonly uint[] _array = new uint[7];

    public uint this[uint index]
    {
        get
        {
            if (index == 7)
                return cpuContext.IsInSupervisorMode ? cpuContext.SupervisorStackPointer : cpuContext.UserStackPointer;
            return _array[index];
        }
        set
        {
            if (index == 7)
            {
                if (cpuContext.IsInSupervisorMode)
                {
                    cpuContext.SupervisorStackPointer = value;
                }
                else
                {
                    cpuContext.UserStackPointer = value;
                }
            }
            else
            {
                _array[index] = value;
            }
        }
    }
}
