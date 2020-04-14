using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    //the following is in order to use rewired
    [Tooltip("Reference for using rewired")]
    [HideInInspector]
    public Player myPlayer;
    [Header("Rewired")]
    [Tooltip("Number identifier for each player, must be above 0")]
    public int playerNum;

    public EventSystem es;

    public GameObject areYouSureMenu;
    public GameObject notSureButton;
    public GameObject quitButton;

    public GameObject inventoryMenu;
    public GameObject partyMenu;
    public GameObject saveMenu;

    public GameObject inventoryStartButton;
    public GameObject partyStartButton;
    public GameObject saveStartButton;

    public Animator quitFade;

    public Transform menuPivot;
    public float pivotInterval;
    public float rotateSpeed;
    bool rotateRight;
    bool rotateLeft;
    bool isRotating;
    public Vector3 menuRotation;

    public enum ActiveMenu { Inventory, Party, Save }
    public ActiveMenu activeMenu;
    public ActiveMenu firstMenu;
    public ActiveMenu lastMenu;


    private void Awake()
    {
        //Rewired Code
        myPlayer = ReInput.players.GetPlayer(playerNum - 1);
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        CheckController(myPlayer);
    }

    // Start is called before the first frame update
    void Start()
    {
        areYouSureMenu.SetActive(false);
        activeMenu = firstMenu;
        lastMenu = ActiveMenu.Save;
    }

    // Update is called once per frame
    void Update()
    {
        menuPivot.eulerAngles = menuRotation;
        if (!areYouSureMenu.activeSelf)
        {
            if (myPlayer.GetButtonDown("rb") && !isRotating)
            {
                StartCoroutine("SwitchRight");
            }
            if (myPlayer.GetButtonDown("lb") && !isRotating)
            {
                StartCoroutine("SwitchLeft");
            }
        }

        if (rotateRight)
        {
            menuRotation += new Vector3(0, 0, -rotateSpeed * Time.unscaledDeltaTime);
        }
        if (rotateLeft)
        {
            menuRotation += new Vector3(0, 0, rotateSpeed * Time.unscaledDeltaTime);
        }
    }

    IEnumerator SwitchRight()
    {
        isRotating = true;
        float nextPivot = menuRotation.z - pivotInterval;
        Debug.Log(nextPivot);
        while(menuRotation.z > nextPivot)
        {
            rotateRight = true;
            yield return null;
        }
        rotateRight = false;
        menuRotation = new Vector3(menuRotation.x, menuRotation.y, nextPivot);
        es.SetSelectedGameObject(null);
        EndGoingRight();
    }
    IEnumerator SwitchLeft()
    {
        isRotating = true;
        float nextPivot = menuRotation.z + pivotInterval;
        while (menuRotation.z < nextPivot)
        {
            rotateLeft = true;
            yield return null;
        }
        rotateLeft = false;
        menuRotation = new Vector3(menuRotation.x, menuRotation.y, nextPivot);
        es.SetSelectedGameObject(null);
        EndGoingLeft();
    }

    void EndGoingRight()
    {
        if (activeMenu != lastMenu)
        {
            activeMenu += 1;
        }
        else
        {
            activeMenu = firstMenu;
        }
        switch (activeMenu)
        {
            case ActiveMenu.Inventory:
                es.SetSelectedGameObject(inventoryStartButton);
                break;
            case ActiveMenu.Party:
                es.SetSelectedGameObject(partyStartButton);
                break;
            case ActiveMenu.Save:
                es.SetSelectedGameObject(saveStartButton);
                break;
        }
        isRotating = false;
    }
    void EndGoingLeft()
    {
        if (activeMenu != firstMenu)
        {
            activeMenu -= 1;
        }
        else
        {
            activeMenu = lastMenu;
        }
        switch (activeMenu)
        {
            case ActiveMenu.Inventory:
                es.SetSelectedGameObject(inventoryStartButton);
                break;
            case ActiveMenu.Party:
                es.SetSelectedGameObject(partyStartButton);
                break;
            case ActiveMenu.Save:
                es.SetSelectedGameObject(saveStartButton);
                break;
        }
        isRotating = false;
    }

    public void ResumeGame()
    {
        es.SetSelectedGameObject(null);
        Time.timeScale = 1;
        menuRotation = Vector3.zero;
        this.gameObject.SetActive(false);
    }

    public void AreYouSure()
    {
        areYouSureMenu.SetActive(true);
        es.SetSelectedGameObject(notSureButton);
    }

    public void NotSure()
    {
        es.SetSelectedGameObject(saveStartButton);
        areYouSureMenu.SetActive(false);
    }

    public void Quit()
    {
        quitFade.SetTrigger("Fade");
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
            switch (playerNum)
            {
                case 4:
                    ds4.SetLightColor(Color.yellow);
                    break;
                case 3:
                    ds4.SetLightColor(Color.green);
                    break;
                case 2:
                    ds4.SetLightColor(Color.blue);
                    break;
                case 1:
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
