#include "walker.h"

#include <cstdlib>
#include <ctime>
#include <random>
#include <thread>


Direction* direction = new Direction();




Vector2 RandomDirection()
{
    /* initialize random seed: */

    // printf("%d\n",choice);
    //use that int to chose a direction
    switch (int choice = Utils::intRand(0,3))
    {
    case 0:
        return *Direction::down;
    case 1:
        return *Direction::left;
    case 2:
        return *Direction::up;
    default:
        return *Direction::right;
    }
}

bool walker::WillIGetDestroyed(float chanceToBeKaboomed)
{
    //TODO:: Function not thread safe (working on it)
    int random = Utils::intRand(1,10);


    if (random < chanceToBeKaboomed * 10)
    {
        return true;
    }

    return false;
}

void walker::ChangeDirection(float chanceToChangeDir)
{
    const int random = Utils::intRand(1,10);


    if (random < chanceToChangeDir * 10)
    {
        dir = RandomDirection();
    }
}

void walker::MoveWalker()
{
    pos.x += dir.x;
    pos.y += dir.y;
}

