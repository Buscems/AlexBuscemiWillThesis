using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameplayController : MonoBehaviour
{

    public TextMeshProUGUI countdown;

    public static bool countdownOver;

    public BaseGoop[] playersInGame;

    public Animator textFade;

    public Animator canvasAnimator;

    [Header("Easing Variables")]
    public float rippleSpeed;
    public float lengthOfRipple;
    public float rippleFrequency;
    public float amplitude;

    float equationTime;
    float origScale;

    // Start is called before the first frame update
    void Start()
    {
        countdownOver = false;

        origScale = transform.localScale.x;

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
    }

    IEnumerator Countdown()
    {
        equationTime = 0;
        countdown.text = "3";
        countdown.color = playersInGame[3].basicProjectile.GetComponent<Projectile>().colors[playersInGame[3].goopColor];
        yield return new WaitForSeconds(1);
        equationTime = 0;
        countdown.text = "2";
        countdown.color = playersInGame[2].basicProjectile.GetComponent<Projectile>().colors[playersInGame[2].goopColor];
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

    void WinCamera(int playerNumber)
    {
        canvasAnimator.SetTrigger("CanvasFade");
    }

    void TurnThisOff()
    {
        this.gameObject.SetActive(false);
    }

}
