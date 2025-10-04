using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenes : MonoBehaviour
{
    public int sceneID = 0;
    public void LoadScene()
    {
        Debug.Log("sceneID to load: " + sceneID);
        SceneManager.LoadScene(sceneID);
    }
}