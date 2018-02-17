using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject player;

    private int mapWidth = 0;
    private int mapHeight = 0;
    private float mapScale = 1;
    private int[,] map;

    // Use this for initialization
    void Start ()
    {
        // access the map generator
        MapGenerator mpg = GetComponent<MapGenerator>();

        // create the world
        mpg.Main();

        map = mpg.map;
        mapWidth = mpg.width;
        mapHeight = mpg.height;
        mapScale = mpg.scale;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // if first occurance of clear region
                if(map[x,y] == 0)
                {
                    // convert into world coordinates
                    float[] pos = Map2WorldCoordinates(x, y);

                    // move player to a starting location
                    player.transform.position = new Vector3(pos[0], 0F, pos[1]);
                    
                    // break
                    x = mapWidth;
                    break;
                }
            }
        }

        
    }
	
    /*  Given map position (x and y) convert into unity world
     *  coordinates / position assuming the world is build from origin.
     *  Include scale calculation.
     *  Map[x,y] starts at -x, -z location in Unity
     */
    private float[] Map2WorldCoordinates(int x, int y)
    {
        int halfx = mapWidth / 2 - 1;
        int halfy = mapHeight / 2 - 1;

        return new float[]
        {
            (x > halfx ? x - halfx : 0 - (halfx - x)) * mapScale,
            (y > halfy ? y - halfy : 0 - (halfy - y)) * mapScale
        };
    }


    // Update is called once per frame
    void Update ()
    {
		
	}
}
