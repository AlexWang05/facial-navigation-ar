using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject player;

    private float speed = 1.0f;
    private float turnSpeed = 1.0f;

    public float vertical;
    public float horizontal;
    public float turn;

    // Update is called once per frame
    void Update()
    {
        //Calculates character coordinates and how much to move in what direction
        vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        horizontal = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        turn = Input.GetAxis("AXIS_4") * turnSpeed * Time.deltaTime;

        //executes movements in the game
        player.transform.Translate(Vector3.forward * vertical);
        player.transform.Translate(Vector3.forward * horizontal);
        player.transform.Rotate(Vector3.up, turn);
    }
}
