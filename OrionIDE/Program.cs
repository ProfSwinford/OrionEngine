using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OrionEngine;
using OrionEngine.EngineModules.Rendering;


namespace OrionIDE;

internal class Program
{
    static void Main(string[] args)
    {
        //ExampleBehaviour example = new ExampleBehaviour();
        PongExample p = new PongExample();
        //Pong p = new Pong();
        EngineCore.EngineCoreInit();

        //DX11QuadExample quadExample = new DX11QuadExample();
    }
}
