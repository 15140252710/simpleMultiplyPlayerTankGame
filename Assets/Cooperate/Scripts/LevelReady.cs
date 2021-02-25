using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class LevelReady : MonoBehaviour
{
    public static LevelReady instance;

    public Transform MessageParent;
    public GameObject MessageItemPrefab;

    private void Awake() {
        instance = this;
    }

    public void Start() {
        if (GlobalData.instance.isServer) {
            //服务器，生成GameLogic
            NetworkManager.Instance.InstantiateGameLogic();
        }
        
        if (GlobalData.instance.isClient) {
            //客户端，生成Player
            Vector3 initPos = RandomPlayerInitPos();
            NetworkManager.Instance.InstantiatePlayer(position: initPos);
        }
    }

    public void ShowServerMessage(string message,float deleteTime=5) {
        GameObject go = Instantiate(MessageItemPrefab, MessageParent);
        MessageItem item = go.GetComponent<MessageItem>();

        item.ShowMessage(message, deleteTime);
    }

    /// <summary>
    /// 随机生成一个，玩家位置
    /// </summary>
    /// <returns></returns>
    public Vector3 RandomPlayerInitPos() {
        return new Vector3(Random.Range(0.5f, 9.5f) - 5, 1, Random.Range(0.5f, 9.5f) - 5);
    }

}
