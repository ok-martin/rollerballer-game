using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // The reference to the ball
    public GameObject player;

    private Vector3 offset;

    // Initialization
    void Start ()
    {
        // Place this behind the player
        offset = transform.position - player.transform.position;
    }

    // For last known states. Run after all items have been processed + updated
    void LateUpdate()
    {
        // Update the camera after the player has moved
        transform.position = player.transform.position + offset;
    }
}
