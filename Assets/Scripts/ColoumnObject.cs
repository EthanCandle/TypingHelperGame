using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ColoumnObject : MonoBehaviour
{
    public string textFormat = "XXX/XXX";
    public TextMeshProUGUI experienceText, levelText;
    public int experienceCurrent, levelCurrent, experienceThreshold;
    public SliderNew experienceSlider;

    public List<int> columnLevelUpTable; // exp needed to level up
    // Start is called before the first frame update
    void Start()
    {
        SetStats(0, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IncreaseExperience(int amountToChangeBy)
    {
        // returns true if we leveled up so the thing calling can set the stats
        SetExperiencePoints(experienceCurrent + amountToChangeBy);

        if (CheckIfCanLevelUp())
        {
            IncrementLevel();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckIfCanLevelUp()
    {
        // if we have enough experience that meets the threshold then return true
        if(experienceCurrent >= experienceThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncrementLevel()
    { 
        // level ++
        SetLevel(levelCurrent + 1);

        // exp = 0 since we leveled up
        SetExperiencePoints(0);
    }

    public void SetStats(int level, int experience)
    {
        SetLevel(level);
        SetExperiencePoints(experience);

    }

    public void SetLevel(int level)
    {
        // sets the level of the column
        levelCurrent = level;
        SetThreshold(columnLevelUpTable[level]);
        SetText();
    }
    public void SetExperiencePoints(int points)
    {
        // sets the experience amount
        experienceCurrent = points;
        SetText();
    }
    public void SetThreshold(int threshold)
    {
        // sets the level up requirement amount of experience
        experienceThreshold = threshold;
        SetText();
    }
    public void SetText()
    {
       // print($"{experienceCurrent}/{experienceThreshold}");
        // sets the experience and level up text
        experienceText.text = $"{experienceCurrent}/{experienceThreshold}";
        levelText.text = $"Lv: {levelCurrent}";
    }

    // need to have 3 indivual column scripts that derive from this script, polymoripshim or some thing?


}
