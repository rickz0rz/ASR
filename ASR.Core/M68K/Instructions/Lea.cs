namespace ASR.Core.M68K.Instructions;

public class Lea : BaseInstruction
{
    private const int InstMask = 0b1111_0001_1100_0000;
    private const int InstMaskTarget = 0b0100_0001_1100_0000;

    private uint _addressRegister;

    public Lea(ushort opcode, CPUContext cpuContext) : base(opcode)
    {
        _addressRegister = (uint)(Opcode >> 9) & 0b111;
    }

    public static bool IsInstruction(ushort opcode)
    {
        return (opcode & InstMask) == InstMaskTarget;
    }

    public override bool Execute(CPUContext cpuContext)
    {
        cpuContext.A[_addressRegister] = GetEffective(Opcode, cpuContext);
        return true;
    }
}
