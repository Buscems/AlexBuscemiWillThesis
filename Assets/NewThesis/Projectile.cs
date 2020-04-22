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

    public Color red;
    public Color green;
    public Color blue;
    public Color yellow;
    public Color pink;
    public Color purple;
    public Color orange;
    public Color white;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        switch (currentClass)
        {
            case Class.Knight:
                sr.sprite = knightSprite;
                break;
            case Class.Rogue:
                sr.sprite = rogueSprite;
                break;
            case Class.Witch:
                sr.sprite = witchSprite;
                break;
        }

        transform.up = direction;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
    }

}
