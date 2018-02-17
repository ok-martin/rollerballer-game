using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;

    public Text countText;
    public Text winText;

    public Material playerMaterial;

    private Rigidbody ballBody;
    private GameObject[] pickups;
    private int count;

    private int[,] map;

    private Color32[] playerColours =
    {
        new Color32(255, 228, 225, 1),
        new Color32(230, 230, 250, 1),
        new Color32(240, 255, 255, 1),
        new Color32(131, 139, 131, 1),
        new Color32(255, 250, 205, 1),
        new Color32(255, 228, 181, 1),
        new Color32(255, 218, 185, 1),
        new Color32(255, 228, 196, 1),
        new Color32(245, 245, 245, 1),
        new Color32(205, 201, 201, 1)
    };

    // Initialization
    void Start ()
    {
        // Use rigidbody for ball physics
        ballBody = GetComponent<Rigidbody>();

        // Get all of the pickups in the scene
        pickups = GameObject.FindGameObjectsWithTag("Pickup");

        // Get the number of pickups
        count = pickups.Length;
        SetCountText();
        winText.text = "";

    }
	
	// Update is called once per frame (before rendering a frame)
	void Update () { }

    // Called just before any physics calcs
    void FixedUpdate()
    {
        // Get current position x and y
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        // Create the x,y vector
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Push the ball
        ballBody.AddForce(movement * speed);
    }

    // When player object first touches an object collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if it is the pickup object.
        // Also: Add rigidbody to pickup object. Make it Kinematic.
        if (other.gameObject.CompareTag("Pickup"))
        {
            // Hide the pickup as opposed to Destroy(other.gameObject);
            other.gameObject.SetActive(false);

            // Update the count
            count--;
            SetCountText();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(playerMaterial.color.ToString());

        //Renderer rend = GetComponent<Renderer>(); rend.material.color
        playerMaterial.color = RandomPlayerColor();

        // Debug-draw all contact points and normals
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }

        // big impact.
        //if (collision.relativeVelocity.magnitude > 2)
    }


    private Color32 RandomPlayerColor()
    {
        int colours = playerColours.Length;
        int colour = (int)Random.Range(0F, colours);
        return playerColours[colour];
    }

    // Update the text UI
    private void SetCountText()
    {
        countText.text = "Collect: " + count.ToString();
        if (count == 0)
        {
            winText.text = "You Win!";
        }
    }

}
