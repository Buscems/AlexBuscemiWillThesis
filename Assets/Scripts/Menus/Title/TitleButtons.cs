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
    public GameObject classicButton;

    public Animator fadeGame;
    public Animator fadeQuit;

    public Animator camAnimator;

    public GameObject cText, rText, oText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }
    }

    public void StartButton()
    {
        camAnimator.SetBool("Move", true);
        //fadeGame.SetTrigger("Fade");
    }

    public void SetFade(){
        fadeGame.SetTrigger("Fade");
    }

    public void QuitButton()
    {
        fadeQuit.SetTrigger("Fade");
    }

    public void GoToGame()
    {
        SceneManager.LoadScene("GameplayScene 1");
    }

    public void GoToRandomGame()
    {
        SceneManager.LoadScene("GameplayScene 1");
    }

    public void GoToOneForAllGame()
    {
        SceneManager.LoadScene("GameplayScene 1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetFirstButton()
    {
        es.SetSelectedGameObject(startButton);
    }

    public void SetButtonNull()
    {
        es.SetSelectedGameObject(null);
    }

    public void SetButtonClassic()
    {
        es.SetSelectedGameObject(classicButton);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Title");
    }

    public void RestartGameScene()
    {
        SceneManager.LoadScene("GameplayScene 1");
    }

    public void SetTextClassic(){
        cText.SetActive(true);
        rText.SetActive(false);
        oText.SetActive(false);
    }

    public void SetTextRandom()
    {
        cText.SetActive(false);
        rText.SetActive(true);
        oText.SetActive(false);
    }

    public void SetTextOne()
    {
        cText.SetActive(false);
        rText.SetActive(false);
        oText.SetActive(true);
    }

    public void HowToPlayOn(){
        camAnimator.SetBool("HowToPlay", true);
    }
    public void HowToPlayOff()
    {
        camAnimator.SetBool("HowToPlay", false);
    }

    public void MoveOff(){
        camAnimator.SetBool("Move", false);
    }
}
