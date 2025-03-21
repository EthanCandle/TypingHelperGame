using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
//    public LevelTransitionManager levelTransition;
    private void Start()
    {
       // levelTransition = GameObject.FindGameObjectWithTag("Transition").GetComponent<LevelTransitionManager>();
    }


    public void PlayGame ()
    {
      //  levelTransition.PlayTransitionIn();
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame ()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }




}
