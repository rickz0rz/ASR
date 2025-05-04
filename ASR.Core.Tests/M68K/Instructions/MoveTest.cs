using ASR.Core.M68K.Instructions;
using ASR.Core.Tests.Common;
using Shouldly;

namespace ASR.Core.Tests.M68K.Instructions;

public class MoveTest
{
    public static IEnumerable<object[]> Data()
    {
        return TestDataTools.GetTestData(["MOVE.b"]);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void DoTest(ContextScenario contextScenario)
    {
        var instruction = BaseInstruction.GetInstruction(contextScenario.CpuContext);
        instruction.ShouldBeOfType<Move>();
        instruction.Execute(contextScenario.CpuContext);
        TestDataTools.ValidateTest(contextScenario);
    }
}
