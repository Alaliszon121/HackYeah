using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public int nextIdx = 1; // Index of the next scene to load  
    private void ChangeScene()
    {
        //Debug.Log("Game Over! Loading scene: " + nextIdx);
        SceneManager.LoadScene(nextIdx);
    }
}
