using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // The reference to the ball
    public GameObject player;
    public Camera playerCamera;

    public Transform ballObject;
    public Transform ballCamera;
    public float speed;
    public float turnSpeed;
    private Vector3 forceDirection;

    public Vector3 offset;

    private Vector3 oldPos, newPos;
    private Vector3 oldCameraPos;

    public float alpha = 0.01f;
    public float alphaStep = 0.01f;
    bool farLeft = false;
    bool farRight = false;

    // Initialization
    void Start ()
    {
        // Place this behind the player
        //offset = transform.position - player.transform.position;
        

        alphaStep = 0.01f;

        alpha = 0f;

        offset = new Vector3(0, 2, -5);
    }


    void FixedUpdate()
    {
        // get the direction in which the force will be applied
        forceDirection = ballCamera.transform.forward;

        // remove y force direction (as the camera is at an angle)
        forceDirection = new Vector3(forceDirection.x, 0, forceDirection.z);

        // apply force in the direction that the camera is facing
        ballObject.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * speed * (Input.GetAxis("Vertical")));

        Vector3 pointOnScreen = playerCamera.WorldToScreenPoint(player.GetComponentInChildren<Renderer>().bounds.center);


        if (Input.GetKey(KeyCode.M))
        {
            ballObject.position = Vector3.zero;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            ballObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            ballObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) //  Input.GetAxis is negative
        {
            farRight = false;
            if (pointOnScreen.x >= Screen.width)
            {
                farLeft = true;
            }
            if(!farLeft)
            {
                Vector3 rotation = Vector3.up * Input.GetAxis("Horizontal") * turnSpeed;
                transform.Rotate(rotation, Space.World);
            }
            else
            {

                alpha += alphaStep;
                float radius = 5f;
                float x = radius * Mathf.Sin(alpha);
                float z = -radius * Mathf.Cos(alpha);
                offset = new Vector3(x, offset.y, z);

                transform.Rotate(0f, -alphaStep * Mathf.Rad2Deg, 0f, Space.World);

                

                
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            farLeft = false;
            if (pointOnScreen.x <= 0)
            {
                farRight = true;
            }
            if(!farRight)
            {
                Vector3 rotation = Vector3.up * Input.GetAxis("Horizontal") * turnSpeed;
                transform.Rotate(rotation, Space.World);
            }
            else
            {
                alpha -= alphaStep;
                float radius = 5f;
                float x = radius * Mathf.Sin(alpha);
                float z = -radius * Mathf.Cos(alpha);
                offset = new Vector3(x, offset.y, z);

                transform.Rotate(0f, alphaStep * Mathf.Rad2Deg, 0f, Space.World);
            }
        }
        else
        {
            // add force to ball
            
        }

        Debug.Log("alpha" + alpha + "powa " + Input.GetAxis("Horizontal"));



        /*
        else if (Input.GetKey(KeyCode.LeftArrow))
        {

             Rigidbody _Ball = ballObject.GetComponent<Rigidbody>();

            // COOOL CLIMB UP THE WALL
            //ballObject.GetComponent<Rigidbody>().AddForce(forceDirection.normalized*10);
            float _Radius = 2f;
            Vector3 v = _Ball.velocity;
            //Vector3 radius = playerCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, playerCamera.nearClipPlane));
            Vector3 radius = player.transform.position - playerCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, playerCamera.nearClipPlane));
            radius = new Vector3(2 * forceDirection.x, 0, 2 * forceDirection.z);

            Debug.Log("r: " + radius);

            // a vector perpendicular to both of those 
            // (a vector that would be an acceptable velocity for the ball sans the vertical component)
            Vector3 tangent = Vector3.Cross(Vector3.up, radius);

            Debug.Log("t: " + radius);

            // store the magnitude so we don't lose momentum when changing direction of speed
            float mag = _Ball.velocity.magnitude;

            // new speed is the old velocity projected on a plane defined by "up" and "tangent"
            Vector3 newVelo = (Vector3.Project(v, tangent) + Vector3.Project(v, Vector3.up)).normalized * 2;

            Debug.Log("nv: " + newVelo);

            _Ball.velocity = newVelo;

            // set the ball to the correct distance from the cylinder axis (assuming the vertical axis of cylinder is at X==0 && Z==0)
            radius.y = 0;
            radius = radius.normalized * _Radius;
            //radius.y = _Ball.transform.position.y;

            //_Ball.transform.position = radius;
    }*/



        /*
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Create the x,y vector
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Push the ball
        ballObject.GetComponent<Rigidbody>().AddForce(movement * speed);
        */
        transform.position = ballObject.position + offset;
    }

    // For last known states. Run after all items have been processed + updated
    void LateUpdate()
    {
        //transform.position = ballObject.position + offset;


        
    }


    private Vector3 CameraPosition()
    {
        newPos = ballObject.position;

        Vector3 finalCameraPos;

        float cameraOffset = 2;

        float deltaX = Mathf.Abs(newPos.x - oldPos.x);
        float deltaZ = Mathf.Abs(newPos.z - oldPos.z);

        float h = Mathf.Sqrt(Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaZ, 2)) - cameraOffset;

        if(deltaX != 0 && deltaZ !=0)
        {
            float alpha = Mathf.Atan(deltaZ / deltaX);
            float newDeltaX = h * Mathf.Cos(alpha);
            float newDeltaZ = h * Mathf.Sin(alpha);

            finalCameraPos = new Vector3(newPos.x - newDeltaX, -2F, newPos.z - newDeltaZ); 
        }
        else if(deltaX == 0)
        {
            finalCameraPos = new Vector3(oldCameraPos.x, -2F, newPos.z - cameraOffset);
        }
        else
        {
            finalCameraPos = new Vector3(newPos.x - 2, -2F, oldCameraPos.z);
        }

        oldCameraPos = finalCameraPos;
        oldPos = newPos;

        return finalCameraPos;
    }


    private bool IsInView(GameObject toCheck, Camera cam)
    {
        
        Vector3 pointOnScreen = cam.WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            Debug.Log("Behind: " + toCheck.name);
            return false;
        }

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            Debug.Log("OutOfBounds: " + toCheck.name);
            return false;
        }
        
        return true;
    }

}
