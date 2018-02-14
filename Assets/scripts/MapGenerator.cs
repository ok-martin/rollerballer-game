using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Map size
    public int width = 50;
    public int height = 50;
    public float scale = 1;

    // Map specs
    public string seed = "Bob";
    public bool randomSeed;

    // How much of the map should be filled (the walls)
    [Range(0, 100)]
    public int fillPercentage;

    // Binary array for the map (0 clear, 1 wall)
    int[,] map;

    // Object for the bottom / floor 
    public GameObject floor;
    public Material floorMaterial;

    // Initialization
    void Start()
    {
        
        GenerateMap();
        AddFloor();
    }

    void Update()
    {

    }

    void AddFloor()
    {
        // Convert tiles to Unity units
        float wx = width / 9 * scale;
        float wy = height / 9 * scale;

        // Set the position for the floor
        floor.transform.position = new Vector3(0, -4.9F, 0);

        // Scale the floor to the map size
        floor.transform.localScale = new Vector3(wx, 1F, wy);

        // Scale the floor texture to the floor size
        floorMaterial.mainTextureScale = new Vector2(wx, wy);
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

        // Region Detection: Remove tiny regions (below the treshold count of tiles)
        SmoothRegion(10, 1);

        // Region Connection
        List<Room> Rooms = SmoothRegion(50, 0);
        Rooms.Sort();
        Rooms[0].isMainRoom = true;
        Rooms[0].isAccessibleFromMainRoom = true;
        ConnectClosestRooms(Rooms);

        // Genereate mesh for the top and 3D walls
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        
        meshGen.GenerateMesh(map, scale);
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


    List<Room> SmoothRegion(int threshold, int topography)
    {
        int replaceTopography = 0;
        if(topography == 0)
            replaceTopography = 1;

        // Get all of the regions given the topography
        List<List<Tile>> regions = FindRegions(topography);

        List<Room> survivingRegions = new List<Room>();

        foreach (List<Tile> tiles in regions)
        {
            // Any region that is made up of less than threshold tiles is replaced
            if (tiles.Count < threshold)
            {
                foreach (Tile adjacent in tiles)
                {
                    map[adjacent.tileX, adjacent.tileY] = replaceTopography;
                }
            }
            else if (topography == 0)
            {
                survivingRegions.Add(new Room(tiles, map));
            }
        }
        return survivingRegions;
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

    // ------------------------------------------ Region Connection


    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Tile bestTileA = new Tile();
        Tile bestTileB = new Tile();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edges.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edges.Count; tileIndexB++)
                    {
                        Tile tileA = roomA.edges[tileIndexA];
                        Tile tileB = roomB.edges[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Tile tileA, Tile tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);

        List<Tile> line = GetLine(tileA, tileB);
        foreach (Tile c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(Tile c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (InBoundary(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }


    List<Tile> GetLine(Tile from, Tile to)
    {
        List<Tile> line = new List<Tile>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Tile(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Tile tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }


    class Room : IComparable<Room>
    {
        // This rooms tiles
        public List<Tile> tiles;

        // Which tiles are on the edge
        public List<Tile> edges;

        // What other rooms are connected to this room
        public List<Room> connectedRooms;

        // How many tiles in this room
        public int roomSize;

        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room() { }

        public Room(List<Tile> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;

            connectedRooms = new List<Room>();

            edges = new List<Tile>();

            // Go through each tile in the room
            foreach (Tile tile in tiles)
            {
                // Check if any of the neighbours are wall tiles to fiad room's edges
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        // Exclude diagonal neighbours (so just see 4 around)
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edges.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

}
