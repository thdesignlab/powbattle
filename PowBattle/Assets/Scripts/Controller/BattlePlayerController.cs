using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using TouchScript.Gestures.TransformGestures;

public class BattlePlayerController : PlayerController
{
    protected Quaternion freeCamRotation;
    protected int camMode;
    protected Transform camTargetTran;
    protected Transform camPointTran;
    protected Vector3 lookAtVector;
    protected Slider hpSlider;

    const string CAM_POINT = "CamPoint/";
    const string CAM_POINT_FIRST = "First";
    const string CAM_POINT_THIRD = "Third";


    protected override void Init()
    {
        base.Init();
        camMode = Common.CO.CAM_MODE_FREE;
        freeCamRotation = camTran.localRotation;
        SetTapLayerMask(new string[] { Common.CO.LAYER_UNIT });
    }

    protected override void Drag(float deltaX, float deltaY)
    {
        switch (camMode)
        {
            case Common.CO.CAM_MODE_FREE:
                Vector3 deltaMove = myTran.forward * deltaY + myTran.right * deltaX;
                deltaMove *= dragRate;
                if (!isEnabledMove(deltaMove)) return;
                myTran.position += deltaMove;
                break;

            case Common.CO.CAM_MODE_FIRST:
            case Common.CO.CAM_MODE_THIRD:
                lookAtVector += (myTran.up * deltaY + myTran.right * deltaX) * dragRate;
                break;
        }
    }
    protected override void Pinch(float delta)
    {
        if (camMode != Common.CO.CAM_MODE_FREE) return;
        base.Pinch(delta);
    }
    protected override void Twist(float delta)
    {
        if (camMode != Common.CO.CAM_MODE_FREE) return;
        base.Twist(delta);
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
            MenuController.Instance.OpenCamMenu(camTargetTran);
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
        lookAtVector = camTargetTran.position;
    }
    private void SetThirdCamPoint()
    {
        camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_THIRD);
        if (camPointTran == null) camPointTran = camTargetTran ;
        myTran.position = camPointTran.position;
        myTran.rotation = camPointTran.rotation;
        lookAtVector = camTargetTran.position;
    }

    //カメラ方向セット
    private void LookAtForword()
    {
        switch (camMode)
        {
            case Common.CO.CAM_MODE_FIRST:
            case Common.CO.CAM_MODE_THIRD:
                camTran.LookAt(lookAtVector + camTargetTran.forward * 10.0f);
                break;
        }
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
