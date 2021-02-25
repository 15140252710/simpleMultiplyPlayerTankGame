using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public static GlobalData instance;


    public bool isServer;
    public bool isClient;
    public string ownerPlayerName = "";

    private void Awake() {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
