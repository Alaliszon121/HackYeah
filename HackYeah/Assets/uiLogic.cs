using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiLogic : MonoBehaviour
{

    public GameObject[] uiElements;
    public LivesManager livesManager;
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
       for(int i=0;i<=livesManager.bodyParts.Count;i++)
       {
        uiElements[i].SetActive(true);
       }
       for(int i=livesManager.bodyParts.Count+1;i<uiElements.Length;i++)
       {
        uiElements[i].SetActive(false);
       }
    }
}
