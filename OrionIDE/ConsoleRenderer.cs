//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Runtime.InteropServices;
//using System.Security.Policy;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Net.Mime.MediaTypeNames;

//namespace ConsoleRenderer
//{
//    public class ConsoleRenderer //Idea 1
//    {
//        Graphics buffer;//Used to render 
//        Bitmap img;//Render object that will be visible

//        private Size ConsoleFontSize => new Size(8, 16); // approximation
//        public ConsoleRenderer(int width, int height)
//        {
//            img = new Bitmap(width, height);
//            buffer = Graphics.FromImage(img);
//        }

//        public void DrawImage(Image toDraw, Point location)
//        {
//            Size fontSize = ConsoleFontSize;//Get the font sie to calculate accurate image bounds
//            Rectangle imgRect = new Rectangle(//Calculate image bounds
//            location.X * fontSize.Width,
//            location.Y * fontSize.Height,
//            img.Width * fontSize.Width,
//            img.Height * fontSize.Height);
//            buffer.DrawImage(img, imgRect);//Use the 'buffer' Graphic instance to render the image to the Bitmap 'img'
//        }

//        public void Render()
//        {
//            Graphics render = Graphics.FromHwnd(Process.GetCurrentProcess().MainWindowHandle);
//            //Grab the Console handle for drawing
//            render.DrawImage(img, new Point(0,0));//draw rendered image to Console
//            buffer.Clear(Color.Gray); //use 'buffer' to wipe the rendered image with a background color(I used gray to have contrast to the Console background)
//        }

        
//    }

//    class ASCIIRenderer //Idea 2
//    {
//        // https://www.codeproject.com/articles/Generate-ASCII-Art-A-Simple-How-To-in-Csharp#comments-section
//    }
//}
