using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightSlash : MonoBehaviour
{
    [HideInInspector]
    public BaseGoop thisPlayer;

    [HideInInspector]
    public int playerNum;

    [HideInInspector]
    public Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        if (direction == Vector2.zero)
        {
            direction = Vector2.down;
        }
        transform.up = -direction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.transform.parent.GetComponent<BaseGoop>().isSpawning)
            {
                if (collision.transform.parent.GetComponent<BaseGoop>().playerNum != playerNum)
                {
                    var ps = collision.transform.parent.GetComponent<BaseGoop>();
                    if (!ps.hasBeenHit)
                    {
                        ps.hitBySlash = true;
                        ps.slashHitColor = this.GetComponent<SpriteRenderer>().color;
                        ps.lastPlayerThatHitThis = this.playerNum;
                        ps.StartCoroutine(ps.GetHit(0));
                        ps.knockbackDirection = this.direction;
                        thisPlayer.mana += thisPlayer.manaGainPerHit;
                    }
                }
            }
        }
    }

    public void SpawnSlash()
    {
        thisPlayer.KnightProjectile();
    }

    public void DestroyThisObject()
    {
        Destroy(this.gameObject);
    }

}
