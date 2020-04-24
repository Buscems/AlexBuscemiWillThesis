using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicExplosion : MonoBehaviour
{

    public int playerNum;
    public ParticleSystem particles;

    // Start is called before the first frame update
    void Start()
    {
        particles.startColor = this.GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.transform.parent.GetComponent<BaseGoop>().playerNum != playerNum)
            {
                var ps = collision.transform.parent.GetComponent<BaseGoop>();
                if (!ps.hasBeenHit)
                {
                    ps.StartCoroutine(ps.GetHit());
                    //ps.hasBeenHitByExplosion = true;
                    ps.knockbackDirection = (collision.transform.position - transform.position).normalized;
                }
            }
        }
    }

    public void DestroyThis()
    {
        Destroy(this.gameObject);
    }

}
