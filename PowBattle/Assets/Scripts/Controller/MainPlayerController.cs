using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TouchScript.Gestures.TransformGestures;

public class MainPlayerController : PlayerController
{
    protected Transform spotLight;
    protected int spotLightLayerMask;
    protected Vector3 prePlayerPos;

    protected override void Init()
    {
        base.Init();
        SetTapLayerMask(new string[] { Common.CO.LAYER_UNIT });
        GameObject spotLightObj = GameObject.Find("SpotLight");
        if (spotLightObj != null) spotLight = spotLightObj.transform;
        spotLightLayerMask = LayerMask.GetMask(Common.CO.LAYER_STAGE);
    }

    //タップ
    protected override void Tap(Vector2 screenPoint)
    {
        if (Common.Func.IsPointerUI()) return;

        Ray ray = mainCam.ScreenPointToRay(screenPoint);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, myTran.position.y * 10, tapLayerMask))
        {
            SetTargetCenter(hit.transform);
        }
    }

    protected void MoveSpotLight()
    {
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, myTran.position.y * 10, spotLightLayerMask))
        {
            spotLight.position = new Vector3(hit.point.x, spotLight.position.y, hit.point.z);
        }
    }

    private void Update()
    {
        if (spotLight != null && prePlayerPos != myTran.position)
        {
            MoveSpotLight();
        }
        prePlayerPos = myTran.position;
    }
}
