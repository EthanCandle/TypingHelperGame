using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public GameManager gm; 
    // Start is called before the first frame update
    void Start()
    {
        // add self to event
        GameManager.current.OnCorrectWord += UpdateScoreText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void OnDestroy()
    {
        // remove self from event
        GameManager.current.OnCorrectWord -= UpdateScoreText;
    }

}
