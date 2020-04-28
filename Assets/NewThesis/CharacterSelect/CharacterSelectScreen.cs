using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectScreen : MonoBehaviour
{

    public Image[] playerImages;

    public CursorController[] players;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < playerImages.Length; i++)
        {
            playerImages[i].enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCharacter(int playerNum, GameObject goop)
    {
        playerImages[playerNum - 1].enabled = true;
        playerImages[playerNum - 1].gameObject.GetComponent<Animator>().runtimeAnimatorController = goop.GetComponent<Animator>().runtimeAnimatorController;
    }

}
