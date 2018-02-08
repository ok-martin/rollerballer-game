using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;

    public Text countText;
    public Text winText;

    private Rigidbody ballBody;
    private GameObject[] pickups;
    private int count;

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
        if(other.gameObject.CompareTag("Pickup"))
        {
            // Hide the pickup as opposed to Destroy(other.gameObject);
            other.gameObject.SetActive(false);

            // Update the count
            count--;
            SetCountText();
        }
    }

    // Update the text UI
    void SetCountText()
    {
        countText.text = "Collect: " + count.ToString();
        if (count == 0)
        {
            winText.text = "You Win!";
        }
    }
}
