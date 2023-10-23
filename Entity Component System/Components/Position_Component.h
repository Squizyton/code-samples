#pragma once
#include "Components.h"
#include <iostream>
class Position_Component : public Component
{
public:
    Vector2* position;
    Position_Component()
    {
        position = new Vector2();
    }

    Position_Component(float x, float y)
    {
        position = new Vector2(x,y);
    }
    
private:
    void SetPos(float x, float y)
    {
        position->x = x;
        position->y = y;
    }

    void Update() override
    {
        //TODO:: Remove
        position->x++;
        position->y++;

    
    }
};
