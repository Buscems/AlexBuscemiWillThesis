using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEditor;
using UnityEditor.Animations;
using JetBrains.Annotations;

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
    public Color[] controllerColors;

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

    public float joystickDeadzone;

    Rigidbody2D rb;

    [Header("Damage")]
    public int lowDamage;
    public int normalDamage;
    public int highDamage;

    public enum Class { Knight, Rogue, Witch }
    [Header("Classes")]
    public Class currentClass;

    public GameObject basicProjectile;
    public int rogueKnifeAmount;
    [SerializeField]
    Vector2 attackDirection;

    [Header("Raycast Variables")]
    public float rayDistance;
    public LayerMask obstacleLayer;
    public bool canGoDown;
    public bool canGoUp;
    public bool canGoRight;
    public bool canGoLeft;

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
    public float tierOneWitchProjectileSpeed;
    public float tierTwoWitchProjectileSpeed;

    [Header("Tier Level")]
    public GameObject thisTierOneGoop;
    public GameObject thisTierTwoGoop;

    bool tierTwo;
    bool attacking;

    [Header("Switching Classes")]
    public float switchCooldown;
    bool isSwitching;

    [Header("Getting Hit")]
    public int maxHealth;
    int health;
    public float invincibilityTime;
    [HideInInspector]
    public bool hasBeenHit;
    Animator flashAnim;
    bool knockback;
    public float knockbackSpeed;
    [HideInInspector]
    public Vector2 knockbackDirection;
    public float knockbackDuration;

    [Header("Effects - Puffs")]
    public GameObject swapPuff;
    public GameObject walkPuff;

    [Header("Poof Timer")]
    public float maxPoofTime;
    private float currentPoofTimer;

    [Header("Spawning")]
    public float spawnTime;
    public float spawningOpacity;
    bool isSpawning;

    private void Awake()
    {
        //Rewired Code + Goop bois
        myPlayer = ReInput.players.GetPlayer(playerNum - 1);
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        GoopSetter();
        CheckController(myPlayer);
    }

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        thisTierOneGoop.GetComponent<Animator>().runtimeAnimatorController = tierOneGoopColor[goopColor];
        thisTierTwoGoop.GetComponent<Animator>().runtimeAnimatorController = tierTwoGoopColor[goopColor];
        rb = GetComponent<Rigidbody2D>();
        anim = thisTierOneGoop.GetComponent<Animator>();
        flashAnim = GetComponent<Animator>();
        thisTierTwoGoop.SetActive(false);

        StartCoroutine(SpawnPlayer());

        //poof timer
        currentPoofTimer = maxPoofTime;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        if (!isSpawning)
        {
            if (myPlayer.GetButton("Attack") && !attacking)
            {
                StartCoroutine("Attack");
            }
            /*
            if (!attacking)
            {
                StartCoroutine("Attack");
            }
            */
            ClassController();

            //animation stuff
            flashAnim.SetBool("hasBeenHit", hasBeenHit);

            CheckForWall();
        }
    }

    private void FixedUpdate()
    {
        if (!knockback)
        {
            rb.MovePosition(rb.position + velocity * speed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + knockbackDirection * knockbackSpeed * Time.fixedDeltaTime);
        }
    }

    void Movement()
    {
        if(Mathf.Abs(myPlayer.GetAxis("DirectionHorizontal")) > 0 || Mathf.Abs(myPlayer.GetAxis("DirectionVertical")) > 0)
        {
            direction = new Vector2(myPlayer.GetAxis("DirectionHorizontal"), myPlayer.GetAxis("DirectionVertical"));
        }
        else
        {
            direction = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));
        }
        velocity = new Vector2(myPlayer.GetAxis("MoveHorizontal"), myPlayer.GetAxis("MoveVertical"));

        if (!isSpawning)
        {
            //poof code
            if (Mathf.Abs(velocity.x) > joystickDeadzone || Mathf.Abs(velocity.y) > joystickDeadzone)
            {

            }
            currentPoofTimer -= Time.deltaTime;

            if (currentPoofTimer < 0)
            {
                GameObject poof = Instantiate(walkPuff, transform.position, Quaternion.identity);
                Vector3 size = poof.transform.eulerAngles / 3;
                poof.transform.eulerAngles = size;
                poof.GetComponent<ParticleSystem>().startColor = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, poof.GetComponent<ParticleSystem>().startColor.a);
                currentPoofTimer = maxPoofTime;
            }
        }

        if (velocity.x > joystickDeadzone)
        {
            velocity.x = 1;
        }
        else if (velocity.x < -joystickDeadzone)
        {
            velocity.x = -1;
        }
        else
        {
            velocity.x = 0;
        }

        if (velocity.y > joystickDeadzone)
        {
            velocity.y = 1;
        }
        else if (velocity.y < -joystickDeadzone)
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
        var poof = Instantiate(swapPuff, transform.position, Quaternion.identity);
        poof.GetComponent<ParticleSystem>().startColor = basicProjectile.GetComponent<Projectile>().colors[goopColor];
        yield return new WaitForSeconds(switchCooldown);
        isSwitching = false;
    }

    IEnumerator Attack()
    {
        if (attackDirection == Vector2.down && !canGoDown)
        {
            //do nothing
        }
        else if (attackDirection == Vector2.right && !canGoRight)
        {
            //do nothing
        }
        else if (attackDirection == Vector2.up && !canGoUp)
        {
            //do nothing
        }
        else if (attackDirection == Vector2.left && !canGoLeft)
        {
            //do nothing
        }
        else
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
                        if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                        {
                            bp.layer = 15;
                        }
                        else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                        {
                            bp.layer = 16;
                        }
                        projScript.playerNum = this.playerNum;
                        attackDelay = tierOneKnightAttackDelay;
                    }
                    else
                    {

                    }
                    break;
                case Class.Rogue:
                    if (!tierTwo)
                    {
                        for (int i = 0; i < rogueKnifeAmount; i++)
                        {
                            float newDir = 0;
                            switch (i)
                            {
                                case 0:
                                    newDir = 0;
                                    break;
                                case 1:
                                    newDir = .5f;
                                    break;
                                case 2:
                                    newDir = -.5f;
                                    break;
                            }
                            var bp = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                            var projScript = bp.GetComponent<Projectile>();
                            if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                            {
                                projScript.direction = attackDirection + new Vector2(newDir, 0);
                            }
                            else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                            {
                                projScript.direction = attackDirection + new Vector2(0, newDir);
                            }
                            projScript.projectileColor = projScript.colors[goopColor];
                            projScript.speed = tierOneRogueProjectileSpeed;
                            projScript.currentClass = Projectile.Class.Rogue;
                            projScript.playerNum = this.playerNum;
                        }
                        attackDelay = tierOneRogueAttackDelay;
                    }
                    else
                    {

                    }
                    break;
                case Class.Witch:
                    if (!tierTwo)
                    {
                        var bp = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                        var projScript = bp.GetComponent<Projectile>();
                        projScript.direction = this.attackDirection;
                        projScript.projectileColor = projScript.colors[goopColor];
                        projScript.speed = tierOneWitchProjectileSpeed;
                        projScript.currentClass = Projectile.Class.Witch;
                        if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                        {
                            bp.layer = 15;
                        }
                        else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                        {
                            bp.layer = 16;
                        }
                        projScript.playerNum = this.playerNum;
                        attackDelay = tierOneWitchAttackDelay;
                    }
                    else
                    {

                    }
                    break;
            }

            yield return new WaitForSeconds(attackDelay);
            attacking = false;
        }
    }

    void CheckForWall()
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

    void GoopSetter()
    {
        //for test
        PlayerPrefs.SetString("Player1Color", "Green");
        PlayerPrefs.SetString("Player2Color", "Green");
        PlayerPrefs.SetString("Player3Color", "Blue");
        PlayerPrefs.SetString("Player4Color", "Yellow");
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
                    case "Yellow":
                        goopColor = 3;
                        break;
                    case "Pink":
                        goopColor = 4;
                        break;
                    case "Purple":
                        goopColor = 5;
                        break;
                    case "Orange":
                        goopColor = 6;
                        break;
                    case "White":
                        goopColor = 7;
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
                    case "Yellow":
                        goopColor = 3;
                        break;
                    case "Pink":
                        goopColor = 4;
                        break;
                    case "Purple":
                        goopColor = 5;
                        break;
                    case "Orange":
                        goopColor = 6;
                        break;
                    case "White":
                        goopColor = 7;
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
                    case "Yellow":
                        goopColor = 3;
                        break;
                    case "Pink":
                        goopColor = 4;
                        break;
                    case "Purple":
                        goopColor = 5;
                        break;
                    case "Orange":
                        goopColor = 6;
                        break;
                    case "White":
                        goopColor = 7;
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
                    case "Yellow":
                        goopColor = 3;
                        break;
                    case "Pink":
                        goopColor = 4;
                        break;
                    case "Purple":
                        goopColor = 5;
                        break;
                    case "Orange":
                        goopColor = 6;
                        break;
                    case "White":
                        goopColor = 7;
                        break;
                }
                break;
        }
    }

    

    public IEnumerator GetHit()
    {
        hasBeenHit = true;
        Debug.Log("Yert");
        int damageToBeTaken = 0;
        /*
        switch (currentClass)
        {
            
            case Class.Knight:
                switch (classNum)
                {
                    case 0:
                        damageToBeTaken = normalDamage;
                        break;
                    case 1:
                        damageToBeTaken = lowDamage;
                        break;
                    case 2:
                        damageToBeTaken = highDamage;
                        break;
                }
                break;
            case Class.Rogue:
                switch (classNum)
                {
                    case 0:
                        damageToBeTaken = highDamage;
                        break;
                    case 1:
                        damageToBeTaken = normalDamage;
                        break;
                    case 2:
                        damageToBeTaken = lowDamage;
                        break;
                }
                break;
            case Class.Witch:
                switch (classNum)
                {
                    case 0:
                        damageToBeTaken = lowDamage;
                        break;
                    case 1:
                        damageToBeTaken = highDamage;
                        break;
                    case 2:
                        damageToBeTaken = normalDamage;
                        break;
                }
                break;
        }
        */
        health -= damageToBeTaken;
        StartCoroutine(Knockback());
        yield return new WaitForSeconds(invincibilityTime);
        hasBeenHit = false;
    }

    IEnumerator Knockback()
    {
        knockback = true;
        yield return new WaitForSeconds(knockbackDuration);
        knockback = false;
    }

    IEnumerator SpawnPlayer()
    {
        isSpawning = true;
        anim.SetInteger("Class", 3);
        var sr = thisTierOneGoop.GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, spawningOpacity);
        yield return new WaitForSeconds(spawnTime);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
        int rand = Random.Range(0, 3);
        anim.SetInteger("Class", rand);
        isSpawning = false;
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
            ds4.SetLightColor(controllerColors[goopColor]);
            /*
            switch (goopColor)
            {
                case 7:
                    ds4.SetLightColor(basicProjectile.GetComponent<Projectile>().colors[7]);
                    break;
                case 6:
                    ds4.SetLightColor(basicProjectile.GetComponent<Projectile>().colors[6]);
                    break;
                case 5:
                    ds4.SetLightColor(basicProjectile.GetComponent<Projectile>().colors[5]);
                    break;
                case 4:
                    ds4.SetLightColor(basicProjectile.GetComponent<Projectile>().colors[4]);
                    break;
                case 3:
                    ds4.SetLightColor(Color.yellow);
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
            */
        }
    }
}
