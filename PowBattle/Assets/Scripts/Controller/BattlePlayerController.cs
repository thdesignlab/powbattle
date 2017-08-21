using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using TouchScript.Gestures.TransformGestures;

public class BattlePlayerController : PlayerController
{
    [SerializeField]
    protected float rotateSpeed;

    protected Quaternion freeCamRotation;
    protected int camMode;
    protected Transform camTargetTran;
    protected UnitController camTargetCtrl;
    protected Transform camPointTran;
    protected Slider hpSlider;
    protected Transform playerCanvas;
    protected Transform lockOnSight;

    const string CAM_POINT = "CamPoint/";
    const string CAM_POINT_FIRST = "First";
    const string CAM_POINT_THIRD = "Third";

    const string PLAYER_CANVAS = "PlayerCanvas";
    const string LOCK_ON_SIGHT = "LockOnSight";

    const float LOCK_ON_INTERVAL = 2.0f;
    protected float leftLockOnInterval = 0;

    protected override void Init()
    {
        base.Init();
        camMode = Common.CO.CAM_MODE_FREE;
        freeCamRotation = camTran.localRotation;
        SetTapLayerMask(new string[] { Common.CO.LAYER_UNIT });
        playerCanvas = myTran.Find(PLAYER_CANVAS);
        if (playerCanvas != null) lockOnSight = playerCanvas.Find(LOCK_ON_SIGHT);
        SetLockOnSight(false);
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
                //camLookAtVector += (myTran.up * deltaY + myTran.right * deltaX) * dragRate;
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
            camTargetCtrl = camTargetTran.GetComponent<UnitController>();
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
                break;

            case Common.CO.CAM_MODE_FIRST:
            case Common.CO.CAM_MODE_VR:
                SetFirstCamPoint();
                break;

            case Common.CO.CAM_MODE_THIRD:
                SetThirdCamPoint();
                break;
        }
        camMode = mode;
    }

    //カメラポジション設定
    private void SetFreeCamPoint()
    {
        MenuController.Instance.CloseCamMenu();
        myTran.position = new Vector3(myTran.position.x, yLimitMin, myTran.position.z);
        camTran.localRotation = freeCamRotation;
        camTargetTran = null;
        camTargetCtrl = null;
        SetLockOnSight(false);
    }
    private void SetFirstCamPoint()
    {
        if (camTargetTran == null) SwitchCameraMode(Common.CO.CAM_MODE_FREE);
        camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_FIRST);
        if (camPointTran == null) camPointTran = camTargetTran;
        myTran.position = camPointTran.position;
        myTran.rotation = camPointTran.rotation;
        camTran.rotation = myTran.rotation;
        //lookAtVector = camTargetTran.position;
    }
    private void SetThirdCamPoint()
    {
        if (camTargetTran == null) SwitchCameraMode(Common.CO.CAM_MODE_FREE);
        camPointTran = camTargetTran.Find(CAM_POINT + CAM_POINT_THIRD);
        if (camPointTran == null) camPointTran = camTargetTran ;
        myTran.position = camPointTran.position;
        myTran.rotation = camPointTran.rotation;
        //lookAtVector = camTargetTran.position;
    }

    //カメラ方向セット
    private void LookAtForword()
    {
        switch (camMode)
        {
            case Common.CO.CAM_MODE_FIRST:
            case Common.CO.CAM_MODE_THIRD:
                //camTran.LookAt(lookAtVector + camTargetTran.forward * 10.0f);
                break;
        }
    }

    protected void SetLockOnSight(bool flg, Vector3 pos = default(Vector3))
    {
        if (lockOnSight == null) return;
        lockOnSight.gameObject.SetActive(flg);
        if (pos != default(Vector3)) lockOnSight.position = pos;
    }

    protected virtual void Update()
    {
        if (BattleManager.Instance.isBattleEnd) return;
        if (camMode == Common.CO.CAM_MODE_FREE) return;

        if (camTargetTran == null)
        {
            SwitchCameraMode(Common.CO.CAM_MODE_FREE);
            return;
        }
        myTran.position = camPointTran.position;
        myTran.rotation = camTargetTran.rotation;

        Transform targetLockOnTran = camTargetCtrl.GetTarget();
        if (targetLockOnTran != null)
        {
            Vector3 lockOnScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, targetLockOnTran.position);
            SetLockOnSight(true, lockOnScreenPos);
        }
        else
        {
            SetLockOnSight(false);
        }

        if (!isDraging)
        {
            if (leftLockOnInterval > 0) leftLockOnInterval -= Time.deltaTime;
            if (leftLockOnInterval <= 0)
            {
                Vector3 lockOnPos = (targetLockOnTran != null) ? targetLockOnTran.position : camTran.TransformDirection(camTargetTran.forward * 10.0f);
                if (Common.Func.LookTarget(camTran, lockOnPos, rotateSpeed, Vector3.one, 5.0f)) leftLockOnInterval = LOCK_ON_INTERVAL;
            }
        }
    }
}
