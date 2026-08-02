#include "CharacterController.h"

void CharacterController::Start()
{
    floor = (Camera::canvas().Height() / 3) * 2;

    // https://theasciicode.com.ar/
    const PixelGrid sprite = {
        {Pixel(' '), Pixel('o'), Pixel(' ')},
        {Pixel('-'), Pixel('|'), Pixel('-')},
        {Pixel('/'), Pixel(' '), Pixel('\\')},
    };

    character = new GameObject2D(3, 3, sprite);
}

void CharacterController::Update()
{
    Vector2 temp = Vector2::zero;

    if (Input::GetKey(Keycode::W))
    {
        // character->transform()->position += Vector2::up;
        temp += Vector2::up;
    }
    if (Input::GetKey(Keycode::S))
    {
        // character->transform()->position += -Vector2::up;
        temp += -Vector2::up;
    }
    if (Input::GetKey(Keycode::A))
    {
        // character->transform()->position += -Vector2::right;
        temp += -Vector2::right;
    }
    if (Input::GetKey(Keycode::D))
    {
        // character->transform()->position += Vector2::right;
        temp += Vector2::right;
    }

    if (character->transform()->position.y + character->height + temp.y > floor)
    {
        temp.y = 0;
    }

    character->transform()->position += temp;
}
