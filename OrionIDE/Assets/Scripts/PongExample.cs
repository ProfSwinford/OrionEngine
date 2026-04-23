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
        private GameObject2D pongObj;
        private int _x;
        private int _y;
        private int _xVel;
        private int _yVel;

        int color = 0;

        public void Awake() 
        {
            Pixel[,] pongArt = new Pixel[1,3] 
            {
                { new Pixel('D'), new Pixel('V'), new Pixel('D')}
            };

            pongObj = new GameObject2D(3,1,pongArt);
            _x = 0;
            _y = 0;
            _xVel = 1;
            _yVel = 1;
        }

        public void Update() 
        {
            if (Input.GetKey(Keycode.Space))
                return;

            if(Input.GetKeyDown(Keycode.Escape))
                Environment.Exit(0);

            ModifiedPong();
        }

        public void ModifiedPong()
        {
            color += 1;
            pongObj.transform.position.x += _xVel;
            pongObj.transform.position.y += _yVel;

            if (pongObj.transform.position.x < 1)
            {
                pongObj.transform.position.x = 1;
                _xVel = 1;
            }
            else if (pongObj.transform.position.x + pongObj.width + 1 >= ConsoleRendererModule.canvas.Width)
            {
                _x = ConsoleRendererModule.canvas.Width - (pongObj.width+1);
                _xVel = -1;
            }

            if (pongObj.transform.position.y < 1)
            {
                pongObj.transform.position.y = 1;
                _yVel = 1;
            }
            else if (pongObj.transform.position.y + pongObj.height + 1 >= ConsoleRendererModule.canvas.Height)
            {
                pongObj.transform.position.y = ConsoleRendererModule.canvas.Height - (pongObj.height+1);
                _yVel = -1;
            }
        }

        public void OriginalPong() 
        {
            color += 1;
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
        }

        public void LateUpdate()
        {
            //Print($"Ball Position: ({_x}, {_y})");
            //ConsoleRendererModule.canvas.Set(_x, _y, (ConsoleColor)(color%15)+1);
        }
    }
}
