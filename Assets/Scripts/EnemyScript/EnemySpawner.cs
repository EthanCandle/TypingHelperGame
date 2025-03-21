using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner enemySpawner;

    // need a wave scriptable object
    // need 
    public int rowOn = 0;
    public int enemiesToStillSpawn;
    public List<List<GameObject>> columnsOfEnemies;
    public List<GameObject> column1,column2,column3;
    public List<GameObject> enemiesSpawned;
    //public List<GameObject> enemiesSpawnedSorted;
    public GameObject enemyCurrentObj;
    public EnemyManager enemyCurrentScript;
    public GameObject enemyToSpawn; // need to replace with a list of enemies
    public List<Vector3> spawnPositions;

    public LevelWaveScriptableObject levelWaves;
    public List<WaveScriptableObject> wavesToSpawn;
    public int waveOn = 0;
    public Coroutine waveCurrentlyOnTimer;

    //public int enemiesToSpawn = 10;

     public GameManager gm; 
    // Start is called before the first frame update
    void Start()
    {
        // sets the static reference per scene
        enemySpawner = this;

        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        columnsOfEnemies = new List<List<GameObject>>(); // Make sure it's initialized
        columnsOfEnemies.Add(column1);
        columnsOfEnemies.Add(column2);
        columnsOfEnemies.Add(column3);
        SetLevelWaves();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            SpawnWave();   
            //SpawnEnemy();
        }
    }

    public void RemoveEnemy(EnemyManager enemyToRemove)
    {
        ExperienceManager.experienceManager.IncreaseExperience(enemyToRemove.columnIn, enemyToRemove.experienceWorth);

        // remove enemy from list
        columnsOfEnemies[enemyToRemove.columnIn].Remove(enemyToRemove.gameObject);


    }

    public void SetLevelWaves()
    {
        for(int i = 0; i < levelWaves.waves.Count; i++)
        {
            wavesToSpawn.Add(levelWaves.waves[i]);
        }
    }

    public void SpawnWave()
    {
        if(waveOn > wavesToSpawn.Count)
        {
            // means we are on last wave
            print("On last wave");
            return;
        }
        float maxTime = 0;

        // for each column
        for(int i = 0; i < gm.columnTotal; i++)
        {
            float timeToWait = 0;
            // for each enemy to spawn
            for (int j = 0; j < wavesToSpawn[waveOn].enemyAmounts[i]; j++)
            {
                timeToWait += Random.Range(0.0f, 5.0f) + 3.25f;

                // delay each subsequent enemy by 0.25 seconds
                StartCoroutine(DelayEnemySpawn(i, timeToWait));
            }
            // gets longest time it takes for a single enemy to spawn
            if(timeToWait > maxTime)
            {
                maxTime = timeToWait;
            }
        }
        waveOn++;

        // sets next wavev timer and the coroutine to stop it
        waveCurrentlyOnTimer = StartCoroutine(DelayedFunction(SpawnWave, maxTime + 4));
        // start countdown to the next wave spawning, need to interupt when enemies killed is half 
        print($"On wave: {waveOn}");
    }

    public void StopNextWaveTimer()
    {
        // stops wave set up
        StopCoroutine(waveCurrentlyOnTimer);
    }

    public

    //IEnumerator DelayedFunction(System.Action functionToCall)
    //{
    //    // this delays for 1 frame
    //    yield return null;

    //    // calls function given
    //    functionToCall();
    //}

    IEnumerator DelayedFunction(System.Action functionToCall, float delay)
    {

        // delays based on a time given
        yield return new WaitForSeconds(delay);
        functionToCall();
    }
    IEnumerator DelayEnemySpawn(int column, float delay)
    {

        // delays based on a time given
        yield return new WaitForSeconds(delay);
        SpawnEnemy(column);
    }
    public int GetRandomColumn()
    {
        return Random.Range(0, gm.columnTotal);
    }

    public void SpawnEnemy()
    {
        SpawnEnemy(GetRandomColumn());
    }

    public void SpawnEnemy(int columnToSpawnIn)
    {
        // we have 3 columns, so has to be between 0-2
        GameObject enemySpawned = Instantiate(enemyToSpawn, spawnPositions[columnToSpawnIn], enemyToSpawn.transform.rotation);
        columnsOfEnemies[columnToSpawnIn].Add(enemySpawned);

        EnemyManager enemySpawnedScript = enemySpawned.GetComponent<EnemyManager>();

        enemySpawnedScript.SetColumn(columnToSpawnIn);
        enemySpawnedScript.SetWord();
        enemySpawnedScript.SetLevel(GetRandomLevel());
        SetDeactiveTypingActiveOnEnemy(enemySpawnedScript);
        if (columnToSpawnIn == gm.GetColumnOn())
        {
            SetTargetIfNoEnemies(); // set target if there was no other enemies in the lane
        }

    }

    public int GetRandomLevel()
    {
        // returns a random number from 0 to whatever level we are on + 1 since exclusive
        return Random.Range(0, GameManager.current.levelOn + 1);
    }


    public void SetTargetIfNoEnemies()
    {
        // set target if there was no other enemies in the lane
        if (columnsOfEnemies[gm.GetColumnOn()].Count != 1)
        {
            return;
        }
        GetAndSetClosestEnemy();
    }



    public void GetAndSetClosestEnemy()
    {        // deactive previous enemy
        SetDeactiveTypingActiveOnEnemy(enemyCurrentScript);
        if (columnsOfEnemies[gm.GetColumnOn()].Count == 0)
        {
            return;
        }


        // finds closest enemy and sets it as active target
        print($"{gm.GetColumnOn()}");
        enemyCurrentObj = columnsOfEnemies[gm.GetColumnOn()][0];
        enemyCurrentScript = enemyCurrentObj.GetComponent<EnemyManager>();
        SetActiveTypingActiveOnEnemy(enemyCurrentScript);
    }

    public void SetActiveTypingActiveOnEnemy(EnemyManager enemyToChange)
    {
        if (enemyToChange == null)
        {
            return;
        }
        enemyToChange.SetAsActiveTarget();
    }

    public void SetDeactiveTypingActiveOnEnemy(EnemyManager enemyToChange)
    {
        if(enemyToChange == null)
        {
            return;
        }
        // turns off enemy
        enemyToChange.SetAsDeactiveTarget();
    }


}
