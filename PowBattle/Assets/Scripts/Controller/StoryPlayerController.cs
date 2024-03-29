﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TouchScript.Gestures.TransformGestures;

public class StoryPlayerController : MainPlayerController
{

    protected override void Init()
    {
        base.Init();
        SetTapLayerMask(new string[] { Common.CO.LAYER_STAGE });
    }

    protected override void Twist(float delta)
    {
        return;
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
}
