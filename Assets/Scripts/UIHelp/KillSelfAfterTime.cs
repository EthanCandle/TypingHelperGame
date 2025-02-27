using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillSelfAfterTime : MonoBehaviour
{
    public float deathTimer = 5; // time until it dies
    public float speed = 1; // amount per second it dies

    public void Start()
    {
        if (PlayerPrefs.GetInt("ShowHitBoxes", 0) == 1)
        {
            GetComponent<MeshRenderer>().enabled = true;

        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;

        }
    }
    // Update is called once per frame
    void Update()
    {
        if(deathTimer > 0)
        {
            deathTimer -= speed * Time.deltaTime;
        }
        else
        {
            HasDied();
        }
    }

    public void SetDeathTimer(float timeToLive)
    {
        deathTimer = timeToLive;
    }
    public void HasDied()
    {
        // might need to make it fade instead
        Destroy(gameObject);
    }
}
