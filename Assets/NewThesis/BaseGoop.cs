using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEditor;
using UnityEditor.Animations;

public class BaseGoop : MonoBehaviour
{

    //the following is in order to use rewired
    [Tooltip("Reference for using rewired")]
    [HideInInspector]
    public Player myPlayer;
    [Header("Rewired")]
    [Tooltip("Number identifier for each player, must be above 0")]
    public int playerNum;

    [Tooltip("0 = Red\n1 = Green\n2 = Blue\n3 = Yellow\n4 = Pink\n5 = Purple\n6 = Orange\n7 = White")]
    public AnimatorController[] tierOneGoopColor;
    [Tooltip("0 = Red\n1 = Green\n2 = Blue\n3 = Yellow\n4 = Pink\n5 = Purple\n6 = Orange\n7 = White")]
    public AnimatorController[] tierTwoGoopColor;
    int goopColor;

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

    public GameObject basicProjectile;
    [SerializeField]
    Vector2 attackDirection;

    public float tierOneKnightAttackDelay;
    public float tierTwoKnightAttackDelay;
    public float tierOneRogueAttackDelay;
    public float tierTwoRogueAttackDelay;
    public float tierOneWitchAttackDelay;
    public float tierTwoWitchAttackDelay;
    float attackDelay;

    public float tierOneKnightProjectileSpeed;
    public float tierTwoKnightProjectileSpeed;
    public float tierOneRogueProjectileSpeed;
    public float tierTwoRogueProjectileSpeed;

    [Header("Tier Level")]
    public GameObject thisTierOneGoop;
    public GameObject thisTierTwoGoop;

    bool tierTwo;
    bool attacking;

    [Header("Switching Classes")]
    public float switchCooldown;
    bool isSwitching;
    

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
        GoopSetter();
        thisTierOneGoop.GetComponent<Animator>().runtimeAnimatorController = tierOneGoopColor[goopColor];
        thisTierTwoGoop.GetComponent<Animator>().runtimeAnimatorController = tierTwoGoopColor[goopColor];
        rb = GetComponent<Rigidbody2D>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        thisTierTwoGoop.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        if (myPlayer.GetButtonDown("Attack") && !attacking)
        {
            StartCoroutine("Attack");
        }

        ClassController();

    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * speed *  Time.deltaTime);
    }

    void Movement()
    {
        direction = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));
        velocity = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));

        if (velocity.x > 0.5f)
        {
            velocity.x = 1;
        }
        else if (velocity.x < -0.5f)
        {
            velocity.x = -1;
        }
        else
        {
            velocity.x = 0;
        }

        if (velocity.y > 0.5f)
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
                attackDirection = Vector2.left;
            }
            if (direction.x > 0)
            {
                currentDirection = Direction.East;
                attackDirection = Vector2.right;
            }
            if (direction.y < 0)
            {
                currentDirection = Direction.South;
                attackDirection = Vector2.down;
            }
            if (direction.y > 0)
            {
                currentDirection = Direction.North;
                attackDirection = Vector2.up;
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

    void ClassController()
    {
        switch (currentClass)
        {
            case Class.Knight:
                anim.SetInteger("Class", 0);
                break;
            case Class.Rogue:
                anim.SetInteger("Class", 1);
                break;
            case Class.Witch:
                anim.SetInteger("Class", 2);
                break;
        }
        if (!isSwitching)
        {
            if (myPlayer.GetButtonDown("SwitchRight"))
            {
                StartCoroutine(SwitchClass(0));
            }
            if (myPlayer.GetButtonDown("SwitchLeft"))
            {
                StartCoroutine(SwitchClass(1));
            }
        }
    }

    IEnumerator SwitchClass(int direction)
    {
        isSwitching = true;
        if (direction == 0)
        {
            switch (currentClass)
            {
                case Class.Knight:
                    currentClass = Class.Rogue;
                    break;
                case Class.Rogue:
                    currentClass = Class.Witch;
                    break;
                case Class.Witch:
                    currentClass = Class.Knight;
                    break;
            }
        }
        if (direction == 1)
        {
            switch (currentClass)
            {
                case Class.Knight:
                    currentClass = Class.Witch;
                    break;
                case Class.Rogue:
                    currentClass = Class.Knight;
                    break;
                case Class.Witch:
                    currentClass = Class.Rogue;
                    break;
            }
        }
        yield return new WaitForSeconds(switchCooldown);
        isSwitching = false;
    }

    IEnumerator Attack()
    {
        attacking = true;
        switch (currentClass)
        {
            case Class.Knight:
                if (!tierTwo)
                {
                    var bp = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                    var projScript = bp.GetComponent<Projectile>();
                    projScript.direction = this.attackDirection;
                    projScript.projectileColor = projScript.colors[goopColor];
                    projScript.speed = tierOneKnightProjectileSpeed;
                    projScript.currentClass = Projectile.Class.Knight;
                    attackDelay = tierOneKnightAttackDelay;
                }
                else
                {

                }
                break;
            case Class.Rogue:
                if (!tierTwo)
                {
                    var bp = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                    var projScript = bp.GetComponent<Projectile>();
                    projScript.direction = this.attackDirection;
                    projScript.projectileColor = projScript.colors[goopColor];
                    projScript.speed = tierOneRogueProjectileSpeed;
                    projScript.currentClass = Projectile.Class.Rogue;
                    attackDelay = tierOneRogueAttackDelay;
                }
                else
                {

                }
                break;
            case Class.Witch:
                if (!tierTwo)
                {

                }
                else
                {

                }
                break;
        }

        yield return new WaitForSeconds(attackDelay);
        attacking = false;
    }

    void GoopSetter()
    {
        //for test
        PlayerPrefs.SetString("Player1Color", "Blue");
        switch (playerNum)
        {
            case 1:
                switch (PlayerPrefs.GetString("Player1Color"))
                {
                    case "Red":
                        goopColor = 0;
                        break;

                    case "Green":
                        goopColor = 1;
                        break;

                    case "Blue":
                        goopColor = 2;
                        break;
                }
                break;
            case 2:
                switch (PlayerPrefs.GetString("Player2Color"))
                {
                    case "Red":
                        goopColor = 0;
                        break;

                    case "Green":
                        goopColor = 1;
                        break;

                    case "Blue":
                        goopColor = 2;
                        break;
                }
                break;
            case 3:
                switch (PlayerPrefs.GetString("Player3Color"))
                {
                    case "Red":
                        goopColor = 0;
                        break;

                    case "Green":
                        goopColor = 1;
                        break;

                    case "Blue":
                        goopColor = 2;
                        break;
                }
                break;
            case 4:
                switch (PlayerPrefs.GetString("Player4Color"))
                {
                    case "Red":
                        goopColor = 0;
                        break;

                    case "Green":
                        goopColor = 1;
                        break;

                    case "Blue":
                        goopColor = 2;
                        break;
                }
                break;
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
