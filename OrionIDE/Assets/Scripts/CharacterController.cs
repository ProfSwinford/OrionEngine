using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrionEngine;

public class CharacterController : OrionBehaviour
{
    GameObject2D character;

    int floor = 0;
    
    public void Start()
    {
        floor = (Camera.canvas.Height / 3) * 2;
        //https://theasciicode.com.ar/
        Pixel[,] sprite = new Pixel[,]
        {
            {   new Pixel(' '), new Pixel('o'), new Pixel(' ') },
            {   new Pixel('-'), new Pixel('|'), new Pixel('-') },
            {   new Pixel('/'), new Pixel(' '),  new Pixel('\\')},
        };


        character = new GameObject2D(3,3, sprite);
    }

    public void Update()
    {
        Vector2 temp = Vector2.zero;

        if (Input.GetKey(Keycode.W)) 
        {
            //character.transform.position += Vector2.up;
            temp += Vector2.up;
        }
        if (Input.GetKey(Keycode.S))
        {
            //character.transform.position += -Vector2.up;
            temp += -Vector2.up;
        }
        if (Input.GetKey(Keycode.A))
        {
            //character.transform.position += -Vector2.right;
            temp += -Vector2.right;

        }
        if (Input.GetKey(Keycode.D))
        {
            //character.transform.position += Vector2.right;
            temp += Vector2.right;
        }

        if (character.transform.position.y + character.height + temp.y > floor)
            temp.y = 0;
        
        character.transform.position += temp;
    }
}

