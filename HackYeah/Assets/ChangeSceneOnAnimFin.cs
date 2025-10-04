using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneOnAnimFin : MonoBehaviour
{
    public void ChangeScene() {         
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainLevel");
    }
}
