using OrionIDE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OrionEngine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EngineCore.EngineCoreInit();

            ExampleBehaviour example = new ExampleBehaviour();
            //DX11QuadExample quadExample = new DX11QuadExample();
        }
    }

}
