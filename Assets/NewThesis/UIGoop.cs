using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGoop : MonoBehaviour
{

    public BaseGoop player;

    public SpriteRenderer sr;

    public Image thisImage;

    public Animator anim;

    public bool characterSelect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterSelect)
        {
            anim.runtimeAnimatorController = player.thisTierOneGoop.GetComponent<Animator>().runtimeAnimatorController;

            if (!player.tierTwo)
            {
                anim.SetInteger("Class", player.thisTierOneGoop.GetComponent<Animator>().GetInteger("Class"));
            }
            thisImage.sprite = sr.sprite;
        }
        else
        {
            thisImage.sprite = sr.sprite;
        }
    }
}
