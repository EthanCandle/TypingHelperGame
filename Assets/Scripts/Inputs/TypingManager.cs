using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingManager : MonoBehaviour
{
    // attach this to the gameManager object

    public List<WordBank> wordBanks; 


    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        //GameManager.current.OnCorrectWord += MakeNewTypingWord; // makes it so the Gamemanager will call the function when the InputAcceptor gets the right word

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public string GetNewWordFromBank(int bankToGetFrom)
    {
        // when called, it returns a random string from the word bank's word list
        if(bankToGetFrom >= wordBanks.Count)
        {
            Debug.LogError($"Bank doesn't go that high: Tried {bankToGetFrom} when we go to {wordBanks.Count}");
            return "ERROR";
        }
        return wordBanks[bankToGetFrom].words[Random.Range(0, wordBanks[bankToGetFrom].words.Count)];
    }



}
