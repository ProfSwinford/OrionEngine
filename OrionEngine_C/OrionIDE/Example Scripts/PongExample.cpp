#include "PongExample.h"

#include <cstdlib>

void PongExample::Awake()
{
    const PixelGrid pongArt = {
        {Pixel('D'), Pixel('V'), Pixel('D')}
    };

    pongObj = new GameObject2D(3, 1, pongArt);
    _x = 0;
    _y = 0;
    _xVel = 1;
    _yVel = 1;
}

void PongExample::Update()
{
    if (Input::GetKey(Keycode::Space))
    {
        return;
    }

    if (Input::GetKeyDown(Keycode::Escape))
    {
        std::exit(0);  // Environment.Exit(0)
    }

    ModifiedPong();
}

void PongExample::ModifiedPong()
{
    color += 1;
    pongObj->transform()->position.x += _xVel;
    pongObj->transform()->position.y += _yVel;

    if (pongObj->transform()->position.x < 1)
    {
        pongObj->transform()->position.x = 1;
        _xVel = 1;
    }
    else if (pongObj->transform()->position.x + pongObj->width + 1 >= Camera::canvas().Width())
    {
        _x = Camera::canvas().Width() - (pongObj->width + 1);
        _xVel = -1;
    }

    if (pongObj->transform()->position.y < 1)
    {
        pongObj->transform()->position.y = 1;
        _yVel = 1;
    }
    else if (pongObj->transform()->position.y + pongObj->height + 1 >= Camera::canvas().Height())
    {
        pongObj->transform()->position.y = Camera::canvas().Height() - (pongObj->height + 1);
        _yVel = -1;
    }
}

void PongExample::OriginalPong()
{
    color += 1;
    _x += _xVel;
    _y += _yVel;

    if (_x < 1)
    {
        _x = 1;
        _xVel = 1;
    }
    else if (_x + 1 >= Camera::canvas().Width())
    {
        _x = Camera::canvas().Width() - 2;
        _xVel = -1;
    }

    if (_y < 1)
    {
        _y = 1;
        _yVel = 1;
    }
    else if (_y + 1 >= Camera::canvas().Height())
    {
        _y = Camera::canvas().Height() - 2;
        _yVel = -1;
    }
}

void PongExample::LateUpdate()
{
    // Print("Ball Position: (" + std::to_string(_x) + ", " + std::to_string(_y) + ")");
    // ConsoleRendererModule::canvas().Set(_x, _y, (ConsoleColor)(color % 15) + 1);
}
