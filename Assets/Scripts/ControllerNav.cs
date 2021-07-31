using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerNav : MonoBehaviour
{
    // ControllerInput controls;
    public GameObject player;
    // Vector2 move;
    private float speed = 1.0f;
    private float turnSpeed = 2.0f;

    public float Vertical;
    public float Horizontal;

    public float Turn;

    // Start is called before the first frame update
    void Start() {
        // controls = new ControllerInput();

        // controls.Player.TurnRight.performed += ctx => TurnRight();
        // controls.Player.TurnLeft.performed += ctx => TurnLeft();
        // controls.Player.ForwardBackward.performed += ctx => move = ctx.ReadValue<Vector2>();
        // controls.Player.ForwardBackward.canceled += ctx => move = Vector2.zero;
    }

    void Update() {
        Vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        Horizontal = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        Turn = Input.GetAxis("AXIS_4") * turnSpeed * Time.deltaTime;

        player.transform.Translate(Vector3.forward * Vertical);
        player.transform.Translate(Vector3.right * Horizontal);
        player.transform.Rotate(Vector3.up, Turn);
    }
}
