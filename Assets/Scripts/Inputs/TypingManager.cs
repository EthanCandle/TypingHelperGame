using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingManager : MonoBehaviour
{
    public string wordCurrent;
    public WordBank wordBankCurrent;
    public List<WordBank> wordBanks; 
    public InputAcceptor inputAcceptorScript;

    private void Awake()
    {
        wordBankCurrent = wordBanks[0];
    }
    // Start is called before the first frame update
    void Start()
    {
        GameManager.current.OnCorrectWord += MakeNewTypingWord; // makes it so the Gamemanager will call the function when the InputAcceptor gets the right word
        print("Added typing manager");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeNewTypingWord(int uselessIntHodler)
    {
        print("In make typing manager");
        GetNewWordFromBank();
        SetNewWord();
        EnableInput();
    }

    public void GetNewWordFromBank()
    {
        wordCurrent = wordBankCurrent.words[Random.Range(0, wordBankCurrent.words.Count)];
    }

    public void SetNewWord()
    {
        print("Set new word from tpying typing manager");
        inputAcceptorScript.SetTargetWord(wordCurrent);
    }

    public void CallAutoComplete()
    {
        inputAcceptorScript.CallAutoComplete();
    }

    public void DisableInput()
    {
        inputAcceptorScript.DeactivateInput();
    }

    public void EnableInput()
    {
        print("Enable input");
        inputAcceptorScript.ActivateInput();
    }

}
