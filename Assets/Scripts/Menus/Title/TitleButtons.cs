using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class TitleButtons : MonoBehaviour
{

    public EventSystem es;
    public GameObject startButton;

    public Animator fadeGame;
    public Animator fadeQuit;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButton()
    {
        fadeGame.SetTrigger("Fade");
    }

    public void QuitButton()
    {
        fadeQuit.SetTrigger("Fade");
    }

    public void GoToGame()
    {
        SceneManager.LoadScene("Pablo");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetFirstButton()
    {
        es.SetSelectedGameObject(startButton);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Title");
    }

}
