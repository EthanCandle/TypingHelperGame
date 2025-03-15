using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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

     public GameManager gm; 
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        columnsOfEnemies = new List<List<GameObject>>(); // Make sure it's initialized
        columnsOfEnemies.Add(column1);
        columnsOfEnemies.Add(column2);
        columnsOfEnemies.Add(column3);




    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            SpawnEnemy();
        }
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

        if(columnToSpawnIn == gm.GetColumnOn())
        {
            SetTargetIfNoEnemies(); // set target if there was no other enemies in the lane

        }
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
    {
        if (columnsOfEnemies[gm.GetColumnOn()].Count == 0)
        {
            return;
        }
        // deactive previous enemy
        SetDeactiveTypingActiveOnEnemy(enemyCurrentScript);

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
