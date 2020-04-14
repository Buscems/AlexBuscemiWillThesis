using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine.EventSystems;

public class OverworldPlayer : MonoBehaviour
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
    [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public bool movementIsPlaying;
    Vector3 startPos, endPos;
    public GameObject endPosCollider;
    float timeToMove;
    public float walkSpeed;
    public AudioSource walkThud;
    bool playThud;
    Animator anim;

    [Header("Raycast Variables")]
    public float rayDistance;
    public LayerMask obstacleLayer;

    public bool canGoDown;
    public bool canGoUp;
    public bool canGoRight;
    public bool canGoLeft;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public GameObject resumeButton;
    public EventSystem es;


    //[HideInInspector]
    public bool hasBeenDetectedByEnemy;

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

        pauseMenu.SetActive(false);

        try
        {
            anim = GetComponent<Animator>();
        }
        catch
        {
            Debug.LogError("No Animator attached");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseMenu.activeSelf)
        {
            if (myPlayer.GetButtonDown("Pause") && !isMoving)
            {
                Pause();
            }
            if (!isMoving)
            {
                transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            }
            CheckForObstacles();
            if (!hasBeenDetectedByEnemy)
            {
                if (!isMoving)
                {
                    direction = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));

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
                                case Direction.North:
                                    anim.SetFloat("Blend", 1);
                                    break;

                                case Direction.East:
                                    anim.SetFloat("Blend", 3);
                                    break;

                                case Direction.South:
                                    anim.SetFloat("Blend", 0);
                                    break;

                                case Direction.West:
                                    anim.SetFloat("Blend", 2);
                                    break;
                            }
                        }
                        if (currentDirection == Direction.South && !canGoDown)
                        {
                            if (!playThud && walkThud != null)
                            {
                                walkThud.Play();
                                playThud = true;
                            }
                        }
                        else if (currentDirection == Direction.North && !canGoUp)
                        {
                            if (!playThud && walkThud != null)
                            {
                                walkThud.Play();
                                playThud = true;
                            }
                        }
                        else if (currentDirection == Direction.East && !canGoRight)
                        {
                            if (!playThud && walkThud != null)
                            {
                                walkThud.Play();
                                playThud = true;
                            }
                        }
                        else if (currentDirection == Direction.West && !canGoLeft)
                        {
                            if (!playThud && walkThud != null)
                            {
                                walkThud.Play();
                                playThud = true;
                            }
                        }
                        else
                        {
                            if (!movementIsPlaying)
                            {
                                StartCoroutine(Movement(transform));
                            }
                        }
                    }
                    else
                    {
                        playThud = false;
                    }
                }

            }
        }

    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        es.SetSelectedGameObject(resumeButton);
        Time.timeScale = 0;
    }

    void CheckForObstacles()
    {

        if (SomethingBelow())
        {
            canGoDown = false;
        }
        else
        {
            canGoDown = true;
        }
        if (SomethingAbove())
        {
            canGoUp = false;
        }
        else
        {
            canGoUp = true;
        }
        if (SomethingRight())
        {
            canGoRight = false;
        }
        else
        {
            canGoRight = true;
        }
        if (SomethingLeft())
        {
            canGoLeft = false;
        }
        else
        {
            canGoLeft = true;
        }

    }

    bool SomethingBelow()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, Vector2.down, Color.white);
            return true;
        }
        Debug.DrawRay(transform.position, Vector2.down, Color.yellow);
        return false;
    }
    bool SomethingAbove()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, rayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, Vector2.up, Color.white);
            return true;
        }
        Debug.DrawRay(transform.position, Vector2.up, Color.yellow);
        return false;
    }
    bool SomethingRight()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, rayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, Vector2.right, Color.white);
            return true;
        }
        Debug.DrawRay(transform.position, Vector2.right, Color.yellow);
        return false;
    }
    bool SomethingLeft()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, rayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, Vector2.left, Color.white);
            return true;
        }
        Debug.DrawRay(transform.position, Vector2.left, Color.yellow);
        return false;
    }

    IEnumerator Movement(Transform entity)
    {
        movementIsPlaying = true;
        isMoving = true;
        anim.SetBool("isMoving", true);

        startPos = entity.position;
        timeToMove = 0;

        endPos = new Vector3(startPos.x + System.Math.Sign(direction.x), startPos.y + System.Math.Sign(direction.y), startPos.z);
        while (timeToMove < 1f && !hasBeenDetectedByEnemy)
        {
            timeToMove += Time.deltaTime * walkSpeed;
            entity.GetComponent<Rigidbody2D>().MovePosition(Vector3.Lerp(startPos, endPos, timeToMove));
            yield return null;
        }

        anim.SetBool("isMoving", false);
        isMoving = false;
        yield return new WaitForSeconds(.0001f);
        movementIsPlaying = false;

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
