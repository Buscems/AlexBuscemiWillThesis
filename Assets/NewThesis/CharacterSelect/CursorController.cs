using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine.UI;
using TMPro;

public class CursorController : MonoBehaviour
{

    //the following is in order to use rewired
    [Tooltip("Reference for using rewired")]
    [HideInInspector]
    public Player myPlayer;
    [Header("Rewired")]
    [Tooltip("Number identifier for each player, must be above 0")]
    public int playerNum;

    Rigidbody2D rb;
    public float speed;
    Vector2 velocity;

    public CharacterSelectScreen characterSelect;

    public Image thisImage;
    public TextMeshProUGUI thisText;

    GameObject currentGoop;

    public BoxCollider2D currentGoopBox;

    bool hasSelected;

    public bool isReady;

    public GameObject readyText;

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
        thisImage.enabled = false;
        thisText.enabled = false;
        rb = GetComponent<Rigidbody2D>();
        readyText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        velocity = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical")) * speed;
        
        if(Mathf.Abs(velocity.x) > 0 || Mathf.Abs(velocity.y) > 0 && !thisText.enabled)
        {
            thisImage.enabled = true;
            thisText.enabled = true;
        }

        if(myPlayer.GetButtonDown("Select") && currentGoop != null && !hasSelected)
        {
            hasSelected = true;
            this.transform.position = currentGoop.transform.position;
            characterSelect.SetCharacter(playerNum, currentGoop);
            readyText.SetActive(true);
            isReady = true;
            currentGoopBox.enabled = false;
        }
        if (myPlayer.GetButtonDown("Back") && hasSelected)
        {
            characterSelect.UnSetCharacter(playerNum);
            currentGoopBox.enabled = true;
            readyText.SetActive(false);
            isReady = false;
            hasSelected = false;
        }

    }

    private void FixedUpdate()
    {
        if (!hasSelected)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Goop")
        {
            currentGoop = collision.gameObject;
            currentGoopBox = collision.GetComponent<BoxCollider2D>();
            characterSelect.SetAnimator(playerNum, currentGoop);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        currentGoop = null;
        if (!hasSelected)
        {
            currentGoopBox = null;
        }
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
