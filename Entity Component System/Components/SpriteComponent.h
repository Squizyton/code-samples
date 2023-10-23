#pragma once

#include "Position_Component.h"
#include "SDL.h"
#include "../Base Scripts/TextureManager.h"


class SpriteComponent : public Component
{
private:
    Position_Component* position;
    SDL_Texture* texture;
    SDL_Rect srcRect, destRect;
    int width;
    int height;
    bool isAnimated;

public:
    /**
     * \brief 
     * \param textureSheet The sheet/sprite that we will be using
     * \param nwidth Width of the Sprite
     * \param nheight Height of the sprite
     * \param animated  is this Sprite animated
     */
    SpriteComponent(const char* textureSheet, float nwidth, float nheight, bool animated)
    {
        texture = TextureManager::LoadTexture(textureSheet);
        width = nwidth;
        height = nheight;
        isAnimated = animated;
    }


    void Init() override
    {
        position = &entity->GetComponent<Position_Component>();

        srcRect.x = srcRect.y = 0;
        srcRect.w = width;
        srcRect.h = height;

        destRect.w = width;
        destRect.h = height;
    }

    void Update() override
    {
        destRect.x = position->position->x;
        destRect.y = position->position->y;
    }

    void Draw() override
    {
        TextureManager::Draw(texture, srcRect, destRect);
    }
};
