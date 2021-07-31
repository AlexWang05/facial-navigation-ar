using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timePassed = 0;
    public bool timerIsRunning = false;
    public Text timeText;
    public Text timeOver;
    public int trashGoal;
    public bool timeOut = false;
    private CollisionDetector collisionDetector;
    public Transform playerCamera;
    public GameObject playSpace;
    private void Start()
    {
        // Starts the timer automatically
        timerIsRunning = true;
        timeOut = false;
        playerCamera = playSpace.gameObject.transform.GetChild(0);
        collisionDetector = playerCamera.GetComponent<CollisionDetector>();
    }

    void LateUpdate()
    {
        if (timerIsRunning)
        {
            if (collisionDetector.trashcount < trashGoal)
            {
                timePassed += Time.deltaTime;
                float seconds = Mathf.FloorToInt(timePassed);
                timeText.text = "Time: " + seconds.ToString();
                timeOut = false;
            }
            else
            {
                float seconds = Mathf.FloorToInt(timePassed);
                timeOver.text = string.Format("You Collected All The Orbs In " + seconds.ToString() + " Seconds!");
                //timeText.text = "";
                timePassed = 0;
                timerIsRunning = false;
                timeOut = true;
            }
        }
    }
}
