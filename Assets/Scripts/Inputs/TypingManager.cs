using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingManager : MonoBehaviour
{
    // public GameManager gm;
    public InputAcceptor inputAcceptorScript;
    // Start is called before the first frame update
    void Start()
    {
     //   gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CallAutoComplete()
    {
        inputAcceptorScript.CallAutoComplete();
    }

}
