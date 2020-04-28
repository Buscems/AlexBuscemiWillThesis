using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Rewired;
using Rewired.ControllerExtensions;


public class TitleButtons : MonoBehaviour
{

    public EventSystem es;
    public GameObject startButton;
    public GameObject classicButton;

    public Animator fadeGame;
    public Animator fadeQuit;

    public Animator camAnimator;

    public GameObject cText, rText, oText;

    //the following is in order to use rewired
    [Tooltip("Reference for using rewired")]
    [HideInInspector]
    public Player myPlayer;
    [Header("Rewired")]
    [Tooltip("Number identifier for each player, must be above 0")]
    public int playerNum;

    private void Awake()
    {
        //Rewired Code + Goop bois
        myPlayer = ReInput.players.GetPlayer(playerNum - 1);
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        CheckController(myPlayer);
    }

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

        if (myPlayer.GetButtonDown("Back"))
        {
            if (camAnimator.GetBool("Move") && !camAnimator.GetBool("HowToPlay"))
            {
                MoveOff();
            }
            if (camAnimator.GetBool("HowToPlay"))
            {
                HowToPlayOff();
            }
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
        PlayerPrefs.SetInt("GameMode", 0);
        SceneManager.LoadScene("Character Select");
    }

    public void GoToRandomGame()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        SceneManager.LoadScene("Character Select");
    }

    public void GoToOneForAllGame()
    {
        PlayerPrefs.SetInt("GameMode", 2);
        SceneManager.LoadScene("Character Select");
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

    //[REWIRED METHODS]
    //these two methods are for ReWired, if any of you guys have any questions about it I can answer them, but you don't need to worry about this for working on the game - Buscemi
    void OnControllerConnected(ControllerStatusChangedEventArgs arg)
    {
        CheckController(myPlayer);
    }
    void CheckController(Player player)
    {
        foreach (Joystick joyStick in player.controllers.Joysticks)
        {
            var ds4 = joyStick.GetExtension<DualShock4Extension>();
            if (ds4 == null) continue;//skip this if not DualShock4
            switch (playerNum - 1)
            {
                case 3:
                    ds4.SetLightColor(Color.white);
                    break;
                case 2:
                    ds4.SetLightColor(Color.blue);
                    Debug.Log("Yert");
                    break;
                case 1:
                    ds4.SetLightColor(Color.green);
                    break;
                case 0:
                    ds4.SetLightColor(Color.red);
                    break;
                default:
                    ds4.SetLightColor(Color.white);
                    Debug.LogError("Player Num is 0, please change to a number > 0");
                    break;
            }
        }
    }
}
