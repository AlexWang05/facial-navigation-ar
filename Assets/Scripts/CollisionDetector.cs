using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionDetector : MonoBehaviour
{
    public int trashcount = 0;
    public Text trashScore;
    private Timer theTimer;
    // Start is called before the first frame update
    void Start()
    {
        theTimer = GameObject.Find("Canvas").GetComponent<Timer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Displays score on UI

        if (theTimer.timeOut == false)
        {
            trashScore.text = "Score: " + trashcount.ToString() + "/" + theTimer.trashGoal.ToString();
            // Debug.Log(trashcount);
        }
        
    }
    // Upon collision the trash is destroyed and 1 point is added
    private void OnTriggerEnter(Collider other)
    {
        
        Destroy(other.gameObject);
        trashcount++;
        
    }

}   
