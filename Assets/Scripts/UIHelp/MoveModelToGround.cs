using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveModelToGround : MonoBehaviour
{
    public GameObject modelOfSelf;
    // Start is called before the first frame update
    void Start()
    {
        // this sets the object to be itself
        modelOfSelf = gameObject;
        MoveModelToGroundPoint();
    }

    public void MoveModelToGroundPoint()
    {
        // sets models position to be above ground
        modelOfSelf.transform.position = new Vector3(modelOfSelf.transform.position.x, modelOfSelf.transform.localScale.y * 0.5f, modelOfSelf.transform.position.z);
    }
}
