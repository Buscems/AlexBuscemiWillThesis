using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEditor;

public class BaseGoop : MonoBehaviour
{

    //the following is in order to use rewired
    [Tooltip("Reference for using rewired")]
    [HideInInspector]
    public Player myPlayer;
    [Header("Rewired")]
    [Tooltip("Number identifier for each player, must be above 0")]
    public int playerNum;

    public enum Direction { North, South, East, West }
    [Header("Movement")]
    public Direction currentDirection;
    public Vector2 direction;
    public Vector2 velocity;
    [HideInInspector]
    public bool isMoving;
    Animator anim;
    public float speed;

    Rigidbody2D rb;

    public enum Class { Knight, Rogue, Witch }
    [Header("Classes")]
    public Class currentClass;

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
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));
        velocity = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));

        if(velocity.x > 0.5f)
        {
            velocity.x = 1;
        }
        else if(velocity.x < -0.5f)
        {
            velocity.x = -1;
        }
        else
        {
            velocity.x = 0;
        }

        if(velocity.y > 0.5f)
        {
            velocity.y = 1;
        }
        else if (velocity.y < -0.5f)
        {
            velocity.y = -1;
        }
        else
        {
            velocity.y = 0;
        }


        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            direction.y = 0;
        }
        else
        {
            direction.x = 0;
        }
        if (direction != Vector2.zero)
        {
            if (direction.x < 0)
            {
                currentDirection = Direction.West;
            }
            if (direction.x > 0)
            {
                currentDirection = Direction.East;
            }
            if (direction.y < 0)
            {
                currentDirection = Direction.South;
            }
            if (direction.y > 0)
            {
                currentDirection = Direction.North;
            }
            if (anim != null)
            {
                switch (currentDirection)
                {
                    case Direction.South:
                        anim.SetFloat("Blend", 0);
                        break;
                    case Direction.East:
                        anim.SetFloat("Blend", 1);
                        break;
                    case Direction.North:
                        anim.SetFloat("Blend", 2);
                        break;
                    case Direction.West:
                        anim.SetFloat("Blend", 3);
                        break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * speed *  Time.deltaTime);
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
