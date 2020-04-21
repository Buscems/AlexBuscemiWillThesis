using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverworldEnemy : MonoBehaviour
{

    public enum Direction { Up, Down, Left, Right}
    [Header("Movement")]
    public Direction currentDirection;
    public Vector2 direction;
    bool isMoving;
    Vector3 startPos, endPos;
    public GameObject endPosTrigger;
    public GameObject behindTrigger;
    float timeToMove;
    public float walkSpeed;
    Animator anim;

    public enum MovementType { UntilHitObstacle, UntilCertainAmountOfSpaces }
    [Header("Movement Type")]
    public MovementType thisMovementType;

    Rigidbody2D rb;

    private int spacesMoved;
    [Tooltip("How many spaces you want the enemy to move before changing directions")]
    public int spacesUntilChange;
    public enum MovementDirection { UpDown, LeftRight, UpRightDownLeft, UpLeftDownRight, DownRightUpLeft, DownLeftUpRight}
    public MovementDirection moveDirection;
    bool upDown;
    bool leftRight;
    bool upRightDownLeft;
    bool UpLeftDownRight;
    bool downRightUpLeft;
    bool downLeftUpRight;

    [Header("Checking Obstacles")]
    public float obstacleRayDistance;
    public LayerMask obstacleLayer;

    [Header("CheckingForPlayer")]
    public float playerRayDistance;
    public LayerMask playerLayer;
    public GameObject noticeIndicator;
    public float moveDelay;
    public AudioSource noticeSound;

    [Tooltip("This is for starting the battle scene")]
    public string transitionName;
    public Animator transitionFade;

    public bool hasDetectedPlayer;
    GameObject player;
    bool getReady;

    public string enemyTriggerName;
    public TurnOffEnemies turnOffEnemies;

    // Start is called before the first frame update
    void Start()
    {

        turnOffEnemies = GameObject.Find(enemyTriggerName).GetComponent<TurnOffEnemies>();

        transitionFade = GameObject.Find(transitionName).GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        try
        {
            anim = GetComponent<Animator>();
        }
        catch
        {
            Debug.LogError("Animator is missing from " + gameObject.name);
        }
        noticeIndicator.SetActive(false);

        endPosTrigger.transform.parent = null;

        if (currentDirection == Direction.Left)
        {
            direction = Vector2.left;
        }
        if (currentDirection == Direction.Right)
        {
            direction = Vector2.right;
        }
        if (currentDirection == Direction.Down)
        {
            direction = Vector2.down;
        }
        if (currentDirection == Direction.Up)
        {
            direction = Vector2.up;
        }

        switch (moveDirection)
        {
            case MovementDirection.UpDown:
                upDown = true;
                break;
            case MovementDirection.LeftRight:
                leftRight = true;
                break;
            case MovementDirection.UpLeftDownRight:
                UpLeftDownRight = true;
                break;
            case MovementDirection.UpRightDownLeft:
                upRightDownLeft = true;
                break;
            case MovementDirection.DownLeftUpRight:
                downLeftUpRight = true;
                break;
            case MovementDirection.DownRightUpLeft:
                downRightUpLeft = true;
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {

        DirectionChecker();

        switch (thisMovementType)
        {
            case MovementType.UntilHitObstacle:

                MovementUntilObstacle();

                break;
        }

        if (IsPlayerInFront())
        {
            hasDetectedPlayer = true;
            if (!getReady)
            {
                StartCoroutine(ReadyBattleScene());
                getReady = true;
            }
        }
        else
        {
            hasDetectedPlayer = false;
        }

        if (!isMoving && !hasDetectedPlayer)
        {
            transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            StartCoroutine(Movement(this.transform));
        }

        behindTrigger.transform.localPosition = -direction / 2;
        
    }

    private void FixedUpdate()
    {
        
    }

    void DirectionChecker()
    {
        if (direction == Vector2.left)
        {
            currentDirection = Direction.Left;
        }
        if (direction == Vector2.right)
        {
            currentDirection = Direction.Right;
        }
        if (direction == Vector2.down)
        {
            currentDirection = Direction.Down;
        }
        if (direction == Vector2.up)
        {
            currentDirection = Direction.Up;
        }
        if (anim != null)
        {
            switch (currentDirection)
            {
                case Direction.Up:
                    anim.SetFloat("Blend", 2);
                    break;

                case Direction.Down:
                    anim.SetFloat("Blend", 0);
                    break;

                case Direction.Left:
                    anim.SetFloat("Blend", 3);
                    break;

                case Direction.Right:
                    anim.SetFloat("Blend", 1);
                    break;
            }
        }
    }

    void MovementUntilObstacle()
    {
        CheckForObstacles();
    }

    void CheckForObstacles()
    {
        if (ObstacleInFront() && !isMoving)
        {
            direction *= -1;
        }
    }

    bool ObstacleInFront()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleRayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, direction, Color.white);
            return true;
        }
        Debug.DrawRay(transform.position, direction, Color.yellow);
        return false;
    }
    bool IsPlayerInFront()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, playerRayDistance, playerLayer);
        if (hit.collider != null)
        {
            player = hit.collider.gameObject;
            var ps = player.GetComponent<OverworldPlayer>();
            if (!ps.isMoving)
            {
                ps.hasBeenDetectedByEnemy = true;
                ps.transform.position = new Vector3(Mathf.RoundToInt(ps.transform.position.x), Mathf.RoundToInt(ps.transform.position.y), ps.transform.position.z);
                return true;
            }
            return false;
        }
        return false;
    }
    void MovementUntilCertainAmountOfSpaces()
    {
        if(spacesMoved >= spacesUntilChange)
        {
            if (upDown || leftRight)
            {
                direction *= -1;
            }
            else if (UpLeftDownRight)
            {
                if (currentDirection == Direction.Up)
                {
                    direction  = Vector2.left;
                }
                else if (currentDirection == Direction.Left)
                {
                    direction = Vector2.down;
                }
                else if (currentDirection == Direction.Down)
                {
                    direction = Vector2.right;
                }
                else if (currentDirection == Direction.Right)
                {
                    direction = Vector2.up;
                }
            }
            else if (upRightDownLeft)
            {
                if (currentDirection == Direction.Up)
                {
                    direction = Vector2.right;
                }
                else if (currentDirection == Direction.Right)
                {
                    direction = Vector2.down;
                }
                else if (currentDirection == Direction.Down)
                {
                    direction = Vector2.left;
                }
                else if (currentDirection == Direction.Left)
                {
                    direction = Vector2.up;
                }
            }
            else if (downLeftUpRight)
            {
                if (currentDirection == Direction.Down)
                {
                    direction = Vector2.left;
                }
                else if (currentDirection == Direction.Left)
                {
                    direction = Vector2.up;
                }
                else if (currentDirection == Direction.Up)
                {
                    direction = Vector2.right;
                }
                else if (currentDirection == Direction.Right)
                {
                    direction = Vector2.down;
                }
            }
            else if (downRightUpLeft)
            {
                if (currentDirection == Direction.Down)
                {
                    direction = Vector2.right;
                }
                else if (currentDirection == Direction.Right)
                {
                    direction = Vector2.up;
                }
                else if (currentDirection == Direction.Up)
                {
                    direction = Vector2.left;
                }
                else if (currentDirection == Direction.Left)
                {
                    direction = Vector2.down;
                }
            }
            spacesMoved = 0;
        }
    }

    IEnumerator Movement(Transform entity)
    {
        isMoving = true;

        startPos = entity.position;
        timeToMove = 0;

        endPos = new Vector3(startPos.x + System.Math.Sign(direction.x), startPos.y + System.Math.Sign(direction.y), startPos.z);
        endPosTrigger.transform.position = endPos;
        while (timeToMove < 1f)
        {
            timeToMove += Time.deltaTime * walkSpeed;
            entity.GetComponent<Rigidbody2D>().MovePosition(Vector3.Lerp(startPos, endPos, timeToMove));
            yield return null;
        }

        if(thisMovementType == MovementType.UntilCertainAmountOfSpaces)
        {
            spacesMoved++;
            MovementUntilCertainAmountOfSpaces();
        }

        isMoving = false;
        yield return 0;
    }

    IEnumerator ReadyBattleScene()
    {
        if (isMoving || player.GetComponent<OverworldPlayer>().isMoving)
        {
            yield return null;
        }
        noticeIndicator.SetActive(true);
        noticeSound.Play();
        yield return new WaitForSeconds(moveDelay);
        noticeIndicator.SetActive(false);

        bool vertical = false;
        bool horizontal = false;
        if(this.transform.position.x == player.transform.position.x)
        {
            vertical = true;
        }
        else if(this.transform.position.y == player.transform.position.y)
        {
            horizontal = true;
        }
        if (horizontal)
        {
            while (Mathf.Abs(transform.position.x - player.transform.position.x) > 1.5f)
            {
                if (!isMoving)
                {
                    StartCoroutine(Movement(transform));
                }
                yield return null;
            }
        }
        else if (vertical)
        {
            while (Mathf.Abs(transform.position.y - player.transform.position.y) > 1.5f)
            {
                //Debug.Log(Mathf.Abs(transform.position.y - player.transform.position.y));
                if (!isMoving)
                {
                    StartCoroutine(Movement(transform));
                }
                yield return null;
            }
        }
        //start battle scene here
        yield return new WaitForSeconds(.5f);
        transitionFade.SetTrigger("Fade");
        yield return new WaitForSeconds(.5f);

    }

}
