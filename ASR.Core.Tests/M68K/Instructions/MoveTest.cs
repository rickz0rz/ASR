using ASR.Core.M68K.Instructions;
using Shouldly;

namespace ASR.Core.Tests.M68K.Instructions;

public class MoveTest
{
    public static IEnumerable<object[]> MoveBData()
    {
        return TestDataTools.GetTestData(["MOVE.b"]);
    }

    [Theory]
    [MemberData(nameof(MoveBData))]
    public void MoveBTest(ContextScenario contextScenario)
    {
        var instruction = BaseInstruction.GetInstruction(contextScenario.ProcessorContext);
        instruction.ShouldBeOfType<Move>();
        instruction.Execute(contextScenario.ProcessorContext);
        TestDataTools.ValidateTest(contextScenario);
    }

    public static IEnumerable<object[]> MoveWData()
    {
        return TestDataTools.GetTestData(["MOVE.w"]);
    }

    [Theory]
    [MemberData(nameof(MoveWData))]
    public void MoveWTest(ContextScenario contextScenario)
    {
        var instruction = BaseInstruction.GetInstruction(contextScenario.ProcessorContext);
        instruction.ShouldBeOfType<Move>();
        instruction.Execute(contextScenario.ProcessorContext);
        TestDataTools.ValidateTest(contextScenario);
    }
}
