using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingController : MonoBehaviour
{
    // attached to enemy, should always be with the InputAcceptor

    public string targetWordCurrent;
    public InputAcceptor inputAcceptorScript;
    public TypingManager typingManagerScript;
    public CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Awake()
    {
        typingManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TypingManager>();
    
    }

    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void GotCorrectWord()
    {
        // call to enemy manager
    }

    public void MakeNewTypingWord(int wordBankToGet)
    {
        typingManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TypingManager>();

       // print("In make typing manager");
        targetWordCurrent = typingManagerScript.GetNewWordFromBank(wordBankToGet);
        SetNewWord(targetWordCurrent);
    }

    public void SetNewWord(string wordToSetTo)
    {
       // print("Set new word from tpying typing manager");
        inputAcceptorScript.SetTargetWord(wordToSetTo);
    }

    public void CallAutoComplete()
    {
        inputAcceptorScript.CallAutoComplete();
    }

    public void DisableInput()
    {
        inputAcceptorScript.DeactivateInput();
        AlphaDown();
    }

    public void EnableInput()
    {
        //print("Enable input");
        inputAcceptorScript.ActivateInput();
        AlphaUp();
    }

    public void AlphaUp()
    {
        canvasGroup.alpha = 1;
    }

    public void AlphaDown()
    {
        canvasGroup.alpha = 0.1f;
    }
}
