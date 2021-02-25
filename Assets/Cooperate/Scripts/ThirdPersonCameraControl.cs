using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;
//透明功能，需要Dotween，并且墙体材质需要是透明材质（即mat.DOFade可用），player身体放到Player的Tag下，可以忽略透明

public class ThirdPersonCameraControl : MonoBehaviour {

    [Tooltip("目标物体")]
    public Transform player;//

    //上下选装的最大角，与最小角
    [Header("上下旋转的最大角，与最小角")]
    [Range(-90, 90)]
    public float MaxAngelX = 65;
    [Range(-90, 90)]
    public float MinAngelX = 5;

    private float angelX = 0;
    private float angelY = 0;

    private Vector3 direction = Vector3.zero;

    [Header("缩放距离")]
    public float MaxDistance = 4;
    public float MinDistance = 1;
    public float distance = 3;

    [Header("速度控制")]//rot=5，zoom=5
    public float rotSpeed = 5f;
    public float zoomSpeed = 5f;


    [HideInInspector]
    public bool isPause = false;//外部调用，控制是否在某时，暂停鼠标的旋转控制

    public List<MeshRenderer> lastFadeRenders = new List<MeshRenderer> { };
    public List<MeshRenderer> currentFadeRenders = new List<MeshRenderer> { };
    

    void Start() {

        Mathf.Clamp(MaxAngelX, MaxAngelX, MaxAngelX);
        Mathf.Clamp(MinAngelX, MinAngelX, MinAngelX);

        ClampAngel();
        //初始化位置======================================================不需要可以删除
        angelX = 36;
        angelY = 137.25f;
        //distance = 1.7f;
    }

    void Update() {
        TransparentMaskSightRenders();
        MoveCamera();

        if (!isPause) {
            angelX -= Input.GetAxis("Mouse Y") * rotSpeed;

            angelY += Input.GetAxis("Mouse X") * rotSpeed;
            //print("X角："+angelX + "  Y角" + angelY+"  距离"+distance);//----------------------------------------------------------找点用的！
            ClampAngel();
            return;
        }


        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, MinDistance, MaxDistance);


        //打印当前位置
        /*
        if (Input.GetKeyDown(KeyCode.Space)) {
            print("X角：" + angelX + "  Y角" + angelY + "  距离" + distance);
        }
		/*/

    }

    private void TransparentMaskSightRenders() {
        //如果有之前，把之前的设为正常
        if (lastFadeRenders.Count > 0) {
            foreach (MeshRenderer renderer in lastFadeRenders) {
                renderer.materials[0].DOFade(1, 0.3f);
            }
        }
        lastFadeRenders.Clear();

        //反向打射线，从玩家到摄像机的方向
        Vector3 dir = -(player.position - transform.position).normalized;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(player.position, dir, Vector3.Distance(player.position, transform.position));
        Debug.DrawLine(player.transform.position, transform.position, Color.red);

        //foreach (RaycastHit hit1 in hits) {
        //    Debug.Log(hit1.transform.name);
        //}

        //保存当前，
        foreach (RaycastHit hit in hits) {
            MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
            if (mr != null && mr.transform.tag != "Player") currentFadeRenders.Add(mr);
        }

        //透明当前
        foreach (MeshRenderer renderer in currentFadeRenders) {
            renderer.materials[0].DOFade(0, 0.3f);
        }

        //将当前，设置为之前
        
        foreach (MeshRenderer renderer in currentFadeRenders) {
            lastFadeRenders.Add(renderer);
        }
        currentFadeRenders.Clear();
    }

    private void MoveCamera() {
        //奎特尼恩
        Quaternion q1 = Quaternion.Euler(angelX, angelY, 0);
        //只是一个方向而已
        direction = new Vector3(0, 0, -distance);
        //只是一个位置而已
        transform.position = player.position + q1 * direction;
        //只是一个注视而已
        transform.LookAt(player.position);
        //
        //target.rotation = transform.rotation;
    }

    private void ClampAngel() {
        if (angelX < MinAngelX) {
            angelX = MinAngelX;
        }
        if (angelX > MaxAngelX) {
            angelX = MaxAngelX;
        }
    }
    //旋转到某个位置
    public void MoveToRotation(float x1, float y1, float doDistance=-1,float delayTime=1) {//x和y为角度，distance为距离
        DOTween.To(() => angelX, x => angelX = x, x1, delayTime);
        DOTween.To(() => angelY, y => angelY = y, y1, delayTime);
        if (distance > 0) {
            DOTween.To(() => distance, z => distance = z, doDistance, delayTime);
        }
    }

}
