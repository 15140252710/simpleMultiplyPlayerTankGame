using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;

public class ShellExplosion : MonoBehaviour
{
    public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
    private int m_MaxDamage = 30;                    // The amount of damage done if the explosion is centred on a tank.

    public uint ShooterNetworkID;

    public Rigidbody rb;

    private void OnTriggerEnter(Collider other) {

        if (GlobalData.instance.isServer) {
            //在服务器判断打到的人，并同步伤害给各个端
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null) {
                //检测下先，是不是打到了自己 =.=
                if (ShooterNetworkID == playerScript.NetworkID) {
                    Debug.Log(ShooterNetworkID + "打到了自己 ===");
                    return;
                }

                //打到了别人
                //string ServerMessage = GameLogic.instance.GetPlayerNameByID(ShooterNetworkID) + "打到了" + GameLogic.instance.GetPlayerNameByID(playerScript.NetworkID);
                //GameLogic.instance.SendServerMessage(ServerMessage);
                //将造成伤害者，和伤害值，传递过去
                playerScript.GetDamageCallRPC(ShooterNetworkID, m_MaxDamage);
            }
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;

        // Play the particle system.
        m_ExplosionParticles.Play();

        // Destroy the shell.
        Destroy(gameObject, 1);
    }

}
