using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OrionEngine;


namespace OrionIDE;

internal class Program
{
    static void Main(string[] args)
    {
        ExampleBehaviour example = new ExampleBehaviour();
        EngineCore.EngineCoreInit();


        //DX11QuadExample quadExample = new DX11QuadExample();
    }
}
