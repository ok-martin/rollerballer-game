using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Map size
    public int width;
    public int height;

    public string seed = "Bob";
    public bool randomSeed;

    // How much of the map should be filled (the walls)
    [Range(0, 100)]
    public int fillPercentage;

    // Binary array for the map (0 clear, 1 wall)
    int[,] map;

    // Initialization
    void Start()
    {
        GenerateMap();
	}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new int[width, height];

        // Create the map
        FillMap();

        // Smooth the map 5 times
        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        float size = 1;
        meshGen.GenerateMesh(map, size);
    }

    // Create the map based on the seed. If feed the same seed, it will generate the same map
    void FillMap()
    {
        if (randomSeed)
        {
            seed = Time.time.ToString();
        }

        // Unique hashcode for the seed
        System.Random random = new System.Random(seed.GetHashCode());

        // Fill the map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // set the borders of the map to be walls
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    // if random is less than the percentage fill then it is a wall, otherwise it's clear
                    // the higher the percentage fill the more
                    map[x, y] = (random.Next(0, 100) < fillPercentage) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 1; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                // if majority is walls
                if (GetSurroundingWalls(x, y) > 4)
                {
                    // make it a wall too
                    map[x, y] = 1;
                }
                else if(GetSurroundingWalls(x, y) < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWalls(int gridX, int gridY)
    {
        int walls = 0;

        // from -1 to +1 from here (around)
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    // Ignore the current x, y
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        walls += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    walls++;
                }
            }
        }

        return walls;
    }

    void OnDrawGizmos()
    {
         /*if (map != null)
         {
             for (int x = 0; x < width; x++)
             {
                 for (int y = 0; y < height; y++)
                 {
                     Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                     Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                     Gizmos.DrawCube(pos, Vector3.one);
                 }
             }
         }*/
    }

}
