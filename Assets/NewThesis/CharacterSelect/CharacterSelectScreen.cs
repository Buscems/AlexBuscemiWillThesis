using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectScreen : MonoBehaviour
{

    public Image[] playerImages;

    public CursorController[] players;

    public GameObject readyText;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < playerImages.Length; i++)
        {
            playerImages[i].enabled = false;
        }

        readyText.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        int howManyAreReady = 0;
        for(int i = 0; i < players.Length; i++)
        {
            if (players[i].isReady)
            {
                howManyAreReady++;
            }
        }

        if(howManyAreReady >= 2)
        {
            readyText.SetActive(true);
            for(int i = 0; i < players.Length; i++)
            {
                if (players[i].myPlayer.GetButtonDown("Pause"))
                {
                    SceneManager.LoadScene("GameplayScene 1");
                }
            }
        }
        else
        {
            readyText.SetActive(false);
        }

    }

    public void SetAnimator(int playerNum, GameObject goop)
    {
        playerImages[playerNum - 1].gameObject.GetComponent<Animator>().runtimeAnimatorController = goop.GetComponent<Animator>().runtimeAnimatorController;
    }

    public void SetCharacter(int playerNum, GameObject goop)
    {
        playerImages[playerNum - 1].enabled = true;
        PlayerPrefs.SetString("Player"+playerNum+"Color", goop.name);
    }

    public void UnSetCharacter(int playerNum)
    {
        playerImages[playerNum - 1].enabled = false;
    }

}
