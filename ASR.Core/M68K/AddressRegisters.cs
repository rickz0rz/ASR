namespace ASR.Core.M68K;

public class AddressRegisters(ProcessorContext processorContext)
{
    private readonly uint[] _array = new uint[7];

    public uint this[uint index]
    {
        get
        {
            if (index == 7)
                return processorContext.IsInSupervisorMode ? processorContext.SupervisorStackPointer : processorContext.UserStackPointer;

            return _array[index];
        }
        set
        {
            if (index == 7)
            {
                if (processorContext.IsInSupervisorMode)
                {
                    processorContext.SupervisorStackPointer = value;
                }
                else
                {
                    processorContext.UserStackPointer = value;
                }
            }
            else
            {
                _array[index] = value;
            }
        }
    }
}
