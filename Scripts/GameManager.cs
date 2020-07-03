using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static Stack<SavingData> runtimeSaving;

    public GameObject SpawnPoint;

    public GameObject PlayerPrefab;

    public CinemachineVirtualCamera cvc;

    private void Awake()
    {  
        DontDestroyOnLoad(this.gameObject);
        if (runtimeSaving == null)
        {
            runtimeSaving = new Stack<SavingData>();
            SavingData first = new SavingData(SpawnPoint.transform.position);
            save(first);
        }
        GameObject g=Instantiate(PlayerPrefab,runtimeSaving.Peek().PlayerTransform,Quaternion.identity);
        cvc.Follow = g.GetComponent<Player2D>().cameraTarget;
        
    }
    public static void save(SavingData sd) {
        runtimeSaving.Push(sd);
    }

   
}
