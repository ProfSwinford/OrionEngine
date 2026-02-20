using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrionEngine;

namespace OrionIDE
{
    internal class ExampleBehaviour : OrionBehaviour
    {
        int count = 0;
        public void Update() 
        {
            count++;
            EngineUtils.ConsoleWriteLoadingDots(count, 5);
        }
    }
}
