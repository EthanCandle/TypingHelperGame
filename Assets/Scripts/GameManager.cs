using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameManager : MonoBehaviour
{

    public int columnOn = 0, columnTotal = 3;

    public static GameManager current;
    // need to make this script listen to the InputAcceptor and see when it
    // gets the correct word in order to call it


    // public GameManager gm; gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    public int scoreCurrent = 0;
    public EnemySpawner enemySpawnerScript;

    private bool waitFrame = true;
    void Awake()
    {
        current = this;
    }

    void Start()
    {
        InputAcceptor.current.GotCorrectWord += CorrectWord; // makes it so the Gamemanager will call the function when the InputAcceptor gets the right word
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            MoveIncrementColumnOn();
        }
    }

    public int GetColumnOn()
    {
        return columnOn;
    }

    public void MoveIncrementColumnOn()
    {
        columnOn++;

        if(columnOn > columnTotal - 1)
        {
            columnOn = 0;
        }
        enemySpawnerScript.GetAndSetClosestEnemy();

    }


    public event Action<int> OnCorrectWord;
    public void CorrectWord()
    {
        // this is the function that gets called when the player types the correct word, needs to subscribe to the inputtyper
        // this will call every script that subscribed to this function (UI mainly)
        scoreCurrent += 1;
        
        if (OnCorrectWord != null)
        {
            OnCorrectWord(scoreCurrent);
        }
    }

    private void OnDestroy()
    {
        OnCorrectWord = null; // removes all listeners
        InputAcceptor.current.GotCorrectWord -= CorrectWord; // makes it so the Gamemanager will call the function when the InputAcceptor gets the right word
    }

}
