using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameManager : MonoBehaviour
{
    public static GameManager current;
    // need to make this script listen to the InputAcceptor and see when it
    // gets the correct word in order to call it
    // Start is called before the first frame update
    // public GameManager gm; gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    public int scoreCurrent = 0;

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
        if (waitFrame)
        {
            waitFrame = false;
            if (OnCorrectWord != null)
            {
                foreach (var d in OnCorrectWord.GetInvocationList())
                {
                    Debug.Log($"Listener: {d.Method.Name} in {d.Target}");
                }
            }
            OnCorrectWord(scoreCurrent);
            print("waied frame");
        }
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
