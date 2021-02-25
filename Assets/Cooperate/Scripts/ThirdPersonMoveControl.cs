using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMoveControl : MonoBehaviour
{
    public CharacterController cc;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 chaRot;//角色旋转

    public float gravity = 30.0F;
    public float moveSpeed = 1.0F;

    public GameObject FollowCamera;

    private void Start() {
        //验证
        if (cc == null) cc = GetComponent<CharacterController>();
        if (cc == null) cc = gameObject.AddComponent<CharacterController>();
        //
        if (FollowCamera == null) FollowCamera = Camera.main.gameObject;
    }

    private void Update() {

        //如果有移动，则player旋转，跟随摄像机视角 
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
            chaRot = new Vector3(0, FollowCamera.transform.localEulerAngles.y, 0);
            transform.eulerAngles = chaRot;
        }

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;
        moveDirection.y -= gravity * Time.deltaTime;
        cc.Move(moveDirection * Time.deltaTime);

    }
}
