using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;
using Rewired.ControllerExtensions;

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

    public Animator menuAnim;

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
        if (myPlayer.GetButtonDown("rb"))
        {
            if(activeMenu == ActiveMenu.Inventory)
            {
                menuAnim.SetTrigger("ToParty");
            }
            if (activeMenu == ActiveMenu.Party)
            {
                menuAnim.SetTrigger("ToSave");
            }
            if (activeMenu == ActiveMenu.Save)
            {
                menuAnim.SetTrigger("ToInventory");
            }
            menuAnim.SetFloat("Blend", 1);
            if (activeMenu != lastMenu)
            {
                activeMenu += 1;
            }
            else
            {
                activeMenu = firstMenu;
            }
        }
        if (myPlayer.GetButtonDown("lb"))
        {
            if (activeMenu == ActiveMenu.Inventory)
            {
                menuAnim.SetTrigger("ToSave");
            }
            if (activeMenu == ActiveMenu.Party)
            {
                menuAnim.SetTrigger("ToInventory");
            }
            if (activeMenu == ActiveMenu.Save)
            {
                menuAnim.SetTrigger("ToParty");
            }
            if (activeMenu != firstMenu)
            {
                activeMenu -= 1;
            }
            else
            {
                activeMenu = lastMenu;
            }
        }
    }

    /*
    IEnumerator TurnRight()
    {
        inventoryMenu.transform.RotateAround(middlePoint.transform.position, Vector2.up, 20 * Time.deltaTime);
        
    }
    */
    public void ResumeGame()
    {
        es.SetSelectedGameObject(null);
        Time.timeScale = 1;
        this.gameObject.SetActive(false);
    }

    public void AreYouSure()
    {
        areYouSureMenu.SetActive(true);
        es.SetSelectedGameObject(notSureButton);
    }

    public void NotSure()
    {
        es.SetSelectedGameObject(quitButton);
        areYouSureMenu.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
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
