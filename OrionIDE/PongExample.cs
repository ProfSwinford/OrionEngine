using OrionEngine;
using OrionEngine.EngineModules.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrionIDE
{
    public class PongExample : OrionBehaviour
    {

        private int _x;
        private int _y;
        private int _xVel;
        private int _yVel;

        public void Awake() 
        {
            _x = 0;
            _y = 0;
            _xVel = 1;
            _yVel = 1;
        }

        public void Update() 
        {
            //ConsoleRendererModule.canvas.Clear();
            //ConsoleRendererModule.canvas.CreateBorder();
            _x += _xVel;
            _y += _yVel;

            if (_x < 1)
            {
                _x = 1;
                _xVel = 1;
            }
            else if (_x + 1 >= ConsoleRendererModule.canvas.Width)
            {
                _x = ConsoleRendererModule.canvas.Width - 2;
                _xVel = -1;
            }

            if (_y < 1)
            {
                _y = 1;
                _yVel = 1;
            }
            else if (_y + 1 >= ConsoleRendererModule.canvas.Height)
            {
                _y = ConsoleRendererModule.canvas.Height - 2;
                _yVel = -1;
            }
            //Print($"Ball Position: ({_x}, {_y})");
            //ConsoleRendererModule.canvas.Set(_x, _y, ConsoleColor.Blue);
        }

        public void LateUpdate()
        {
            Print($"Ball Position: ({_x}, {_y})");
            ConsoleRendererModule.canvas.Set(_x, _y, ConsoleColor.Blue);
        }
    }
}
