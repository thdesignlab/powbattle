using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TouchScript.Gestures.TransformGestures;

public class StoryPlayerController : PlayerController
{

    protected override void Init()
    {
        return;
    }

    protected override void SetTapLayerMask()
    {
        tapLayerMask = LayerMask.GetMask(new string[] { Common.CO.LAYER_STAGE});
    }

    protected override void Twist(float delta)
    {
        return;
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
        if (Common.Func.IsPointerUI()) return;

        Ray ray = mainCam.ScreenPointToRay(screenPoint);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, myTran.position.y * 10, tapLayerMask))
        {
            StoryManager.Instance.SelectStage(hit.transform);
        }
    }

    protected override void Update()
    {
        return;
    }
}
