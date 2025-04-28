using ASR.Core;
using ASR.Core.Amiga;

namespace ASR;

class Program
{
    static void Main(string[] args)
    {
        EmulatorConfiguration.DebugPrint = true;

        // Way i'm going to do this:
        // - Load all program data into an array\
        // - Create a map of instructions (address -> instruction)
        // - As program execute occurs, see if we have a map value for the program instructions (converted to something more exec friendly)
        // - If we don't, generate the code and store it in the map. Execute from the map.
        //   - Maybe look at Reflection.Emit https://www.c-sharpcorner.com/UploadFile/puranindia/reflection-and-reflection-emit-in-C-Sharp/
        //   - Or some other way to create JIT? How does RyuJinx do it?
        // - Instruction is a class that can take a CPU state and execute against it, altering it
        // - Each function will adjust the address register or whatever appropriately
        // - JSRs will either jump to a new position on the map or if negative execute a predefined function
        // - A function override map can exist that will execute code if an entry exists in the map
        // - Said function override can fall back and execute the code in the original map at any time

        // https://github.com/Sakura-IT/Amiga-programming-examples/blob/master/ASM/HelloWorld/helloworld.s
        new AmigaEmulator("/Users/rj/Downloads/vasm/helloworld").Execute();
        // new AmigaEmulator("/Users/rj/Downloads/ESQ").Execute();
    }
}
