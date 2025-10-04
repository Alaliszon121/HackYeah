using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTestPlatform : MonoBehaviour
{
    
    void Update()
    {
        gameObject.transform.position += new Vector3(0, 4, 0) * Time.deltaTime;   
    }
}
