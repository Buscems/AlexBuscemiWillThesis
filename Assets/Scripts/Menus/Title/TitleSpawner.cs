using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSpawner : MonoBehaviour
{
    public GameObject goop;
    public Vector3 spawnPosL, spawnPosR;
    public int direction, color;
    public float timer, timerMin, timerMax;

    // Start is called before the first frame update
    void Start()
    {
        timer = Random.Range(timerMin, timerMax);
        direction = Random.Range(0, 2);
        color = Random.Range(0, 3);
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if(timer <= 0){
            if(direction == 0){
                var currentGoop = Instantiate(goop, spawnPosL, Quaternion.identity);
                currentGoop.GetComponent<TitleGoop>().direction.x = 1;
                currentGoop.GetComponent<TitleGoop>().animator.SetInteger("Color", color);
                currentGoop.GetComponent<TitleGoop>().animator.SetFloat("Blend", direction);

            } else if (direction == 1)
            {
                var currentGoop = Instantiate(goop, spawnPosR, Quaternion.identity);
                currentGoop.GetComponent<TitleGoop>().direction.x = -1;
                currentGoop.GetComponent<TitleGoop>().animator.SetInteger("Color", color);
                currentGoop.GetComponent<TitleGoop>().animator.SetFloat("Blend", direction);
            }

            timer = Random.Range(timerMin, timerMax);
            direction = Random.Range(0, 2);
            color = Random.Range(0, 3);
        }
    }
}
