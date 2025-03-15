using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // need a scriptable enemy object
    public float speed = 10f;
    public int healthCurrent = 100;
    public bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            MoveForward();
        }
    }
    
    public void AllowMovement()
    {
        SetCanMove(true);
    }

    public void RemoveMovement()
    {
        SetCanMove(false);
    }

    public void SetCanMove(bool newState)
    {
        canMove = true;
    }

    public void MoveForward()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }


}
