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

    const float MIN_HIGHT = 10.0f;
    const float MAX_HIGHT = 45.0f;

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
        myTran.Rotate(0, delta * twistRate, 0);
    }


    private bool isEnabledMove(Vector3 deltaMove = default(Vector3))
    {
        //高さ制限
        float h = myTran.position.y + deltaMove.y;
        if (h < MIN_HIGHT || MAX_HIGHT < h) return false;
        //マップ内制限
        int layerMask = LayerMask.GetMask(new string[] { Common.CO.LAYER_STAGE });
        Ray ray = new Ray(myTran.position + deltaMove, Vector3.down);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, MAX_HIGHT, layerMask)) return false;
        return true;
    }
}
