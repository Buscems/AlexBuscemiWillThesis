using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public enum Class { Knight, Rogue, Witch }
    [HideInInspector]
    public Class currentClass;

    [Header("Knight Stuff")]
    public Sprite knightSprite;
    [Header("Rogue Stuff")]
    public Sprite rogueSprite;
    [Header("Witch Stuff")]
    public Sprite witchSprite;
    Rigidbody2D rb;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public Vector2 direction;
    [HideInInspector]
    public SpriteRenderer sr;
    public BoxCollider2D[] colliders;

    public Color[] colors;
    [HideInInspector]
    public Color projectileColor;

    Vector2 distanceMoved;
    Vector2 startPoint;

    public float knightDistance;
    public float rogueDistance;

    float maxDistance;
    [HideInInspector]
    public int playerNum;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        switch (currentClass)
        {
            case Class.Knight:
                sr.sprite = knightSprite;
                maxDistance = knightDistance;
                colliders[0].enabled = true;
                break;
            case Class.Rogue:
                sr.sprite = rogueSprite;
                maxDistance = rogueDistance;
                colliders[1].enabled = true;
                break;
            case Class.Witch:
                sr.sprite = witchSprite;
                colliders[2].enabled = true;
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
            Debug.Log("Yert");
            if (collision.transform.parent.GetComponent<BaseGoop>().playerNum != playerNum)
            {
                var ps = collision.transform.parent.GetComponent<BaseGoop>();
                if (!ps.hasBeenHit)
                {
                    ps.StartCoroutine(ps.GetHit());
                    ps.knockbackDirection = this.direction;
                }
            }
            Destroy(this.gameObject);
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

}
