#pragma once
#include <vector>

#include "LevelGenerator.h"
#include "LevelGenerator.h"
#include "walker.h"
#include "Utilties/Utils.h"




class LevelGenerator
{
public:
    enum GridSpace { null, empty, ground, wall };

    LevelGenerator(float worldUnits = 1.f);
    std::vector<std::vector<GridSpace>> Setup(int room_width, int room_height);
    //What is this Spot in Grid classified as
private:
    float worldUnitsInOneGridCell = 1;
    int roomHeight, roomWidth;


    std::pmr::vector<walker*> walkers;

    //Generation Variables
    float chanceWalkerChangeDir = .5f;
    float chanceWalkerSpawn = 0.2f;
    float chanceWalkerDestroy = .5f;
    const int maxWalkers = 10;
    float percentToFill = .3f;


    //functions
    std::vector<std::vector<GridSpace>> GenerateFloor(std::vector<std::vector<GridSpace>>& grid_array);
    std::vector<std::vector<GridSpace>> GenerateWalls(std::vector<std::vector<GridSpace>> &grid_array);
    void SpawnNewWalker();
    void CleanUp() const;
};
typedef std::vector<std::vector<LevelGenerator::GridSpace>> Matrix;