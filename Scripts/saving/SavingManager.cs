using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
public class SavingManager : MonoBehaviour
{
    public static SavingData obj;
    
    private string directorys;

    public void Local_SaveGame() {
        directorys = Application.persistentDataPath + "/ghost_Saving_data";
        Debug.Log("saving");
        obj = new SavingData();
        if (!Directory.Exists(directorys))
            Directory.CreateDirectory(directorys);
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(directorys + "/data.txt");

        var json = JsonUtility.ToJson(obj);

        formatter.Serialize(file, json);

        file.Close();

    }

    public void Local_LoadGame() {
        Debug.Log("loading");
        directorys = Application.persistentDataPath + "/ghost_Saving_data";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream f;
     if(File.Exists(directorys + "/data.txt"))
        {
            f = File.Open(directorys + "/data.txt", FileMode.Open);
            JsonUtility.FromJsonOverwrite((string)bf.Deserialize(f), obj);
            f.Close();
            if (SceneManager.GetActiveScene().name == obj.sceneName)
                GameObject.FindObjectOfType<Player2D>().transform.position = obj.PlayerTransform;
            else
                SceneManager.LoadScene(obj.sceneName);
           
        }
    }

    public static void Runtime_SaveGame() {
        obj = new SavingData();
        GameManager.save(obj);
    }

    public static void Runtime_LoadGame() {
       
        obj = GameManager.runtimeSaving.Peek(); 
        SceneManager.LoadScene(obj.sceneName);
        GameObject.FindObjectOfType<Player2D>().transform.position = obj.PlayerTransform;
    }

}
