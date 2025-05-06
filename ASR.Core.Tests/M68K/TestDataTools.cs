using System.Text.Json;
using Shouldly;

namespace ASR.Core.Tests.M68K;

public static class TestDataTools
{
    public static IEnumerable<object[]> GetTestData(IEnumerable<string> variants)
    {
        // hacky thing to get to resources in parent path
        var contentsPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        contentsPath = Path.GetDirectoryName(contentsPath); // remove /
        contentsPath = Path.GetDirectoryName(contentsPath); // remove net9.0
        contentsPath = Path.GetDirectoryName(contentsPath); // remove debug
        contentsPath = Path.GetDirectoryName(contentsPath); // remove bin
        contentsPath = Path.GetDirectoryName(contentsPath); // remove ASR.Core.Tests

        foreach (var variant in variants)
        {
            var filename = Path.Combine(contentsPath, $"m68000/v1/{variant}.json");

            if (!File.Exists(filename))
                throw new Exception($"File {filename} does not exist... did you forget to do a recursive pull or decode the tests?");

            using var stream = File.OpenRead(filename);
            var jsonDocument = JsonDocument.Parse(stream);
            var rootElement = jsonDocument.RootElement;

            for (var testIndex = 0; testIndex < rootElement.GetArrayLength(); testIndex++)
            {
                Console.WriteLine($"Test: {testIndex}");

                var property = rootElement[testIndex];

                var name = property.GetProperty("name").GetString() ?? "";
                var initial = property.GetProperty("initial");
                var ram = initial.GetProperty("ram");

                var memory = new DictionaryMemory(new Dictionary<uint, byte>());
                var processorContext = new ASR.Core.M68K.ProcessorContext(memory)
                {
                    ProgramCounter = initial.GetProperty("pc").GetUInt32(),
                    StatusRegister = initial.GetProperty("sr").GetUInt16(),
                    UserStackPointer = initial.GetProperty("usp").GetUInt32(),
                    SupervisorStackPointer = initial.GetProperty("ssp").GetUInt32(),
                    Prefetch = new Queue<byte>()
                };

                for (var pfIndex = 0; pfIndex <= 1; pfIndex++)
                {
                    var prefetch = initial.GetProperty("prefetch")[pfIndex].GetUInt16();
                    processorContext.Prefetch.Enqueue((byte)(prefetch >> 8));
                    processorContext.Prefetch.Enqueue((byte)(prefetch & 0xFF));
                }

                for (var r = 0; r < ram.GetArrayLength(); r++)
                {
                    processorContext.Memory[ram[r][0].GetUInt32()] = ram[r][1].GetByte();
                }

                for (uint i = 0; i < 8; i++)
                {
                    if (i != 7)
                        processorContext.A[i] = initial.GetProperty($"a{i}").GetUInt32();

                    processorContext.D[i] = initial.GetProperty($"d{i}").GetUInt32();
                }

                var formattedName = $"({testIndex}) {name}";

                yield return [new ContextScenario(formattedName, processorContext, property.GetProperty("final"))];
            }
        }
    }

    public static void ValidateTest(ContextScenario contextScenario)
    {
        contextScenario.ProcessorContext.A[0].ShouldBe(contextScenario.JsonElement.GetProperty("a0").GetUInt32());
        contextScenario.ProcessorContext.A[1].ShouldBe(contextScenario.JsonElement.GetProperty("a1").GetUInt32());
        contextScenario.ProcessorContext.A[2].ShouldBe(contextScenario.JsonElement.GetProperty("a2").GetUInt32());
        contextScenario.ProcessorContext.A[3].ShouldBe(contextScenario.JsonElement.GetProperty("a3").GetUInt32());
        contextScenario.ProcessorContext.A[4].ShouldBe(contextScenario.JsonElement.GetProperty("a4").GetUInt32());
        contextScenario.ProcessorContext.A[5].ShouldBe(contextScenario.JsonElement.GetProperty("a5").GetUInt32());
        contextScenario.ProcessorContext.A[6].ShouldBe(contextScenario.JsonElement.GetProperty("a6").GetUInt32());

        contextScenario.ProcessorContext.D[0].ShouldBe(contextScenario.JsonElement.GetProperty("d0").GetUInt32());
        contextScenario.ProcessorContext.D[1].ShouldBe(contextScenario.JsonElement.GetProperty("d1").GetUInt32());
        contextScenario.ProcessorContext.D[2].ShouldBe(contextScenario.JsonElement.GetProperty("d2").GetUInt32());
        contextScenario.ProcessorContext.D[3].ShouldBe(contextScenario.JsonElement.GetProperty("d3").GetUInt32());
        contextScenario.ProcessorContext.D[4].ShouldBe(contextScenario.JsonElement.GetProperty("d4").GetUInt32());
        contextScenario.ProcessorContext.D[5].ShouldBe(contextScenario.JsonElement.GetProperty("d5").GetUInt32());
        contextScenario.ProcessorContext.D[6].ShouldBe(contextScenario.JsonElement.GetProperty("d6").GetUInt32());
        contextScenario.ProcessorContext.D[7].ShouldBe(contextScenario.JsonElement.GetProperty("d7").GetUInt32());

        contextScenario.ProcessorContext.ProgramCounter.ShouldBe(contextScenario.JsonElement.GetProperty("pc").GetUInt32());
        contextScenario.ProcessorContext.UserStackPointer.ShouldBe(contextScenario.JsonElement.GetProperty("usp").GetUInt32());
        contextScenario.ProcessorContext.SupervisorStackPointer.ShouldBe(contextScenario.JsonElement.GetProperty("ssp").GetUInt32());
        contextScenario.ProcessorContext.StatusRegister.ShouldBe(contextScenario.JsonElement.GetProperty("sr").GetUInt16());

        contextScenario.ProcessorContext.GetPrefetchWord().ShouldBe(contextScenario.JsonElement.GetProperty("prefetch")[0].GetUInt16());
        contextScenario.ProcessorContext.GetPrefetchWord().ShouldBe(contextScenario.JsonElement.GetProperty("prefetch")[1].GetUInt16());

        var finalRam = contextScenario.JsonElement.GetProperty("ram");
        for (var finalRamIndex = 0; finalRamIndex < finalRam.GetArrayLength(); finalRamIndex++)
        {
            var i = finalRam[finalRamIndex][0].GetUInt32();
            var v = finalRam[finalRamIndex][1].GetByte();
            contextScenario.ProcessorContext.Memory[i].ShouldBe(v);
        }
    }
}
