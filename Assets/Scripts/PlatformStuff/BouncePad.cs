using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public Sound boingSFX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BouncePlayer(other.gameObject);
        }
    }

    public void BouncePlayer(GameObject objToBounce)
    {
        objToBounce.gameObject.GetComponent<PlayerController>().Bounce();
        FindObjectOfType<AudioManager>().PlaySoundInstantiate(boingSFX);
    }
}
