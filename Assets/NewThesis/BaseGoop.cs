using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEditor;
using UnityEditor.Animations;
using JetBrains.Annotations;
using UnityEngine.UI;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;

public class BaseGoop : MonoBehaviour
{

    public GameObject uiCanvas;

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
    [HideInInspector]
    public int goopColor;

    public enum Direction { North, South, East, West }
    [Header("Movement")]
    public Direction currentDirection;
    public Vector2 direction;
    public Vector2 velocity;
    [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public Animator anim;
    public float speed;
    public float tierTwoSpeed;
    public float reviveSpeed;
    [SerializeField]
    float currentSpeed;
    public float joystickDeadzone;

    Rigidbody2D rb;

    [Header("Damage")]
    public int[] knightDamage;
    public int[] knightSlashDamage;
    public int[] rogueDamage;
    public int[] witchDamage;
    public int tierTwoDamageDealt;
    public int tierTwoDamageTaken;

    public enum Class { Knight, Rogue, Witch }
    [Header("Classes")]
    public Class currentClass;

    public GameObject basicProjectile;
    public GameObject slashHolder;
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
    public float tierOneRogueAttackDelay;
    public float tierOneWitchAttackDelay;
    public float tierTwoAttackDelay;
    float attackDelay;

    public float tierOneKnightProjectileSpeed;
    public float tierOneRogueProjectileSpeed;
    public float tierOneWitchProjectileSpeed;
    public float tierTwoProjectileSpeed;

    [Header("Tier Level")]
    public GameObject thisTierOneGoop;
    public GameObject thisTierTwoGoop;
    [HideInInspector]
    public bool tierTwo;
    bool hasTransformed;
    bool attacking;
    public GameObject tierTwoStars;

    [Header("Switching Classes")]
    public float switchCooldown;
    [HideInInspector]
    public bool isSwitching;

    [Header("Getting Hit")]
    public int maxHealth;
    int health;
    public float invincibilityTime;
    public GameObject[] killSections;
    public BaseGoop[] players;
    [HideInInspector]
    public bool hasBeenHit;
    [HideInInspector]
    public bool hitBySlash;
    public GameObject slashHitSprite;
    public float slashHitDuration;
    [HideInInspector]
    public Color slashHitColor;
    bool hitAnimation;
    Animator flashAnim;
    bool knockback;
    public float knockbackSpeed;
    [HideInInspector]
    public Vector2 knockbackDirection;
    public float knockbackDuration;
    [HideInInspector]
    public int lastPlayerThatHitThis;
    public float[] vibrationamount;
    public float[] vibrationDuration;

    public ParticleSystem hitParticles;

    [Header("Effects - Puffs")]
    public GameObject swapPuff;
    public GameObject walkPuff;

    [Header("Poof Timer")]
    public float maxPoofTime;
    private float currentPoofTimer;

    [Header("Effects - Splat")]
    public GameObject goopSplat;

    [Header("Spawning")]
    public float spawnTime;
    public float spawningOpacity;
    [HideInInspector]
    public bool isSpawning;
    bool gameHasStarted;

    [Header("Mana")]
    public float maxMana;
    [HideInInspector]
    public float mana;
    public float manaGainPerSecond;
    public float manaGainPerHit;
    public float manaGainWhenHit;
    public float manaLossOnDeath;
    public float tierTwoDuration;
    float manaLossPerSecondTierTwo;

    [Header("UI Stuff")]
    public Image healthBar;
    public Image manaBar;

    [Header("Pause")]
    public GameObject pauseMenu;
    public static int pauseNumber;
    public TextMeshProUGUI pauseText;
    public static bool isPaused;
    [HideInInspector]
    public int currentKills;

    [HideInInspector]
    public bool lost;

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
        tierTwoStars.SetActive(false);
        slashHitSprite.SetActive(false);

        tierTwo = false;
        manaLossPerSecondTierTwo = maxMana / tierTwoDuration;

        StartCoroutine(SpawnPlayer());

        if (pauseMenu.activeSelf)
        {
            pauseMenu.SetActive(false);
        }

        healthBar.color = basicProjectile.GetComponent<Projectile>().colors[goopColor];

        //poof timer
        currentPoofTimer = maxPoofTime;

    }

    // Update is called once per frame
    void Update()
    {
        if (!isPaused)
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
                if (!tierTwo)
                {
                    ClassController();
                }

                //animation stuff
                flashAnim.SetBool("hasBeenHit", hitAnimation);

                CheckForWall();

                if (health <= 0)
                {
                    StartCoroutine(SpawnPlayer());
                }

                if (mana < maxMana && !tierTwo)
                {
                    mana += manaGainPerSecond * Time.deltaTime;
                }

                if (myPlayer.GetButtonDown("Transform") && mana >= maxMana)
                {
                    tierTwo = true;
                }

                if (tierTwo)
                {
                    TierTwo();
                }

            }
        }

        if (GameplayController.countdownOver && !GameplayController.gameEnd)
        {
            Pause();
        }

        healthBar.fillAmount = (float)health / (float)maxHealth;
        manaBar.fillAmount = mana / maxMana;
        manaLossOnDeath = (int)mana / 2;
        if(mana < 0)
        {
            mana = 0;
        }
        if (mana > maxMana)
        {
            mana = maxMana;
        }

    }

    private void FixedUpdate()
    {
        if (!knockback)
        {
            rb.MovePosition(rb.position + velocity * currentSpeed * Time.fixedDeltaTime);
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
                var poofMain = poof.GetComponent<ParticleSystem>().main;
                poofMain.startColor = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, poofMain.startColor.color.a);
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
        }
    }

    void Pause()
    {
        if (myPlayer.GetButtonDown("Pause"))
        {
            if (!isPaused)
            {
                pauseNumber = this.playerNum;
                pauseMenu.SetActive(true);
                pauseText.color = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, pauseText.color.a);
                isPaused = true;
                Time.timeScale = 0;
            }
            else if(isPaused && pauseNumber == this.playerNum)
            {
                pauseNumber = 0;
                pauseMenu.SetActive(false);
                pauseText.color = new Color(1, 1, 1, pauseText.color.a);
                Time.timeScale = 1;
                isPaused = false;
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
        if (!isSwitching && GameplayController.classic)
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

    void TierTwo()
    {
        mana -= manaLossPerSecondTierTwo * Time.deltaTime;
        if (!hasTransformed)
        {
            thisTierOneGoop.SetActive(false);
            thisTierTwoGoop.SetActive(true);
            anim = thisTierTwoGoop.GetComponent<Animator>();
            currentSpeed = tierTwoSpeed;
            hasTransformed = true;
            tierTwoStars.SetActive(true);
            tierTwoStars.transform.GetChild(0).GetComponent<SpriteRenderer>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
            tierTwoStars.transform.GetChild(0).GetComponent<TrailRenderer>().startColor = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, tierTwoStars.transform.GetChild(0).GetComponent<TrailRenderer>().startColor.a);
            tierTwoStars.transform.GetChild(0).GetComponent<TrailRenderer>().endColor = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, tierTwoStars.transform.GetChild(0).GetComponent<TrailRenderer>().endColor.a);
            
            tierTwoStars.transform.GetChild(1).GetComponent<SpriteRenderer>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
            tierTwoStars.transform.GetChild(1).GetComponent<TrailRenderer>().startColor = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, tierTwoStars.transform.GetChild(0).GetComponent<TrailRenderer>().startColor.a);
            tierTwoStars.transform.GetChild(1).GetComponent<TrailRenderer>().endColor = new Color(basicProjectile.GetComponent<Projectile>().colors[goopColor].r, basicProjectile.GetComponent<Projectile>().colors[goopColor].g, basicProjectile.GetComponent<Projectile>().colors[goopColor].b, tierTwoStars.transform.GetChild(0).GetComponent<TrailRenderer>().endColor.a);
        }

        if(mana <= 0)
        {
            thisTierOneGoop.SetActive(true);
            thisTierTwoGoop.SetActive(false);
            mana = 0;
            anim = thisTierOneGoop.GetComponent<Animator>();
            hasTransformed = false;
            tierTwoStars.SetActive(false);
            tierTwo = false;
        }

    }

    public void SwitchRandomClass(int newClass)
    {
        switch (newClass)
        {
            case 0:
                currentClass = Class.Knight;
                break;
            case 1:
                currentClass = Class.Rogue;
                break;
            case 2:
                currentClass = Class.Witch;
                break;
        }
        var poof = Instantiate(swapPuff, transform.position, Quaternion.identity);
        var poofMain = poof.GetComponent<ParticleSystem>().main;
        poofMain.startColor = basicProjectile.GetComponent<Projectile>().colors[goopColor];
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
        var poofMain = poof.GetComponent<ParticleSystem>().main;
        poofMain.startColor = basicProjectile.GetComponent<Projectile>().colors[goopColor];
        yield return new WaitForSeconds(switchCooldown);
        isSwitching = false;
    }

    IEnumerator Attack()
    {
        if (attackDirection == Vector2.down && !canGoDown && currentClass != Class.Witch)
        {
            //do nothing
        }
        else if (attackDirection == Vector2.right && !canGoRight && currentClass != Class.Witch)
        {
            //do nothing
        }
        else if (attackDirection == Vector2.up && !canGoUp && currentClass != Class.Witch)
        {
            //do nothing
        }
        else if (attackDirection == Vector2.left && !canGoLeft && currentClass != Class.Witch)
        {
            //do nothing
        }
        else
        {
            attacking = true;
            if (!tierTwo)
            {
                switch (currentClass)
                {
                    case Class.Knight:
                        var bp = Instantiate(slashHolder, rb.position + (attackDirection / 2), Quaternion.identity);
                        bp.transform.GetChild(0).GetComponent<SpriteRenderer>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                        var slashScript = bp.transform.GetChild(0).GetComponent<KnightSlash>();
                        slashScript.direction = attackDirection;
                        slashScript.thisPlayer = this.GetComponent<BaseGoop>();
                        slashScript.playerNum = this.playerNum;
                        attackDelay = tierOneKnightAttackDelay;
                        break;
                    case Class.Rogue:
                        for (int i = 0; i < rogueKnifeAmount; i++)
                        {
                            float newDir = 0;
                            var bp2 = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                            var projScript2 = bp2.GetComponent<Projectile>();
                            if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                            {
                                if (transform.position.x > 0)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            bp2.layer = 15;
                                            break;
                                        case 1:

                                            break;
                                        case 2:
                                            bp2.layer = 15;
                                            break;
                                    }
                                }
                                if (transform.position.x < 0)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            bp2.layer = 15;
                                            break;
                                        case 1:
                                            bp2.layer = 15;
                                            break;
                                        case 2:

                                            break;
                                    }
                                }
                            }
                            else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                            {
                                if (transform.position.y > 0)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            bp2.layer = 16;
                                            break;
                                        case 1:

                                            break;
                                        case 2:
                                            bp2.layer = 16;
                                            break;
                                    }
                                }
                                if (transform.position.y < 0)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            bp2.layer = 16;
                                            break;
                                        case 1:
                                            bp2.layer = 16;
                                            break;
                                        case 2:

                                            break;
                                    }
                                }
                            }
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
                            if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                            {
                                projScript2.direction = attackDirection + new Vector2(newDir, 0);
                            }
                            else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                            {
                                projScript2.direction = attackDirection + new Vector2(0, newDir);
                            }
                            projScript2.projectileColor = projScript2.colors[goopColor];
                            projScript2.speed = tierOneRogueProjectileSpeed;
                            projScript2.currentClass = Projectile.Class.Rogue;
                            projScript2.thisPlayer = this.GetComponent<BaseGoop>();
                            projScript2.playerNum = this.playerNum;
                        }
                        attackDelay = tierOneRogueAttackDelay;
                        break;
                    case Class.Witch:
                        var bp3 = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                        var projScript3 = bp3.GetComponent<Projectile>();
                        projScript3.direction = this.attackDirection;
                        projScript3.projectileColor = projScript3.colors[goopColor];
                        projScript3.speed = tierOneWitchProjectileSpeed;
                        projScript3.currentClass = Projectile.Class.Witch;
                        projScript3.thisPlayer = this.GetComponent<BaseGoop>();
                        if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                        {
                            bp3.layer = 15;
                        }
                        else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                        {
                            bp3.layer = 16;
                        }
                        projScript3.playerNum = this.playerNum;
                        attackDelay = tierOneWitchAttackDelay;
                        break;
                }
            }
            else
            {
                for (int i = 0; i < rogueKnifeAmount; i++)
                {
                    float newDir = 0;
                    var bp = Instantiate(basicProjectile, rb.position + (attackDirection / 2), Quaternion.identity);
                    var projScript = bp.GetComponent<Projectile>();
                    if(attackDirection == Vector2.down || attackDirection == Vector2.up)
                    {
                        if (transform.position.x > 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    bp.layer = 15;
                                    break;
                                case 1:
                                    
                                    break;
                                case 2:
                                    bp.layer = 15;
                                    break;
                            }
                        }
                        if (transform.position.x < 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    bp.layer = 15;
                                    break;
                                case 1:
                                    bp.layer = 15;
                                    break;
                                case 2:
                                    
                                    break;
                            }
                        }
                    }
                    else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                    {
                        if (transform.position.y > 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    bp.layer = 16;
                                    break;
                                case 1:
                                    
                                    break;
                                case 2:
                                    bp.layer = 16;
                                    break;
                            }
                        }
                        if (transform.position.y < 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    bp.layer = 16;
                                    break;
                                case 1:
                                    bp.layer = 16;
                                    break;
                                case 2:
                                    
                                    break;
                            }
                        }
                    }
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
                    if (attackDirection == Vector2.down || attackDirection == Vector2.up)
                    {
                        projScript.direction = attackDirection + new Vector2(newDir, 0);
                    }
                    else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
                    {
                        projScript.direction = attackDirection + new Vector2(0, newDir);
                    }
                    projScript.projectileColor = projScript.colors[goopColor];
                    projScript.speed = tierTwoProjectileSpeed;
                    projScript.thisPlayer = this.GetComponent<BaseGoop>();
                    projScript.playerNum = this.playerNum;
                    attackDelay = tierTwoAttackDelay;
                }
            }
            yield return new WaitForSeconds(attackDelay);
            attacking = false;
        }
    }

    public void KnightProjectile(Vector2 lastDirection)
    {
        var bp1 = Instantiate(basicProjectile, rb.position + (lastDirection / 2), Quaternion.identity);
        var projScript1 = bp1.GetComponent<Projectile>();
        projScript1.direction = lastDirection;
        projScript1.isKnightProjectile = true;
        projScript1.projectileColor = projScript1.colors[goopColor];
        projScript1.speed = tierOneKnightProjectileSpeed;
        projScript1.currentClass = Projectile.Class.Knight;
        projScript1.thisPlayer = this.GetComponent<BaseGoop>();
        if (attackDirection == Vector2.down || attackDirection == Vector2.up)
        {
            bp1.layer = 15;
        }
        else if (attackDirection == Vector2.right || attackDirection == Vector2.left)
        {
            bp1.layer = 16;
        }
        projScript1.playerNum = this.playerNum;
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
        PlayerPrefs.SetString("Player1Color", "Red");
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

    

    public IEnumerator GetHit(int classNum)
    {
        hasBeenHit = true;
        int damageToBeTaken = 0;
        if (!hitBySlash)
        {
            if (classNum != 3)
            {
                if (!tierTwo)
                {
                    switch (currentClass)
                    {

                        case Class.Knight:
                            switch (classNum)
                            {
                                case 0:
                                    damageToBeTaken = knightDamage[1];

                                    GameObject gameSplat1 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    gameSplat1.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    gameSplat1.GetComponent<SplatChooser>().isUI = false;
                                    StartCoroutine(ControllerVibration(1));
                                    break;
                                case 1:
                                    damageToBeTaken = rogueDamage[0];
                                    StartCoroutine(ControllerVibration(0));
                                    break;
                                case 2:
                                    damageToBeTaken = witchDamage[2];

                                    GameObject gameSplat2 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    gameSplat2.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    gameSplat2.GetComponent<SplatChooser>().isUI = false;

                                    GameObject uiSplat = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    uiSplat.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    uiSplat.GetComponent<SplatChooser>().isUI = true;
                                    uiSplat.transform.SetParent(uiCanvas.transform, false);
                                    StartCoroutine(ControllerVibration(2));
                                    break;
                            }
                            break;
                        case Class.Rogue:
                            switch (classNum)
                            {
                                case 0:
                                    damageToBeTaken = knightDamage[2];

                                    GameObject gameSplat1 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    gameSplat1.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    gameSplat1.GetComponent<SplatChooser>().isUI = false;

                                    GameObject uiSplat = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    uiSplat.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    uiSplat.GetComponent<SplatChooser>().isUI = true;
                                    uiSplat.transform.SetParent(uiCanvas.transform, false);
                                    StartCoroutine(ControllerVibration(2));
                                    break;
                                case 1:
                                    damageToBeTaken = rogueDamage[1];

                                    GameObject gameSplat2 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    gameSplat2.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    gameSplat2.GetComponent<SplatChooser>().isUI = false;
                                    StartCoroutine(ControllerVibration(1));
                                    break;
                                case 2:
                                    damageToBeTaken = witchDamage[0];
                                    StartCoroutine(ControllerVibration(0));
                                    break;
                            }
                            break;
                        case Class.Witch:
                            switch (classNum)
                            {
                                case 0:
                                    damageToBeTaken = knightDamage[0];
                                    StartCoroutine(ControllerVibration(0));
                                    break;
                                case 1:
                                    damageToBeTaken = rogueDamage[2];

                                    GameObject gameSplat1 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    gameSplat1.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    gameSplat1.GetComponent<SplatChooser>().isUI = false;

                                    GameObject uiSplat = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    uiSplat.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    uiSplat.GetComponent<SplatChooser>().isUI = true;
                                    uiSplat.transform.SetParent(uiCanvas.transform, false);
                                    StartCoroutine(ControllerVibration(2));
                                    break;
                                case 2:
                                    damageToBeTaken = witchDamage[1];

                                    GameObject gameSplat2 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                                    gameSplat2.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                                    gameSplat2.GetComponent<SplatChooser>().isUI = false;
                                    StartCoroutine(ControllerVibration(1));
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    GameObject gameSplat1 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                    gameSplat1.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                    gameSplat1.GetComponent<SplatChooser>().isUI = false;
                }
            }
            else
            {
                damageToBeTaken = tierTwoDamageDealt;
                GameObject gameSplat1 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                gameSplat1.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                gameSplat1.GetComponent<SplatChooser>().isUI = false;
                StartCoroutine(ControllerVibration(2));
            }
        }
        else
        {
            switch (currentClass)
            {
                case Class.Knight:
                    damageToBeTaken = knightSlashDamage[1];

                    GameObject gameSplat1 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                    gameSplat1.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                    gameSplat1.GetComponent<SplatChooser>().isUI = false;
                    StartCoroutine(ControllerVibration(1));
                    break;
                case Class.Rogue:
                    damageToBeTaken = knightSlashDamage[2];

                    GameObject gameSplat2 = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                    gameSplat2.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                    gameSplat2.GetComponent<SplatChooser>().isUI = false;

                    GameObject uiSplat = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                    uiSplat.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                    uiSplat.GetComponent<SplatChooser>().isUI = true;
                    uiSplat.transform.SetParent(uiCanvas.transform, false);
                    StartCoroutine(ControllerVibration(2));
                    break;
                case Class.Witch:
                    damageToBeTaken = knightSlashDamage[0];
                    StartCoroutine(ControllerVibration(0));
                    break;
            }
            StartCoroutine(ShowSlashHit());
        }
        if (tierTwo)
        {
            if (!hitBySlash)
            {
                switch (classNum)
                {
                    case 0:
                        damageToBeTaken = knightDamage[0];
                        break;
                    case 1:
                        damageToBeTaken = rogueDamage[0];
                        break;
                    case 2:
                        damageToBeTaken = witchDamage[0];
                        break;
                }
            }
            else
            {
                damageToBeTaken = knightSlashDamage[0];
                GameObject gameSplat = Instantiate(goopSplat, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                gameSplat.GetComponent<SplatChooser>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
                gameSplat.GetComponent<SplatChooser>().isUI = false;

                StartCoroutine(ShowSlashHit());

            }
            StartCoroutine(ControllerVibration(0));
        }
        health -= damageToBeTaken;
        if (!tierTwo)
        {
            mana += manaGainWhenHit;
        }
        if(health > 0)
        {
            hitAnimation = true;
        }
        StartCoroutine(Knockback());
        yield return new WaitForSeconds(invincibilityTime);
        hitAnimation = false;
        hitBySlash = false;
        hasBeenHit = false;
    }

    IEnumerator Knockback()
    {
        knockback = true;
        yield return new WaitForSeconds(knockbackDuration);
        knockback = false;
    }

    IEnumerator ControllerVibration(int vibrationNumber)
    {
        myPlayer.SetVibration(0, vibrationamount[vibrationNumber]);
        yield return new WaitForSeconds(vibrationDuration[vibrationNumber]);
        myPlayer.StopVibration();
    }

    IEnumerator ShowSlashHit()
    {
        slashHitSprite.SetActive(true);
        slashHitSprite.transform.SetParent(null);
        slashHitSprite.GetComponent<SpriteRenderer>().color = slashHitColor;
        slashHitSprite.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        yield return new WaitForSeconds(slashHitDuration);
        slashHitSprite.transform.position = this.transform.position;
        slashHitSprite.transform.SetParent(this.transform);
        slashHitSprite.SetActive(false);
    }

    public IEnumerator SpawnPlayer()
    {   
        isSpawning = true;
        if (tierTwo)
        {
            thisTierOneGoop.SetActive(true);
            thisTierTwoGoop.SetActive(false);
            tierTwoStars.SetActive(false);
            anim = thisTierOneGoop.GetComponent<Animator>();
            hasTransformed = false;
            tierTwo = false;
        }
        //adding kills to a player
        if (gameHasStarted && !lost)
        {
            killSections[lastPlayerThatHitThis - 1].transform.GetChild(players[lastPlayerThatHitThis - 1].currentKills).GetComponent<Image>().color = basicProjectile.GetComponent<Projectile>().colors[goopColor];
            killSections[lastPlayerThatHitThis - 1].transform.GetChild(players[lastPlayerThatHitThis - 1].currentKills).transform.GetChild(1).GetComponent<ParticleSystem>().Play();
            players[lastPlayerThatHitThis - 1].DoTheParticle();
            players[lastPlayerThatHitThis - 1].currentKills++;
        }
        currentSpeed = reviveSpeed;
        mana -= manaLossOnDeath;
        anim.SetInteger("Class", 3);
        flashAnim.SetBool("Spawn", true);
        while (lost)
        {
            yield return null;
        }
        if (gameHasStarted)
        {
            yield return new WaitForSeconds(spawnTime);
        }
        else
        {
            while (!GameplayController.countdownOver)
            {
                yield return null;
            }
        }
        flashAnim.SetBool("Spawn", false);
        if (GameplayController.classic)
        {
            int rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    currentClass = Class.Knight;
                    break;
                case 1:
                    currentClass = Class.Rogue;
                    break;
                case 2:
                    currentClass = Class.Witch;
                    break;
            }
        }
        else
        {
            switch (GameplayController.gameClass)
            {
                case 0:
                    currentClass = Class.Knight;
                    break;
                case 1:
                    currentClass = Class.Rogue;
                    break;
                case 2:
                    currentClass = Class.Witch;
                    break;
            }
        }
        if(GameplayController.random && !gameHasStarted)
        {
            int rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    currentClass = Class.Knight;
                    break;
                case 1:
                    currentClass = Class.Rogue;
                    break;
                case 2:
                    currentClass = Class.Witch;
                    break;
            }
        }
        health = maxHealth;
        currentSpeed = speed;
        if (!gameHasStarted)
        {
            gameHasStarted = true;
        }
        isSpawning = false;
    }

    public void DoTheParticle()
    {
        StartCoroutine(PlayHitParticles());
    }

    public IEnumerator PlayHitParticles()
    {
        hitParticles.transform.SetParent(null);
        var partMain = hitParticles.main;
        partMain.startColor = basicProjectile.GetComponent<Projectile>().colors[goopColor];
        hitParticles.Play();
        yield return new WaitForSeconds(1f);
        hitParticles.transform.position = this.transform.position;
        hitParticles.transform.SetParent(this.transform);
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
