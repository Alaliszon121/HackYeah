using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTestPlatform1 : MonoBehaviour
{
    
    void Update()
    {
        gameObject.transform.position += new Vector3(0, -5, 0) * Time.deltaTime;   
    }
}
