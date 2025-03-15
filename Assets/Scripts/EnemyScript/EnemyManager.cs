using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // controls the enemies scripts
    //
    public string targetWordCurrent;
    public int columnIn = 0;
    public EnemyMovement em;
    public TypingController typingControllerScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMovement()
    {

    }

    public void SetAsActiveTarget()
    {
        typingControllerScript.EnableInput();
    }

    public void SetAsDeactiveTarget()
    {
        typingControllerScript.DisableInput();
    }


    public void SetColumn(int columnToSetTo)
    {
        columnIn = columnToSetTo;
    }

    public void SetWord()
    {
        // called by the enemy spawner script
        typingControllerScript.MakeNewTypingWord(columnIn);
    }

}
