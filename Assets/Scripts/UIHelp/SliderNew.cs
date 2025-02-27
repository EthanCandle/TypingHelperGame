using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderNew : MonoBehaviour
{
    public GameObject sliderBorder, sliderFill;
    public Image sliderFillImage;
    public float valueCurrent;
    public float minValue = 0, maxValue = 100;
    public bool isWholeNumbers = false;

    public bool shouldDepleteSlider = false;
    public float depleteSpeed = 1000.0f, depleteSeconds = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        GetSliderFillImage();
        SetDepleteSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldDepleteSlider)
        {
            DepleteSlider();
        }
    }

    public void GetSliderFillImage()
    {
        sliderFillImage = sliderFill.GetComponent<Image>();
    }

    public void IncreaseSlider(float amountToChangeBy)
    {
        valueCurrent += amountToChangeBy;
        SetSlider(valueCurrent);
    }

    public void DecreaseSlider(float amountToChangeBy)
    {
        valueCurrent -= amountToChangeBy;
        SetSlider(valueCurrent);
       // print($"Decrease to: {valueCurrent}");
    }

    public void DepleteSlider()
    {
        // calls itself in the update to deplete the entire bar
        shouldDepleteSlider = true;
        DecreaseSlider(depleteSpeed * Time.deltaTime);
        if(valueCurrent <= 0)
        {
            shouldDepleteSlider = false;
        }
    }

    public void FullyFillSlider()
    {

        SetSlider(maxValue);
        shouldDepleteSlider = false;
       // print($"Max Filled Value to: {valueCurrent}");
    }

    public void SetSlider(float sliderValueNew)
    {
        valueCurrent = sliderValueNew;

        // 
        if (isWholeNumbers)
        {
            valueCurrent = Mathf.Round(valueCurrent);
        }
        Mathf.Clamp(valueCurrent, minValue, maxValue);
        sliderFillImage.fillAmount = valueCurrent / maxValue;
     //   print($"Set Value to: {valueCurrent}");
    }

    public void SetDepleteSpeed()
    {
        depleteSpeed = maxValue / depleteSeconds;
    }

}
