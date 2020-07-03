using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SavingData:ScriptableObject
{
    public Vector3 PlayerTransform;
    public string sceneName;
    public SavingData() {
        try {
            sceneName = SceneManager.GetActiveScene().name;
            PlayerTransform = GameObject.FindObjectOfType<Player2D>().transform.position;
        }
        catch (System.Exception e) { }
    }

    public SavingData(Vector3 vec)
    {
        try
        {
            sceneName = SceneManager.GetActiveScene().name;
            PlayerTransform = vec;
        }
        catch (System.Exception e) { }
    }
}
