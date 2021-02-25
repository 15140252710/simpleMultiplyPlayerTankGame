using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

public class GameLogic : GameLogicBehavior
{

    public static GameLogic instance { get; private set; }

    //保存所有已连接，并且正在连接中的，player
    public List<Player> playerList = new List<Player> { };
    //保存，玩家NetworkID及IP映射的字典
    private Dictionary<uint, string> PlayerIDIPDIct = new Dictionary<uint, string> { };

    private void Awake() {
        instance = this;
    }

    protected override void NetworkStart() {
        base.NetworkStart();

        if (networkObject.IsServer && NetworkManager.Instance.Networker is IServer) {

            NetWorker server = NetworkManager.Instance.Networker;

            server.playerConnected += (player, sender) => {
                Debug.Log("<color=green>Player " + player.NetworkId + " - " + player.Ip + " connected </color>" + server.Players.Count);

                PlayerIDIPDIct.Add(player.NetworkId, player.Ip);
            };

            server.playerDisconnected += (player, sender) => {
                Debug.Log("<color=red>Player " + player.NetworkId + " - " + player.Ip + " disconnected </color>");

                

                for (int i = 0; i < playerList.Count; i++) {
                    if (playerList[i].NetworkID == player.NetworkId) {
                        Player plGo = playerList[i];

                        MainThreadManager.Run(() => {
                            if (GlobalData.instance.isServer) {
                                SendServerMessage("<color=red>" + plGo.playerName + " 离开了游戏</color>");
                            }
                        });

                        playerList.RemoveAt(i);
                        plGo.networkObject.TakeOwnership();
                        plGo.networkObject.Destroy(1000);
                    }
                }

            };

        }
    }

    public void AddPlayer(Player player) {

        playerList.Add(player);

    }

    public string GetPlayerIPByID(uint id) {
        string ip;
        PlayerIDIPDIct.TryGetValue(id, out ip);
        return ip;
    }

    public string GetPlayerNameByID(uint id) {
        foreach (Player item in playerList) {
            if (item.NetworkID == id) {
                return item.playerName;
            }
        }

        return "未知姓名" + id;
    }

    public Player GetPlayerByID(uint id) {
        foreach (Player item in playerList) {
            if (item.NetworkID == id) {
                return item;
            }
        }

        return null;
    }

    public override void SendServerMessageSync(RpcArgs args) {
        LevelReady.instance.ShowServerMessage(args.GetNext<string>());
    }

    public void SendServerMessage(string message) {
        networkObject.SendRpc(RPC_SEND_SERVER_MESSAGE_SYNC, Receivers.All, message);
    }
}
