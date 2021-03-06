﻿using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public enum Class { Knight, Rogue, Witch }
    [HideInInspector]
    public Class currentClass;
    [HideInInspector]
    public BaseGoop thisPlayer;
    bool tierTwo;

    public float rotationSpeed;

    public GameObject magicExplosion;
    public GameObject starExplosion;

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
    public bool isKnightProjectile;

    public Color[] colors;
    [HideInInspector]
    public Color projectileColor;

    public Color[] outlineColors;

    Vector2 distanceMoved;
    Vector2 startPoint;

    public float knightDistance;
    public float rogueDistance;
    public float witchDistance;
    public float tierTwoDistance;

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
        rogueAttackCollider.enabled = false;
        witchAttackCollider.enabled = false;

        if (thisPlayer.tierTwo && !isKnightProjectile)
        {
            tierTwo = true;
        }

        if (!tierTwo)
        {
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
        }
        else
        {
            anim.SetInteger("Class", 3);
            maxDistance = tierTwoDistance;
            witchAttackCollider.enabled = true;
            transform.localScale = new Vector2(knightScale, knightScale);
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

        if (tierTwo)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

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
        if (collision.gameObject.tag == "Attack")
        {
            if (collision.GetComponent<Projectile>().playerNum != playerNum)
            {
                var temp = collision.GetComponent<Projectile>();
                if (!tierTwo)
                {
                    switch (currentClass)
                    {
                        case Class.Knight:
                            if (temp.currentClass == Class.Witch)
                            {
                                Destroy(this.gameObject);
                            }
                            break;
                        case Class.Rogue:
                            if (temp.currentClass == Class.Knight)
                            {
                                Destroy(this.gameObject);
                            }
                            break;
                        case Class.Witch:
                            if (temp.currentClass == Class.Rogue)
                            {
                                Destroy(this.gameObject);
                            }
                            break;
                    }
                }
            }
        }
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.transform.parent.GetComponent<BaseGoop>().isSpawning)
            {
                if (collision.transform.parent.GetComponent<BaseGoop>().playerNum != playerNum)
                {
                    var ps = collision.transform.parent.GetComponent<BaseGoop>();
                    if (!ps.hasBeenHit)
                    {
                        int classNumber = 0;
                        if (!tierTwo)
                        {
                            switch (currentClass)
                            {
                                case Class.Knight:
                                    classNumber = 0;
                                    break;
                                case Class.Rogue:
                                    classNumber = 1;
                                    break;
                                case Class.Witch:
                                    classNumber = 2;
                                    break;
                            }
                        }
                        else
                        {
                            classNumber = 3;
                        }
                        ps.lastPlayerThatHitThis = this.playerNum;
                        ps.StartCoroutine(ps.GetHit(classNumber));
                        ps.knockbackDirection = this.direction;
                        if (!tierTwo)
                        {
                            thisPlayer.mana += thisPlayer.manaGainPerHit;
                        }
                    }
                    Destroy(this.gameObject);
                }
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
        if(currentClass == Class.Witch && !tierTwo)
        {
            var explosion = Instantiate(magicExplosion, transform.position, Quaternion.identity);
            explosion.GetComponent<MagicExplosion>().playerNum = this.playerNum;
            explosion.GetComponent<SpriteRenderer>().color = projectileColor;
        }
        if (tierTwo)
        {
            var explosion = Instantiate(magicExplosion, transform.position, Quaternion.identity);
            explosion.GetComponent<MagicExplosion>().playerNum = this.playerNum;
            explosion.GetComponent<SpriteRenderer>().color = projectileColor;
            explosion.GetComponent<MagicExplosion>().tierTwo = true;

            var extraExplosion = Instantiate(starExplosion, transform.position, Quaternion.identity);
            var extraExplosionMain = extraExplosion.GetComponent<ParticleSystem>().main; 
            extraExplosionMain.startColor = outlineColors[thisPlayer.goopColor];

        }
    }

}
