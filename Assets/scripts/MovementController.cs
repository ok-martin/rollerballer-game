using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Controls how the player and the camera move
 *  Player moves in relation to the cameras position & rotation
 */
public class MovementController : MonoBehaviour
{
    // The reference to the ball & camera
    public GameObject player;
    public Camera playerCamera;

    // Gameplay controls
    public float ballSpeed;
    public float cameraTurnSpeed;

    // Camera variables
    public Vector3 offset;
    public float alpha;
    public float alphaStep;

    bool farLeft = false;
    bool farRight = false;

    // Initialisation
    void Start ()
    {
        alpha = 0f;
        alphaStep = 0.01f;

        ballSpeed = 11f;
        cameraTurnSpeed = 1f;

        // Place the camera behind the player. transform.position - player.transform.position;
        offset = new Vector3(0, 3, -5);
    }


    void FixedUpdate()
    {
        // movement speed
        float finalSpeed = ballSpeed * Input.GetAxis("Vertical");

        // get the direction in which the force will be applied
        Vector3 forceDirection = playerCamera.transform.forward;

        // remove y force direction (as the camera is at an angle)
        forceDirection.y = 0f;

        Vector3 finalForce = forceDirection.normalized;

        // use this to detect if the ball is visible in the player camera
        Vector3 pointOnScreen = playerCamera.WorldToScreenPoint(player.GetComponentInChildren<Renderer>().bounds.center);
        
        // determine key
        if (Input.GetKey(KeyCode.M))
        {
            player.transform.position = Vector3.zero;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            player.transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            player.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) //  Input.GetAxis is negative
        {
            farRight = false;

            // turning speed
            finalSpeed = ballSpeed / 3;
            // diagonal (tangental, centripetal) force for circular motion
            finalForce = Vector3.Cross(finalForce, offset);
            // remove the height component
            finalForce.y = 0f;

            if (pointOnScreen.x >= Screen.width)
            {
                farLeft = true;
            }
            if(!farLeft)
            {
                Vector3 rotation = Vector3.up * Input.GetAxis("Horizontal") * cameraTurnSpeed;
                playerCamera.transform.Rotate(rotation, Space.World);
            }
            else
            {
                OrbitCamera(alphaStep * Input.GetAxis("Horizontal"));
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            farLeft = false;

            // turning speed
            finalSpeed = -ballSpeed / 3;
            // diagonal (tangental, centripetal) force for circular motion
            finalForce = Vector3.Cross(finalForce, offset);
            // remove the height component
            finalForce.y = 0f;

            if (pointOnScreen.x <= 0)
            {
                farRight = true;
            }
            if(!farRight)
            {
                Vector3 rotation = Vector3.up * Input.GetAxis("Horizontal") * cameraTurnSpeed;
                playerCamera.transform.Rotate(rotation, Space.World);
            }
            else
            {
                OrbitCamera(alphaStep * Input.GetAxis("Horizontal"));
            }
        }

        // apply the force to move the player
        player.transform.GetComponent<Rigidbody>().AddForce(finalForce * finalSpeed);
    }

    /*  For last known states.
     *  Runs after all items have been processed + updated
     */
    void LateUpdate()
    {
        // update the camera position
        playerCamera.transform.position = player.transform.position + offset;
    }

    /*  Adjust the camera as the player is making turns
     *  The camera orbits in a circluar motion around the player object
     */
    private void OrbitCamera(float step)
    {
        alpha -= step;
        float radius = 5f;
        float x = radius * Mathf.Sin(alpha);
        float z = -radius * Mathf.Cos(alpha);
        offset = new Vector3(x, offset.y, z);

        playerCamera.transform.Rotate(0f, step * Mathf.Rad2Deg, 0f, Space.World);
    }
}
