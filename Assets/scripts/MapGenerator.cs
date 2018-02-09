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
            // Smooth the walls of each region
            SmoothMap();
        }

        // Remove the tiny regions
        SmoothAllRegions(50);

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

    // Smooth the map by replacing each tile with what the surr majority is
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
                if (InBoundary(neighbourX, neighbourY))
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

    // ------------------------------------------ Region Detection

    // Store current map position (tile/square)
    struct Tile
    {
        public int tileX;
        public int tileY;

        public Tile(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    void SmoothAllRegions(int threshold)
    {
        SmoothRegion(threshold, 1);
        SmoothRegion(threshold, 0);
    }

    void SmoothRegion(int threshold, int topography)
    {
        int replaceTopography = 0;
        if(topography == 0)
            replaceTopography = 1;

        // Get all of the regions given the topography
        List<List<Tile>> regions = FindRegions(topography);

        foreach (List<Tile> tile in regions)
        {
            // Any region that is made up of less than threshold tiles is replaced
            if (tile.Count < threshold)
            {
                foreach (Tile adjacent in tile)
                {
                    map[adjacent.tileX, adjacent.tileY] = replaceTopography;
                }
            }
        }
    }



    // Given the type, find all regions (eg 1 will find all wall regions)
    // Returns a list of regions. Each region is a list of tiles
    List<List<Tile>>FindRegions(int topography)
    {
        List<List<Tile>> regions = new List<List<Tile>>();

        // store visited tiles
        int[,] visitMap = new int[width, height];

        // go through the map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // if not visited and if the same type
                if (visitMap[x, y] == 0 && map[x, y] == topography)
                {
                    // create a new region
                    List<Tile> region = SeedFill(x, y);

                    // add the new region
                    regions.Add(region);

                    // mark each tile in the new region as visited
                    foreach (Tile tile in region)
                    {
                        visitMap[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    // Flood fill method to detect regions
    List<Tile> SeedFill(int startX, int startY)
    {
        // store map tiles
        List<Tile> tiles = new List<Tile>();

        // new map to store what tiles have been seen (visited) 1 = visited
        int[,] visitMap = new int[width, height];

        // the type fo the current tile (wall or clear)
        int topography = map[startX, startY];

        // use this to process all the adjacent tiles to starting tile
        Queue<Tile> queue = new Queue<Tile>();

        // visit the first tile
        queue.Enqueue(new Tile(startX, startY));
        visitMap[startX, startY] = 1;

        while (queue.Count > 0)
        {
            // current tile
            Tile tile = queue.Dequeue();
            tiles.Add(tile);

            // visit all of the adjacent tiles
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    // if the adjacent tile is within the map boundary and not a diagonal tile (so if x or y are at the centre)
                    if (InBoundary(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        // if not visited and is the same tile type (either a wall or clear space)
                        if (visitMap[x, y] == 0 && map[x, y] == topography)
                        {
                            // visited
                            visitMap[x, y] = 1;

                            // add the new tile
                            queue.Enqueue(new Tile(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool InBoundary(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
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
