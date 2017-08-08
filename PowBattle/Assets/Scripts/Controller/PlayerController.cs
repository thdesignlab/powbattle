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
    private int tapLayerMask;
    private Quaternion freeCamRotation;
    private int camMode;
    private Transform camTargetTran;
    private Transform camPointTran;
    private Vector3 lookAtVector;

    const string CAM_POINT = "CamPoint/";
    const string CAM_POINT_FIRST = "First";
    const string CAM_POINT_THIRD = "Third";

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

    [SerializeField]
    private Vector3 firstCamPos;
    [SerializeField]
    private Vector3 thirdCamPos;


    private void Awake()
    {
        myTran = transform;
        mainCam = Camera.main;
        camTran = mainCam.transform;
        camMode = Common.CO.CAM_MODE_FREE;
        freeCamRotation = camTran.localRotation;

        tapLayerMask = LayerMask.GetMask(Common.CO.LAYER_UNIT);

        StartCoroutine(KeyboardInput());
    }

    protected override void Drag(float deltaX, float deltaY)
    {
        if (camMode == Common.CO.CAM_MODE_FREE)
        {
            Vector3 deltaMove = myTran.forward * deltaY + myTran.right * deltaX;
            deltaMove *= dragRate;
            if (!isEnabledMove(deltaMove)) return;
            myTran.position += deltaMove;
        }
        else
        {
            lookAtVector += (myTran.up * deltaY + myTran.right * deltaX) * dragRate;
        }
    }
    protected override void Pinch(float delta)
    {
        if (camMode != Common.CO.CAM_MODE_FREE) return;

        Vector3 deltaMove = camTran.forward * delta * pinchRate;
        if (!isEnabledMove(deltaMove)) return;
        myTran.position += deltaMove;
    }
    protected override void Twist(float delta)
    {
        if (camMode != Common.CO.CAM_MODE_FREE) return;

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

    //PC用
    IEnumerator KeyboardInput()
    {
        if (!Common.Func.IsPc()) yield break;

        for (;;)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Mouse ScrollWheel");
            
            if (x != 0) Twist(x);
            if (y != 0) Pinch(y * 0.5f);
            if (h != 0) Pinch(h);
            yield return null;
        }
    }

    //タップ
    protected override void Tap(Vector2 screenPoint)
    {
        if (camMode != Common.CO.CAM_MODE_FREE) return;
        Ray ray = mainCam.ScreenPointToRay(screenPoint);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, myTran.position.y * 10, tapLayerMask))
        {
            camTargetTran = hit.transform;

            MenuController.Instance.OpenCamMenu();
        }
    }

    //カメラモード切替
    public void SwitchCameraMode(int mode)
    {
        switch (mode)
        {
            case Common.CO.CAM_MODE_FREE:
                myTran.position = new Vector3(myTran.position.x, yLimitMin, myTran.position.z);
                camTran.localRotation = freeCamRotation;
                camTargetTran = null;
                MenuController.Instance.CloseCamMenu();
                break;

            case Common.CO.CAM_MODE_FIRST:
            case Common.CO.CAM_MODE_VR:
                if (camTargetTran == null)
                {
                    SwitchCameraMode(Common.CO.CAM_MODE_FREE);
                    return;
                }
                camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_FIRST);
                if (camPointTran == null) camPointTran = camTargetTran;
                myTran.position = camPointTran.position;
                myTran.rotation = camPointTran.rotation;
                lookAtVector = camTargetTran.position;
                break;

            case Common.CO.CAM_MODE_THIRD:
                if (camTargetTran == null)
                {
                    SwitchCameraMode(Common.CO.CAM_MODE_FREE);
                    return;
                }
                camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_THIRD);
                if (camPointTran == null) camPointTran = camTargetTran;
                myTran.position = camPointTran.position;
                myTran.rotation = camPointTran.rotation;
                lookAtVector = camTargetTran.position;
                break;
        }
        camMode = mode;
    }

    private void LookAtForword()
    {
        camTran.LookAt(lookAtVector + camTargetTran.forward * 10.0f);
    }

    private void Update()
    {
        if (camMode == Common.CO.CAM_MODE_FREE) return;

        if (camTargetTran == null)
        {
            SwitchCameraMode(Common.CO.CAM_MODE_FREE);
            return;
        }
        myTran.position = camPointTran.position;
        myTran.rotation = camTargetTran.rotation;
        if (camMode == Common.CO.CAM_MODE_FIRST || camMode == Common.CO.CAM_MODE_THIRD) LookAtForword();
    }
}
