using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine.UI;

public class Player : PlayerBehavior
{
    private MeshRenderer meshRenderer;
    public Color bodyColor;
    public float moveSpeed = 3;
    public Text text_ownerName;
    public string playerName { get; set; }

    public CharacterController cc;
    private Vector3 moveDirection = Vector3.zero;
    public float gravity = 30.0F;
    private Vector3 chaRot;//角色旋转

    public GameObject FollowCamera;

    //发射炮弹
    public GameObject Shell;
    public Transform shootPoint;
    public float shellFireRate = 1;
    private float fireTimeSinceLast = 0;

    //血量
    private float maxBlood = 100;//总血量
    public float currentBlood;//当前血量
    public Slider bloodSlider;//显示血量的，滑动条
    public Image fillImage;//填充的颜色条，用于制作根据血量变色
    private bool isDead = false;//标记是否已失死亡（死亡后不可移动及发射炮弹）

    public uint NetworkID { get; private set; }
    public string IP { get; private set; }


    private void Start() {
        meshRenderer = transform.Find("body").GetComponent<MeshRenderer>();

        if (cc == null) cc = GetComponent<CharacterController>();
        if (FollowCamera == null) FollowCamera = Camera.main.gameObject;

        InitBlood();
    }

    protected override void NetworkStart() {
        base.NetworkStart();

        //在生成本player时，首先随机生产一个颜色
        Color color = new Color((float)Random.Range(0, 100) / 100, (float)Random.Range(0, 100) / 100, (float)Random.Range(0, 100) / 100);

        if (networkObject.IsOwner) {
            //发送身体颜色
            networkObject.SendRpc(RPC_SET_BODY_COLOR, Receivers.AllBuffered, networkObject.MyPlayerId, color);
            //发送自己的名字
            networkObject.SendRpc(RPC_SET_OWNER_NAME, Receivers.AllBuffered, GlobalData.instance.ownerPlayerName);
            //发送自己的网络ID
            SetNetworkID();
            //
            ThirdPersonCameraControl tpcc = FollowCamera.gameObject.AddComponent<ThirdPersonCameraControl>();
            tpcc.player = this.transform;
        }
        if (networkObject.IsServer) {
            AddPlayer();
        }
        

    }

    //添加一个player
    public void AddPlayer() {
        GameLogic.instance.AddPlayer(this);
    }

    private void Update() {

        if (networkObject == null) {
            return;
        }

        if (!networkObject.IsOwner) {
            transform.position = networkObject.pos;
            transform.rotation = networkObject.rot;
            return;
        }

        //根据鼠标移动的快慢(增量), 获得相机左右旋转的角度(处理X)  
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
            chaRot = new Vector3(0, FollowCamera.transform.localEulerAngles.y, 0);
            transform.eulerAngles = chaRot;
        }

        if (isDead) return;

        //角色移动
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;
        moveDirection.y -= gravity * Time.deltaTime;
        cc.Move(moveDirection * Time.deltaTime);

        //
        fireTimeSinceLast += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (fireTimeSinceLast > shellFireRate) {
                //开炮！
                networkObject.SendRpc(RPC_FIRE_SHELL, Receivers.All, NetworkID);
                //Fire();
            }
        }


        //发送自己的状态到网络
        networkObject.pos = transform.position;
        networkObject.rot = transform.rotation;

    }

    public void Fire(uint shooterNetworkID) {
        //重置，到上次开炮经历的时间
        fireTimeSinceLast = 0;
        //实例化一个炮弹，
        GameObject shell = Instantiate(Shell, shootPoint.position, shootPoint.rotation);
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        shellRb.AddForce(shootPoint.forward * 20, ForceMode.Impulse);
        //记录，炮弹发射者的ID
        ShellExplosion shellScript = shell.GetComponent<ShellExplosion>();
        shellScript.ShooterNetworkID = shooterNetworkID;
    }

    public void SetNetworkID() {
        uint id = networkObject.MyPlayerId;
        networkObject.SendRpc(RPC_SET_NETWORK_I_D_SYNC, Receivers.All, id);
    }
    

    /// <summary>
    /// 初始化血条
    /// </summary>
    private void InitBlood() {
        //初始化数值
        bloodSlider.maxValue = maxBlood;
        bloodSlider.minValue = 0;

        //初始化血条
        bloodSlider.value = bloodSlider.maxValue;
        currentBlood = maxBlood;

        //初始化颜色，为满血时的颜色
        fillImage.color = CalculateColrByRate(1);
    }

    public void GetDamage(uint shooterID, int damage) {
        //先减血
        currentBlood -= damage;

        //检测血量
        if (currentBlood <= 0) {
            currentBlood = 0;
        }

        //显示当前血量
        bloodSlider.value = currentBlood;
        fillImage.color = CalculateColrByRate(((float)currentBlood) / maxBlood);

        //死掉，并重生
        if (currentBlood <= 0) {
            networkObject.SendRpc(RPC_DEAD_SYNC, Receivers.All);

            //在服务器，发送死亡信息
            if (GlobalData.instance.isServer) {
                string message = GameLogic.instance.GetPlayerNameByID(shooterID) + "拿到了" + playerName + "的赏金";
                GameLogic.instance.SendServerMessage(message);
            }
        }
    }

    public void DeadRelife() {
        StartCoroutine(Relife_IE());
    }

    IEnumerator Relife_IE() {
        isDead = true;//标记为已死亡
        HidePlayer();//隐藏玩家
        yield return new WaitForSeconds(3);//3秒后重生
        isDead = false;//标记为已重生
        //随机生成到新位置========================
        transform.position = LevelReady.instance.RandomPlayerInitPos();//随机一个新位置
        InitBlood();//初始化血条
        ShowPlayer();//显示玩家
    }

    //隐藏玩家
    private void HidePlayer() {
        transform.localScale = Vector3.zero;
    }
    
    //显示玩家
    private void ShowPlayer() {
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 根据比例（血量百分比），返回颜色（满血绿色，没血红色）
    /// </summary>
    private Color CalculateColrByRate(float rate) {
        float g = 255 * rate;//百分比越高，越偏向绿色
        float r = 255 - g;//红色补差，绿色少于255的值，用红色补
        return new Color(r/255, g/255, 0);//返回0-1的色值
    }


    public void GetDamageCallRPC(uint shooterID, int damage) {
        networkObject.SendRpc(RPC_GET_DAMAGE_SYNC, Receivers.All, shooterID, damage);
    }

    #region RPCs

    public override void SetBodyColor(RpcArgs args) {
        uint id = args.GetNext<uint>();
        Color color = args.GetNext<Color>();
        meshRenderer.materials[0].color = color;
    }

    public override void SetOwnerName(RpcArgs args) {
        string ownerName = args.GetNext<string>();

        text_ownerName.text = ownerName;
        playerName = ownerName;
        gameObject.name = ownerName;

        if (GlobalData.instance.isServer) {
            GameLogic.instance.SendServerMessage("<color=#04E006>" + ownerName + " 加入游戏</color>");
        }
    }


    //在服务器，获取，当前player的网络ID
    public override void SetNetworkIDSync(RpcArgs args) {
        //ID
        NetworkID = args.GetNext<uint>();
        //IP
        IP = GameLogic.instance.GetPlayerIPByID(NetworkID);
    }


    public override void FireShell(RpcArgs args) {
        //发射炮弹，并接收发射者的ID
        Fire(args.GetNext<uint>());
    }

    public override void GetDamageSync(RpcArgs args) {
        //挨打，并接收自己挨打的信息
        GetDamage(args.GetNext<uint>(), args.GetNext<int>());
    }

    public override void DeadSync(RpcArgs args) {
        //接收，死亡指令
        DeadRelife();
    }

    #endregion
}
