using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffEnemies : MonoBehaviour
{

    public List<GameObject> enemies;

    public Transform playerTransform;
    [HideInInspector]
    public GameObject battlingEnemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = playerTransform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<OverworldEnemy>())
        {
            enemies.Add(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<OverworldEnemy>())
        {
            if (!enemies.Contains(collision.gameObject))
            {
                enemies.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<OverworldEnemy>())
        {
            enemies.Remove(collision.gameObject);
        }
    }

    public void TurnEnemiesOff()
    {
        if (enemies.Count > 0)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != battlingEnemy)
                {
                    enemies[i].SetActive(false);
                }
            }
        }
    }

}
