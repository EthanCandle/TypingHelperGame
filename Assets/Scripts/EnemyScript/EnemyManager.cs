using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // controls the enemies scripts
    //
    public string targetWordCurrent;
    public int columnIn = 0;
    public int levelOfEnemy = 1, experienceWorth = 150;
    public EnemyMovement em;
    public TypingController typingControllerScript;
    public EnemySpawner enemySpawnerScript;
    public GameObject backgroundObject;
    public int backgroundPaddingSize = 20;
    public TextMeshProUGUI experienceText;
    // Start is called before the first frame update
    void Start()
    {
        typingControllerScript.inputAcceptorScript.GotCorrectWord += GotCorrectWord;
        enemySpawnerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<EnemySpawner>();

        // if making a save file, need to replace this
        SetLevel(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMovement()
    {

    }

    public void GotCorrectWord()
    {
        enemySpawnerScript.RemoveEnemy(this);
        // should kill itself while giving points to enemy spawnmanaer
        Destroy(gameObject, 0.1f);
    }

    public void SetLevel(int setValue)
    {
        levelOfEnemy = setValue;

        // sets experience to be X00 + 0-99
        SetExperience((levelOfEnemy * 100) + Random.Range(0, 100));
        SetText();
    }

    public void SetExperience(int setValue)
    {
        experienceWorth = setValue;
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
        StartCoroutine(StartBackgroundSize());

    }

    public IEnumerator StartBackgroundSize()
    {
        yield return null;
        SetBackgroundSize();
    }

    public void SetBackgroundSize()
    {
       // print($"In set background{backgroundObject.GetComponent<RectTransform>().sizeDelta}");

        Vector2 textSize = typingControllerScript.inputAcceptorScript.shownRect.sizeDelta;
        backgroundObject.GetComponent<RectTransform>().sizeDelta = new Vector2(textSize.x + backgroundPaddingSize, textSize.y + backgroundPaddingSize); // Add padding
        //print($"In set background{backgroundObject.GetComponent<RectTransform>().sizeDelta}");

    }

    public void SetText()
    {
        experienceText.text = $"Lv: {levelOfEnemy}";
    }

}
