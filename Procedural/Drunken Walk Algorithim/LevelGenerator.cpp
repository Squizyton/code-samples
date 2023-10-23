#include "LevelGenerator.h"

#include <algorithm>
#include <cmath>
#include <ctime>
#include <future>
#include <iostream>
#include <string>
#include <vector>


typedef std::vector<std::vector<LevelGenerator::GridSpace>> Matrix;
typedef std::vector<LevelGenerator::GridSpace> Row;

bool printMap = true;
int totalNumberOfWalkersSpawned;




LevelGenerator::LevelGenerator(float worldUnits)
{
    worldUnitsInOneGridCell = worldUnits;
}


int GetNumberOfFloors(const std::vector<std::vector<LevelGenerator::GridSpace>>& gridMatrix, int roomWidth,
                      int roomHeight)
{
    int numberOfFloors = 0;

    for (int x = 0; x < roomWidth; x++)
    {
        for (int y = 0; y < roomHeight; y++)
        {
            if (gridMatrix[x][y] == LevelGenerator::ground)
            {
                numberOfFloors++;
            }
        }
    }

    return numberOfFloors;
}



Matrix LevelGenerator::Setup(const int room_width, const int room_height)
{
    //set the room wid
    this->roomWidth = static_cast<int>(std::round(static_cast<float>(room_width) / worldUnitsInOneGridCell));
    this->roomHeight = static_cast<int>(std::round(static_cast<float>(room_height) / worldUnitsInOneGridCell));


    Matrix gridMatrix;

    for (int x = 0; x < roomWidth; x++)
    {
        Row row(roomHeight);

        for (size_t y = 0; y < roomHeight; y++)
        {
            row[y] = static_cast<GridSpace>(1);
        }

        gridMatrix.push_back(row);
    }

    //create the first walker
    auto* walker = new class walker;
    walker->ChangeDirection(1);

    totalNumberOfWalkersSpawned++;

    walker->id = totalNumberOfWalkersSpawned;
    //Get center of the grid
    auto* spawnPos = new Vector2(round(roomWidth / 2.f), round(roomHeight / 2.f));
    walker->pos = *spawnPos;

    walkers.push_back(walker);

    return GenerateFloor(gridMatrix);
}


void LevelGenerator::CleanUp() const
{
    delete &walkers;
}



void LevelGenerator::SpawnNewWalker()
{
}


//TODO start converting things to async later on
Matrix LevelGenerator::GenerateFloor(std::vector<std::vector<GridSpace>>& grid_array)
{
    printf("Generating Floor");


    int iterations = 0;

    do
    {
        std::cout << "\nBeginning of Iteration";
        //for every walker create a floor tile under them
        std::cout << "\nCurrent Amount of Walkers spawned: " << walkers.size();
        for (const auto walker : walkers)
        {
            std::cout << "\nWalker: " << walker->id << "placed a floor at " << " x: " << walker->pos.x << ",y: " <<
                walker->pos.y;
            grid_array[static_cast<int>(walker->pos.x)][static_cast<int>(walker->pos.y)] = static_cast<GridSpace>(2);
        }
        //See if we are destroying a walker
        int numberChecks = walkers.size();
        for (int x = 0; x < numberChecks; x++)
        {
            if (walkers[x]->WillIGetDestroyed(chanceWalkerDestroy) && walkers.size() > 1)
            {
                std::cout << "\nDestroyed" << walkers[x]->id;
                //remove walker then break so we only remove one
                Utils::RemoveAt(walkers,x);
                std::ranges::sort(walkers);
                walkers.erase(std::ranges::unique(walkers).begin(), walkers.end());

                

                break;
            }
        }
        std::cout << "\nCurrent Amount of Walkers spawned after destruction: " << walkers.size();
        //Change Direction
        for (const auto walker : walkers)
        {
            walker->ChangeDirection(chanceWalkerChangeDir);
        }


        //Spawn new walker
        numberChecks = walkers.size();
        for (int i = 0; i < numberChecks; i++)
        {
            if (Utils::intRand(1, 10) < static_cast<int>(chanceWalkerChangeDir * 10) && numberChecks < maxWalkers)
            {
                auto walker = new class walker;
                walker->ChangeDirection(1);
                walker->pos = walkers[i]->pos;
                walker->parentWalker = i;
                walker->id = Utils::intRand(0, 21648744);
                walkers.emplace_back(walker);
                break;
            }
        }
        //move the walkers
        for (int i = 0; i < numberChecks; i++)
        {
            walkers[i]->MoveWalker();
            std::cout << "\nIndex: " << i << "Walker: " << walkers[i]->id << " Moved to x: " << walkers[i]->pos.x <<
                ",y: " << walkers[i]->pos.y;
        }
        std::cout << "\n-------------------------------------------";


        //check boundries
        for (int i = 0; i < numberChecks; i++)
        {
            walker thisWalker = *walkers[i];

            thisWalker.pos.x = std::clamp(thisWalker.pos.x, 1.f, static_cast<float>(roomWidth - 2));
            thisWalker.pos.y = std::clamp(thisWalker.pos.y, 1.f, static_cast<float>(roomWidth - 2));


            walkers[i] = &thisWalker;
        }


        //printf("Math:%f", static_cast<float>(GetNumberOfFloors(gridMatrix, roomWidth, roomHeight)) / static_cast<float>(roomWidth * roomHeight));
        if (static_cast<float>(GetNumberOfFloors(grid_array, roomWidth, roomHeight)) / static_cast<float>(roomWidth *
            roomHeight) > percentToFill)
        {
            printf("Percentage Filled!");
            //iterations = 100000001;
            break;
        }


        iterations++;
        // printf("%d",iterations);
    }
    while (iterations < 100000000);

    printf("done");
    return GenerateWalls(grid_array);
}

Matrix LevelGenerator::GenerateWalls(Matrix& grid_array)
{
    //This is stupid messy.
    for (int x = 0; x < roomWidth - 1; x++)
        for (int y = 0; y < roomHeight - 1; y++)
        {
            //if Theres a floor, check the spaces around it
            if (grid_array[x][y] == static_cast<GridSpace>(2))
            {
                //Bottom Wall
                if (grid_array[x][y + 1] == static_cast<GridSpace>(1))
                    grid_array[x][y + 1] = static_cast<GridSpace>(3);

                //Top Wall
                if (grid_array[x][y - 1] == static_cast<GridSpace>(1))
                    grid_array[x][y - 1] = static_cast<GridSpace>(3);

                //Right Wall
                if (grid_array[x + 1][y] == static_cast<GridSpace>(1))
                    grid_array[x + 1][y] = static_cast<GridSpace>(3);

                //Left Wall
                if (grid_array[x - 1][y] == static_cast<GridSpace>(1))
                    grid_array[x - 1][y] = static_cast<GridSpace>(3);
            }
        }


    if (printMap)
    {
        std::cout << "\n";
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomWidth; y++)
            {
                printf("%d ", grid_array[x][y]);
            }
            printf("\n");
        }
    }

    CleanUp();
    return grid_array;
}

