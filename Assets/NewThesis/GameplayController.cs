using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GameplayController : MonoBehaviour
{

    public TextMeshProUGUI countdown;

    public static bool countdownOver;

    public BaseGoop[] playersInGame;
    public int amountOfPlayers;

    public GameObject[] playerUI;

    public Camera playerCamera;

    public GameObject confetti;

    [HideInInspector]
    public float distance;

    public Vector2 lowPos, highPos;
    public float[] posX;
    public float[] posY;

    public float cameraSpeed;
    public float cameraWinSpeed;

    public float cameraZoomSpeed;

    public float radius;

    public int maxAmountOfKills;

    public Animator textFade;

    public Animator canvasAnimator;

    [Header("Easing Variables")]
    public float rippleSpeed;
    public float lengthOfRipple;
    public float rippleFrequency;
    public float amplitude;

    float equationTime;
    float origScale;

    bool gameEnd;

    BaseGoop winningGoop;

    // Start is called before the first frame update
    void Start()
    {
        countdownOver = false;

        origScale = transform.localScale.x;

        confetti.SetActive(false);

        posX = new float[amountOfPlayers];
        posY = new float[amountOfPlayers];

        for(int i = 0; i < playersInGame.Length; i++)
        {
            if(i > amountOfPlayers - 1)
            {
                playersInGame[i].gameObject.SetActive(false);
                playerUI[i].SetActive(false);
            }
        }

        StartCoroutine(Countdown());
    }

    // Update is called once per frame
    void Update()
    {

        if (Cursor.visible)
        {
            Cursor.visible = false;
        }

        equationTime += Time.deltaTime * rippleSpeed;
        float equationAdd = Mathf.Exp(-equationTime * lengthOfRipple) * Mathf.Cos(rippleFrequency * Mathf.PI * equationTime) * amplitude;

        transform.localScale = new Vector2(origScale + (equationAdd / 1.8f), origScale - equationAdd);

        if (transform.localScale.y < 0)
        {
            transform.localScale = new Vector2(origScale + (equationAdd / 1.8f), 0.5f);
        }

        for(int i = 0; i < playersInGame.Length; i++)
        {
            if (playersInGame[i].currentKills >= maxAmountOfKills)
            {
                gameEnd = true;
                winningGoop = playersInGame[i];
            }
        }

        if (gameEnd)
        {
            WinState(winningGoop.transform);
        }
        else
        {
            DynamicCamera();
        }

    }

    IEnumerator Countdown()
    {
        equationTime = 0;
        countdown.text = "3";
        if (amountOfPlayers > 3)
        {
            countdown.color = playersInGame[3].basicProjectile.GetComponent<Projectile>().colors[playersInGame[3].goopColor];
        }
        yield return new WaitForSeconds(1);
        equationTime = 0;
        countdown.text = "2";
        if (amountOfPlayers > 2)
        {
            countdown.color = playersInGame[2].basicProjectile.GetComponent<Projectile>().colors[playersInGame[2].goopColor];
        }
        yield return new WaitForSeconds(1);
        equationTime = 0;
        countdown.text = "1";
        countdown.color = playersInGame[1].basicProjectile.GetComponent<Projectile>().colors[playersInGame[1].goopColor];
        yield return new WaitForSeconds(1);
        equationTime = 0;
        countdown.text = "Fight";
        countdown.color = playersInGame[0].basicProjectile.GetComponent<Projectile>().colors[playersInGame[0].goopColor];
        countdownOver = true;
        yield return new WaitForSeconds(.25f);
        textFade.SetTrigger("Fade");
    }

    void WinState(Transform player)
    {
        canvasAnimator.SetTrigger("FadeCanvas");

        if (!confetti.activeSelf)
        {
            confetti.SetActive(true);
        }

        for (int i = 0; i < amountOfPlayers; i++)
        {
            if (playersInGame[i] != winningGoop)
            {
                playersInGame[i].lost = true;
                if (!playersInGame[i].isSpawning)
                {
                    playersInGame[i].StartCoroutine(playersInGame[i].SpawnPlayer());
                }
            }
        }

        
        Vector3 playerPos = player.position;
        distance = Vector2.Distance(playerPos, Camera.main.transform.position);

        if(playerCamera.orthographicSize > 5)
        {
            playerCamera.orthographicSize -= cameraZoomSpeed * Time.deltaTime;
        }
        else
        {
            playerCamera.orthographicSize = 5;
        }

        if (Mathf.Abs(distance) >= radius)
        {
            playerPos.z = -10;
            Vector3 currentPos = Camera.main.transform.position;
            currentPos.z = -10;

            playerCamera.transform.position = Vector3.Slerp(currentPos, playerPos, cameraWinSpeed * Time.fixedDeltaTime);
        }
        

        countdown.color = winningGoop.basicProjectile.GetComponent<Projectile>().colors[winningGoop.goopColor];
        countdown.text = "Player " + winningGoop.playerNum + " Wins";
        //equationTime = 0;
        textFade.SetTrigger("FadeIn");
    }

    void DynamicCamera()
    {

        for (int i = 0; i < amountOfPlayers; i++)
        {

            posX[i] = playersInGame[i].transform.position.x;
            posY[i] = playersInGame[i].transform.position.y;

        }

        float middleX = ((posX.Max() - posX.Min()) / 2) + posX.Min();
        float middleY = ((posY.Max() - posY.Min()) / 2) + posY.Min();


        Vector3 newPos = new Vector2(middleX, middleY);

        newPos.z = -10;
        Vector3 currentPos = Camera.main.transform.position;
        currentPos.z = -10;

        playerCamera.transform.position = Vector3.Slerp(currentPos, newPos, cameraSpeed * Time.fixedDeltaTime);
        
    }

    void TurnThisOff()
    {
        //this.gameObject.SetActive(false);
    }

}
