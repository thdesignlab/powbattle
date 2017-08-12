using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using TouchScript.Gestures.TransformGestures;

public class PlayerController : GestureManager
{
    protected Transform myTran;
    protected Camera mainCam;
    protected Transform camTran;
    protected int tapLayerMask;
    protected Quaternion freeCamRotation;
    protected int camMode;
    protected Transform camTargetTran;
    protected Transform camPointTran;
    protected Vector3 lookAtVector;

    const string CAM_POINT = "CamPoint/";
    const string CAM_POINT_FIRST = "First";
    const string CAM_POINT_THIRD = "Third";

    [SerializeField]
    protected float dragRate;
    [SerializeField]
    protected float pinchRate;
    [SerializeField]
    protected float twistRate;

    [SerializeField]
    protected float xLimit;
    [SerializeField]
    protected float zLimit;
    [SerializeField]
    protected float yLimitMin;
    [SerializeField]
    protected float yLimitMax;

    private void Awake()
    {
        myTran = transform;
        mainCam = Camera.main;
        camTran = mainCam.transform;
        Init();
        SetTapLayerMask();
        StartCoroutine(KeyboardInput());
    }

    protected virtual void Init()
    {
        camMode = Common.CO.CAM_MODE_FREE;
        freeCamRotation = camTran.localRotation;
    }

    protected virtual void SetTapLayerMask()
    {
        tapLayerMask = LayerMask.GetMask(new string[] { Common.CO.LAYER_UNIT});
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
        else if (camMode == Common.CO.CAM_MODE_THIRD)
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
        if (Common.Func.IsPointerUI()) return;
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
                SetFreeCamPoint();
                MenuController.Instance.CloseCamMenu();
                break;

            case Common.CO.CAM_MODE_FIRST:
            case Common.CO.CAM_MODE_VR:
                if (camTargetTran == null)
                {
                    SwitchCameraMode(Common.CO.CAM_MODE_FREE);
                    return;
                }
                SetFirstCamPoint();
                break;

            case Common.CO.CAM_MODE_THIRD:
                if (camTargetTran == null)
                {
                    SwitchCameraMode(Common.CO.CAM_MODE_FREE);
                    return;
                }
                SetThirdCamPoint();
                break;
        }
        camMode = mode;
    }

    //カメラポジション設定
    private void SetFreeCamPoint()
    {
        myTran.position = new Vector3(myTran.position.x, yLimitMin, myTran.position.z);
        camTran.localRotation = freeCamRotation;
        camTargetTran = null;
    }
    private void SetFirstCamPoint()
    {
            camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_FIRST);
        if (camPointTran == null) camPointTran = camTargetTran;
        myTran.position = camPointTran.position;
        myTran.rotation = camPointTran.rotation;
        camTran.rotation = myTran.rotation;
    }
    private void SetThirdCamPoint()
    {
        camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_THIRD);
        if (camPointTran == null) camPointTran = camTargetTran;
        myTran.position = camPointTran.position;
        myTran.rotation = camPointTran.rotation;
        lookAtVector = camTargetTran.position;
    }

    //カメラ方向セット
    private void LookAtForword()
    {
        switch (camMode)
        {
            case Common.CO.CAM_MODE_THIRD:
                camTran.LookAt(lookAtVector + camTargetTran.forward * 10.0f);
                break;
        }
    }

    //カメラの中心を対象に設定
    const float MOVE_TIME = 0.5f;
    Coroutine moveCenterCoroutine;
    public void SetTargetCenter(Transform target, UnityAction callback = null, float time = MOVE_TIME)
    {
        float h = yLimitMin;
        float z = h / Common.Func.Tan(camTran.localRotation.eulerAngles.x);
        Vector3 centerTargetPos = target.position - myTran.forward * z + myTran.up * h;
        if (moveCenterCoroutine != null) StopCoroutine(moveCenterCoroutine);
        moveCenterCoroutine = StartCoroutine(MoveCenter(centerTargetPos, callback));
    }
    IEnumerator MoveCenter(Vector3 targetPos, UnityAction callback = null, float time = MOVE_TIME)
    {
        if (time <= 0) time = MOVE_TIME;
        Vector3 start = myTran.position;
        float r = 0;
        for (;;)
        {
            r += Time.deltaTime / time; 
            myTran.position = Vector3.Lerp(start, targetPos, r);
            if (r >= 1) break;
            yield return null;
        }
        if (callback != null) callback.Invoke();
    }

    protected virtual void Update()
    {
        if (camMode == Common.CO.CAM_MODE_FREE) return;

        if (camTargetTran == null)
        {
            SwitchCameraMode(Common.CO.CAM_MODE_FREE);
            return;
        }
        myTran.position = camPointTran.position;
        myTran.rotation = camTargetTran.rotation;
        LookAtForword();
    }
}
