using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public enum Class { Knight, Rogue, Witch }
    [HideInInspector]
    public Class currentClass;

    public GameObject magicExplosion;

    Animator anim;
    Rigidbody2D rb;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public Vector2 direction;
    [HideInInspector]
    public SpriteRenderer sr;
    public PolygonCollider2D knightAttackCollider;
    public BoxCollider2D rogueAttackCollider;
    public CircleCollider2D witchAttackCollider;

    public float knightScale;

    public Color[] colors;
    [HideInInspector]
    public Color projectileColor;

    Vector2 distanceMoved;
    Vector2 startPoint;

    public float knightDistance;
    public float rogueDistance;
    public float witchDistance;

    float maxDistance;
    [HideInInspector]
    public int playerNum;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        knightAttackCollider.enabled = false;
        rogueAttackCollider.enabled = true;
        witchAttackCollider.enabled = false;

        switch (currentClass)
        {
            case Class.Knight:
                anim.SetInteger("Class", 0);
                maxDistance = knightDistance;
                knightAttackCollider.enabled = true;
                transform.localScale = new Vector2(knightScale, knightScale);
                break;
            case Class.Rogue:
                anim.SetInteger("Class", 1);
                maxDistance = rogueDistance;
                rogueAttackCollider.enabled = true;
                break;
            case Class.Witch:
                anim.SetInteger("Class", 2);
                maxDistance = witchDistance;
                witchAttackCollider.enabled = true;
                break;
        }
        sr.color = projectileColor;
        if(direction == Vector2.zero)
        {
            direction = Vector2.down;
        }
        transform.up = direction;

        startPoint = rb.position;

        StartCoroutine(DestroyObject());

    }

    // Update is called once per frame
    void Update()
    {
        distanceMoved = rb.position - startPoint;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Boundaries")
        {
            Destroy(this.gameObject);
        }
        if (collision.gameObject.tag == "Player")
        {
            if (collision.transform.parent.GetComponent<BaseGoop>().playerNum != playerNum)
            {
                var ps = collision.transform.parent.GetComponent<BaseGoop>();
                if (!ps.hasBeenHit)
                {
                    ps.StartCoroutine(ps.GetHit());
                    ps.knockbackDirection = this.direction;
                }
                Destroy(this.gameObject);
            }
        }
    }

    IEnumerator DestroyObject()
    {
        while(distanceMoved.magnitude < maxDistance)
        {
            yield return null;
        }
        Destroy(this.gameObject);

    }

    private void OnDestroy()
    {
        if(currentClass == Class.Witch)
        {
            var explosion = Instantiate(magicExplosion, transform.position, Quaternion.identity);
            explosion.GetComponent<MagicExplosion>().playerNum = this.playerNum;
            explosion.GetComponent<SpriteRenderer>().color = projectileColor;
        }
    }

}
