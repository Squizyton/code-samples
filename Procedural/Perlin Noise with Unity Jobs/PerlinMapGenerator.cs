using System.Collections.Generic;
using System.Linq;
using Jobs;
using Sirenix.OdinInspector;
using TileVarients;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = Unity.Mathematics.Random;
using SysRandom = System.Random;
namespace MapGenerator
{
    public class PerlinMapGenerator : SerializedMonoBehaviour
    {
        
        private bool useGameObjects;

        
        private bool useTileMap;

      
        private TileType[] _tiles;

   
        private TileTypeSprite[] _spriteTiles;


        private Dictionary<string, GameObject> _tileGroups;

        [SerializeField] private Tilemap tilemap;


        [Header("Generator Settings")] [SerializeField]
        private int mapWidth;

        [SerializeField] private int mapHeight;

        [SerializeField, Range(0, 1)] private float frequency;

        //TODO: Re-explain what this does. I dont remember
        public float noiseScale;
        public int octaves;
        [Range(0, 1)] public float persistance;
        public float lacunarity;

        [Header("Falloff map")] public bool useFallOffMap;
        private NativeArray<float> _falloffMap;


        [Space(10), Header("Misc")] [SerializeField]
        private bool randomizeSeed;

        [SerializeField]private List<GameObject> environments;
        
        
        [EnableIf("@!randomizeSeed")] public uint seed;
        public Vector2 offset;

        [Header("Job/Burst Compiler Related Variables")] private NativeArray<float> _tilesPerlined;
        private JobHandle _perlinNoiseMap;
        private GeneratePerlinNoiseMap _generatePerlinNoiseMap;
        private SmoothNoiseMap _smoothNoiseMap;
        private FalloffGenerator _falloffGenerator;
        private bool generatingMap;
        private NativeArray<float2> octaveOffsets;
        private List<GameObject> spawnedTiles;

        [Title("Misc Debug Variables")] [SerializeField, Sirenix.OdinInspector.ReadOnly]
        private float timeOfGeneration;

        private bool generating;
        private int _totalSpawnedTiles = 0;
        [SerializeField] private bool showGenerationTimeDebug;
        
        void Start()
        {
            spawnedTiles = new List<GameObject>();

            _falloffMap = new NativeArray<float>(mapWidth * mapHeight, Allocator.Persistent);


            CreateTileGroups();
        }
        
        void Update()
        {
            if (generatingMap)
                timeOfGeneration += Time.time;
        }

        void CreateTileGroups()
        {
            _tileGroups = new Dictionary<string, GameObject>();

            foreach (var prefabPair in _tiles)
            {
                var tileGroup = new GameObject(prefabPair.Name)
                {
                    transform =
                    {
                        parent = gameObject.transform,
                        localPosition = new Vector3(0, 0, 0)
                    }
                };

                _tileGroups.Add(prefabPair.Name, tileGroup);
            }
        }

        [Button("Generate Map")]
        void GenerateMap()
        {
            generatingMap = true;
            if (randomizeSeed)
                seed = (uint) UnityEngine.Random.Range(0, int.MaxValue);

            _tilesPerlined = new NativeArray<float>(mapWidth * mapHeight, Allocator.Persistent);

            DestroyAllTiles();

            generatingMap = true;


            System.Random prng = new System.Random((int) seed);


            octaveOffsets = new NativeArray<float2>(octaves, Allocator.Persistent);

            for (int i = 0; i < octaves; i++)
            {
                var offsetX = prng.Next(-100000, 100000) + offset.x;
                var offsetY = prng.Next(-100000, 100000) + offset.y;


                octaveOffsets[i] = new float2(offsetX, offsetY);
            }

            _generatePerlinNoiseMap = new GeneratePerlinNoiseMap()
            {
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                _tilesPerlined = _tilesPerlined,
                NoiseScale = noiseScale,
                Octaves = octaves,
                Persistance = persistance,
                Lacunarity = lacunarity,
                OctaveOffsets = octaveOffsets,
                //amplitude = 1,
                Frequency = frequency,
                //noiseHeight = 0f
            };

            _perlinNoiseMap = _generatePerlinNoiseMap.Schedule(mapHeight * mapWidth, 256);

            _perlinNoiseMap.Complete();

            _smoothNoiseMap = new SmoothNoiseMap()
            {
                Dimensions = new int2(mapWidth, mapHeight),
                result = _tilesPerlined
            };

            _perlinNoiseMap = _smoothNoiseMap.Schedule();
            _perlinNoiseMap.Complete();

            if (useFallOffMap)
            {
                Debug.Log("using falloff map");
                _falloffGenerator = new FalloffGenerator()
                {
                    FalloffMap = _falloffMap,
                    MapSize = (uint) mapHeight
                };

                _perlinNoiseMap = _falloffGenerator.Schedule();


                _perlinNoiseMap.Complete();
            }

            GenerateTiles();
        }


        void GenerateTiles()
        {
            var theArray = Make2DArray(_tilesPerlined, mapHeight, mapWidth);

            if (useFallOffMap)
            {
                ConvertWithFalloffMap(theArray);
            }
            else
            {
                if (useGameObjects)
                {
                    GenerateUsingGo(theArray);
                }
                else if (useTileMap)
                {
                    GenerateUsingTileMap(theArray);
                }
            }

            Debug.Log($"Spawned {_totalSpawnedTiles} out of {mapHeight * mapWidth}");


            if (showGenerationTimeDebug)
            {
                Debug.Log($"It took:" + (double) timeOfGeneration + "seconds");
            }

            _tilesPerlined.Dispose();
            octaveOffsets.Dispose();
        }

        private void ConvertWithFalloffMap(float[,] array)
        {
            var falloffArray = Make2DArray(_falloffMap, mapHeight, mapWidth);

            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    array[i, j] = Mathf.Clamp01(array[i, j] - falloffArray[i, j]);

                    // Debug.Log(array[i,j]);
                }
            }

            if (useGameObjects)
            {
                GenerateUsingGo(array);
            }
            else if (useTileMap)
            {
                GenerateUsingTileMap(array);
            }
        }


        private void GenerateUsingGo(float[,] theArray)
        {
            for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                var currentHeight = theArray[x, y];

                var didSpawnTile = false;
                for (var i = 0; i < _tiles.Length; i++)
                {
                    if (currentHeight < _tiles[i].Height)
                    {
                        var tileType = _tiles[i];

                        var prefab = tileType.Prefab;
                        var tileGroup = _tileGroups[tileType.Name];
                        var spawnedTile = Instantiate(prefab, tileGroup.transform);
                        spawnedTile.name = string.Format($"tile_{x}_{y}");
                        spawnedTile.transform.localPosition = new Vector3(x / 7f, y / 7f, 0);
                        spawnedTiles.Add(spawnedTile);
                        _totalSpawnedTiles++;
                        didSpawnTile = true;
                        break;
                    }
                }

                if (!didSpawnTile)
                {
                    Debug.Log($"Did not spawn tile at {currentHeight}");
                }
            }

            generatingMap = false;
        }

        private void GenerateUsingTileMap(float[,] theArray)
        {
            tilemap.ClearAllTiles();

            for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                var currentHeight = theArray[x, y];

                var didSpawnTile = false;
                for (var i = 0; i < _spriteTiles.Length; i++)
                {
                    if (currentHeight <= _spriteTiles[i].Height)
                    {
                        _totalSpawnedTiles++;
                        //Debug.Log(currentHeight);
                        var tileType = _spriteTiles[i];

                        //If There are variants available, Get a random one
                        if(tileType.Varient != null)
                            tileType.tile.sprite = GetSpriteVariant(tileType.Varient);
                        
                        //Debug.Log(tileType.Name);
                        //Debug.Log(tileType.tile);
                        tilemap.SetTile(new Vector3Int(x, y, 0), tileType.tile);
                        didSpawnTile = true;
                        break;
                    }
                }

                if (!didSpawnTile)
                {
                    Debug.LogError($"Did not spawn tile at {currentHeight}");
                }
            }

            GenerateEnvironment();
        }

        
        void GenerateEnvironment()
        {
            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(position) != _spriteTiles[2].tile) continue;
                var chance = UnityEngine.Random.Range(0f, 1);

                if(chance < .1f)
                    GameManager.instance.PlacePlayer(tilemap.GetCellCenterWorld(position));
                
                
                if (!(chance < 0.15f)) continue;
                Vector3 cellPosition = tilemap.GetCellCenterWorld(position);
                Instantiate(environments[UnityEngine.Random.Range(0, environments.Count)], cellPosition,
                    Quaternion.identity);
            }
            
            generatingMap = false;
        }

        private Sprite GetSpriteVariant(TileVarient varient)
        {
            float weightSum = 0;
            var weightIndex = new List<float>();
            var tempList = new List<TileVarient.Tile>(varient.variants);

            Shuffle(tempList);
            //Grab all the weights in the collection

            //Loop throught the collection of templates and grab all the weight
            foreach (var tile1 in tempList.Where(tile => tile.weight > 0))
            {
                weightSum += tile1.weight;
                weightIndex.Add(tile1.weight);
            }
            
            //Step through all the possibilities one by one checking to see if each one is selected
            int index = 0;
            int lastIndex = tempList.Count - 1;

            while (index < lastIndex)
            {
                //Do a probability check with a likelihood of weight
                if (UnityEngine.Random.Range(0, weightSum) < weightIndex[index])
                {
                    return tempList[index].sprite;
                }

                //Remove the last item from the sum of total untested weights and try again
                weightSum -= weightIndex[index++];
            }

            //If no other item was selected return the last one availible
            return tempList[index].sprite;
        }
        
        
       
        private static SysRandom rng = new SysRandom();
        /// <summary>
        /// Shuffles a List
        /// </summary>
        private static void Shuffle<T>(IList<T> list)
        {
            
            
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        
        /// <summary>
        /// Converts the NativeArray into a normal array before we cleanup NativeArray
        /// </summary>
        /// <param name="input"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private float[,] Make2DArray(NativeArray<float> input, int height, int width)
        {
            var arraySize = height * width;

            var baseArray = new float[input.Length];

            input.CopyTo(baseArray);

            var output = new float[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    output[i, j] = baseArray[i * width + j];
                }
            }

            return output;
        }


        private void LateUpdate()
        {
        }


        private void DestroyAllTiles()
        {
            //Not using foreach as thats less performant

            if (spawnedTiles.Count > 0) return;

            for (int x = 0; x < spawnedTiles.Count; x++)
                Destroy(spawnedTiles[x]);
        }

        
        
        
        
        private void OnDestroy()
        {
            _tilesPerlined.Dispose();
            octaveOffsets.Dispose();
            _falloffMap.Dispose();
        }

        [System.Serializable]
        public struct TileTypeSprite
        {
            public string Name;
            public float Height;
            public Tile tile;
            public TileVarient Varient;
        }

        [System.Serializable]
        public struct TileType
        {
            public string Name;
            public float Height;
            public GameObject Prefab;
        }
    }
}