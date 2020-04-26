using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplatChooser : MonoBehaviour
{

    public Sprite[] splats;
    [HideInInspector]
    public Color color;
    
    public bool isUI;
    public float sizeMin;
    public float sizeMax;
    public float sizeMinGameLayer;
    public float sizeMaxGameLayer;
    public float xMinMax;
    public float yMinMax;

    private Image img;
    private SpriteRenderer sr;

    public float waitUI;
    public float waitGameLayer;
    public float disappearSpeedUI;
    public float disappearSpeedGameLayer;

    float waitTime;
    float disappearSpeed;
    private float alpha;

    // Start is called before the first frame update
    void Start()
    {

        alpha = .75f;

        int splatIndex = Random.Range(0, splats.Length);
        if (isUI)
        {
            img = GetComponent<Image>();
            
            float size = Random.Range(sizeMin, sizeMax);
            float positionX = Random.Range(-xMinMax, xMinMax);
            float positionY = Random.Range(-yMinMax, yMinMax);

            img.sprite = splats[splatIndex];
            img.color = new Color(color.r, color.g, color.b, alpha);
            img.rectTransform.localScale = new Vector3(size, size, size);
            img.rectTransform.localPosition = new Vector3(positionX, positionY, 1);
            disappearSpeed = disappearSpeedUI;
            waitTime = waitUI;
        }
        else
        {
            sr = GetComponent<SpriteRenderer>();

            float size = Random.Range(sizeMinGameLayer, sizeMaxGameLayer);
            transform.localScale = new Vector2(size, size);
            sr.sprite = splats[splatIndex];
            sr.color = color;
            disappearSpeed = disappearSpeedGameLayer;
            waitTime = waitGameLayer;
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (isUI)
        {
            waitTime -= Time.deltaTime;
            if (waitTime < 0)
            {
                alpha -= Time.deltaTime * disappearSpeed / 2;
                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

                if(alpha < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            waitTime -= Time.deltaTime;
            if (waitTime < 0)
            {
                alpha -= Time.deltaTime * disappearSpeed / 2;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

                if (alpha < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
