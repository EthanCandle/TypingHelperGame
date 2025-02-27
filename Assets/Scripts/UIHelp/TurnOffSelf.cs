using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffSelf : MonoBehaviour
{
    public void TurnOffSelfFunction()
    {
        gameObject.SetActive(false);
    }

    public void TurnOffCanvasParent()
    {
        transform.parent.gameObject.SetActive(false);
    }

    public void TurnOnSelfFunction()
    {
        gameObject.SetActive(true);
    }
}
