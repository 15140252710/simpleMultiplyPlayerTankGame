using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PlayerMove : MonoBehaviour {
    [Header("摄像机和角色")]
    public GameObject camera_eye;
    public GameObject character;
    
    private Vector3 chaRot;//角色旋转
    CharacterController cc;//角色控制器
    [Header("角色基本信息")]
    public float speed = 6.0F;
    public float height = 1.2f;

    [Header("当鼠标悬停在UI上时，是否阻止移动")]
    public bool isStopOverUI = true;

    private Vector3 moveDirection = Vector3.zero;
    [Header("重力")]
    public float gravity = 30.0F;
    [Header("可以从外部控制，开始与暂停第一人称移动")]
    public static bool isReadyWalk = true;


    [Header("方向灵敏度")]
    public float sensitivityX = 1F;
    public float sensitivityY = 1F;

    [Header("上下最大视角(Y视角)")]
    public float minimumY = -60F;
    public float maximumY = 60F;

    [Header("摄像机滑动到当前位置")]
    public bool isDoCam = false;

    float rotationY = 0F;
    float rotationX = 0f;


    void Start() {

        if (character == null) {
            character = gameObject;
        }
        if (camera_eye == null && Camera.main.gameObject != null) {
            camera_eye = Camera.main.gameObject;
        }
        else {
            //Debug.Log("请为角色添加视角摄像机");
        }

        if (character.GetComponent<CharacterController>()) {
            cc = character.GetComponent<CharacterController>();
        } else {
            cc = character.AddComponent<CharacterController>();
        }
        //very importent!
        if (isDoCam) {
            Quaternion tempQua = Quaternion.Euler(character.transform.localEulerAngles);
            camera_eye.transform.DORotateQuaternion(tempQua, 1);
            //camera_eye.transform.localEulerAngles = character.transform.localEulerAngles;
            rotationX = character.transform.localEulerAngles.y;
        }
        else {
            camera_eye.transform.localEulerAngles = character.transform.localEulerAngles;
            rotationX = character.transform.localEulerAngles.y;
        }
    }

    public void OnEnable() {
        if (isDoCam) {
            if (camera_eye == null) return;
            Quaternion tempQua = Quaternion.Euler(character.transform.localEulerAngles);
            camera_eye.transform.DORotateQuaternion(tempQua, 1);
            //camera_eye.transform.localEulerAngles = character.transform.localEulerAngles;
            rotationX = character.transform.localEulerAngles.y;
        }
        else {
            camera_eye.transform.rotation = character.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update() {
        //hm = 0; vm = 0;

        if (isReadyWalk) {
            camera_eye.transform.position = character.transform.position + new Vector3(0, height, 0);
            if (Input.GetMouseButton(1)) {

                //根据鼠标移动的快慢(增量), 获得相机左右旋转的角度(处理X)  
                rotationX = camera_eye. transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                //根据鼠标移动的快慢(增量), 获得相机上下旋转的角度(处理Y)  
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                //角度限制. rotationY小于min,返回min. 大于max,返回max. 否则返回value   
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                //总体设置一下相机角度  
                camera_eye.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

                chaRot = new Vector3(0, rotationX, 0);
                character.transform.eulerAngles = chaRot;
            }
            if (Input.GetMouseButtonDown(1)) {
                Cursor.visible = false;
            }
            if (Input.GetMouseButtonUp(1)) {
                Cursor.visible = true;
            }


            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            moveDirection.y -= gravity * Time.deltaTime;
            cc.Move(moveDirection * Time.deltaTime);
        }
        //print(camRot);
    }
}
