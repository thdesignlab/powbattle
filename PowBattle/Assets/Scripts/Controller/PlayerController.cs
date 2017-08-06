using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TouchScript.Gestures.TransformGestures;

public class PlayerController : GestureManager
{
    private Transform myTran;
    private Camera mainCam;
    private Transform camTran;

    [SerializeField]
    private float dragRate;
    [SerializeField]
    private float pinchRate;
    [SerializeField]
    private float twistRate;

    [SerializeField]
    private float xLimit;
    [SerializeField]
    private float zLimit;
    [SerializeField]
    private float yLimitMin;
    [SerializeField]
    private float yLimitMax;

    void Awake()
    {
        myTran = transform;
        mainCam = Camera.main;
        camTran = mainCam.transform;
    }

    protected override void Drag(float deltaX, float deltaY)
    {
        Vector3 deltaMode = myTran.forward * deltaY + myTran.right * deltaX;
        deltaMode *= dragRate;
        if (!isEnabledMove(deltaMode)) return;
        myTran.position += deltaMode;
    }
    protected override void Pinch(float delta)
    {
        Vector3 deltaMove = camTran.forward * delta * pinchRate;
        if (!isEnabledMove(deltaMove)) return;
        myTran.position += deltaMove;
    }
    protected override void Twist(float delta)
    {
        delta *= twistRate;
        if (delta > 30) delta = 30;
        myTran.Rotate(0, delta, 0);
    }


    private bool isEnabledMove(Vector3 deltaMove = default(Vector3))
    {
        Vector3 expectPos = myTran.position + deltaMove;
        //x方向制限
        if (expectPos.x < -xLimit || xLimit < expectPos.x) return false;
        //z方向制限
        if (expectPos.z < -zLimit || zLimit < expectPos.z) return false;
        //y方向制限
        if (expectPos.y < yLimitMin || yLimitMax < expectPos.y) return false;
        ////マップ内制限
        //int layerMask = LayerMask.GetMask(new string[] { Common.CO.LAYER_STAGE });
        //Ray ray = new Ray(myTran.position + deltaMove, Vector3.down);
        //RaycastHit hit;
        //if (!Physics.Raycast(ray, out hit, yLimitMax, layerMask)) return false;
        return true;
    }
}
