using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject player;

	// Use this for initialization
	void Start ()
    {
        // Access the map generator
        MapGenerator mpg = GetComponent<MapGenerator>();

        // Create the world
        mpg.Main();

        int[,] map = mpg.map;
        int width = mpg.width;
        int height = mpg.height;
        float scale = mpg.scale;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // if first occurance of clear region
                if(map[x,y] == 0)
                {
                    // convert into world coordinates
                    int halfx = width / 2 - 1;
                    int halfy = height / 2 - 1;
                    float px = x > halfx ? x - halfx : 0 - (halfx - x);
                    float py = y > halfy ? y - halfy : 0 - (halfy - y);

                    // Move player to a starting location
                    player.transform.position = new Vector3(px, 0F, py);
                    
                    // break
                    x = width;
                    break;
                }
            }
        }

        
    }
	
    

	// Update is called once per frame
	void Update ()
    {
		
	}
}
