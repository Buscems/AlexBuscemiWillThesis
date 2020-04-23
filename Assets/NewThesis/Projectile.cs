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

    public Color[] colors;
    [HideInInspector]
    public Color projectileColor;

    public Vector2 distanceMoved;
    public Vector2 startPoint;

    public float knightDistance;
    public float rogueDistance;

    float maxDistance;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        switch (currentClass)
        {
            case Class.Knight:
                sr.sprite = knightSprite;
                maxDistance = knightDistance;
                break;
            case Class.Rogue:
                sr.sprite = rogueSprite;
                maxDistance = rogueDistance;
                break;
            case Class.Witch:
                sr.sprite = witchSprite;
                break;
        }
        sr.color = projectileColor;
        transform.up = direction;

        startPoint = rb.position;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Boundaries")
        {
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
