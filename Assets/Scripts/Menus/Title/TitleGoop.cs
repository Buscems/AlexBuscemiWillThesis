using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleGoop : MonoBehaviour
{
    public float speed, speedMin, speedMax;
    public int color;
    public Vector3 direction;
    private GameObject spawner;

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(speedMin, speedMax);
        spawner = GameObject.FindWithTag("TitleSpawner");

        animator.SetInteger("Color", color);
       
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
