//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using OrionEngine;

namespace OrionIDE
{
    internal class ExampleBehaviour : OrionBehaviour
    {
        public void Awake() 
        {
            Print("ExampleBehaviour Awake");
        }
        public void OnEnable() 
        {
            Print("ExampleBehaviour OnEnable");
        }
        public void Start() 
        {
            Print("ExampleBehaviour Start");
        }
        public void FixedUpdate() 
        {
            if (count < 1)
                Print("ExampleBehaviour FixedUpdate");
        }
        int count = 0;
        public void Update() 
        {
            count++;
            EngineUtils.ConsoleWriteLoadingDots(count, 5);

            if (Input.GetKeyDown(Keycode.A)) { }
        }
        public void LateUpdate() 
        {

            if (count < 1)
                Print("ExampleBehaviour LateUpdate");
        }
        public void OnDisable() 
        {
            Print("ExampleBehaviour OnDisable");
        }
        public void OnDestroy() 
        {
            Print("ExampleBehaviour OnDestroy");
        }
    }

}
