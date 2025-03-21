using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager experienceManager;

    public List<ColoumnObject> columnScripts;

    

    // Start is called before the first frame update
    void Start()
    {
       if(experienceManager == null)
        {
            // if we don't have it, use this as the one to keep 
            experienceManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // kills self if we already have this object
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void IncreaseExperience(int columnToChange,int amountToChangeBy)
    {
        // called by the enemy when it dies

        // adds experience to the list 
        columnScripts[columnToChange].IncreaseExperience(amountToChangeBy);

    }


}
