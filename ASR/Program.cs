using ASR.Core;
using ASR.Core.Amiga;

namespace ASR;

class Program
{
    static void Main(string[] args)
    {
        EmulatorConfiguration.DebugPrint = true;

        // https://github.com/Sakura-IT/Amiga-programming-examples/blob/master/ASM/HelloWorld/helloworld.s
        // new AmigaEmulator("/Users/rj/Downloads/vasm/helloworld").Execute();

        new AmigaEmulator("/Users/rj/Downloads/ESQ").Execute();
    }
}
