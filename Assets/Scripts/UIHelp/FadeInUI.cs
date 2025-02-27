using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeInUI : MonoBehaviour
{
    public bool shouldIncrease = false;
    public float speedToDecrease = 1.0f;
    public float alphaCurrent = 0f;

    public bool isFadeIn = true, isFadeOut = false;

    public CanvasGroup cg;
    // Start is called before the first frame update
    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        if (isFadeIn)
        {
            alphaCurrent = 0;
            if (shouldIncrease)
            {
                StartFadeIn();
            }

            SetAlpha();
        }
        if (isFadeOut)
        {
            alphaCurrent = 1;

            if (shouldIncrease)
            {
                StartFadeOut();
            }
            SetAlpha();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (shouldIncrease)
        {
            // fades out
            if (isFadeOut)
            {
                alphaCurrent -= speedToDecrease * Time.deltaTime;
                SetAlpha();
            }
            // fades in
            else if (isFadeIn)
            {
                alphaCurrent += speedToDecrease * Time.deltaTime;
                SetAlpha();
            }
        }
    }

    // call this to start the fade in
    public void StartFadeIn()
    {
        shouldIncrease = true;
        isFadeIn = true;
    }


    // call this to fade out
    public void StartFadeOut()
    {
        shouldIncrease = true;
        isFadeOut = true;
    }

    public void SetAlpha()
    {
        cg.alpha = alphaCurrent;
        if (isFadeIn)
        {
            if(alphaCurrent >= 1)
            {
                shouldIncrease = false;
            }
        }
        if (isFadeOut)
        {
            if (alphaCurrent <= 0)
            {
                shouldIncrease = false;
            }
        }
    }
}
