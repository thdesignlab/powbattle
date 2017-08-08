using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class GestureManager : MonoBehaviour
{
    [SerializeField]
    private bool flick;
    [SerializeField]
    private bool tap;
    [SerializeField]
    private bool drag;

    [SerializeField]
    private float dragBorder = 5.0f;
    [SerializeField]
    private float pinchBorder = 0.2f;
    [SerializeField]
    private float twistBorder = 0.5f;

    private bool isDrag = false;
    private bool isPinch = false;
    private bool isTwist = false;

    protected Vector2 point = Vector2.zero;
    protected Vector2 prePoint = Vector2.zero;
    protected float totalPinch = 0;
    protected float totalTwist = 0;


    void OnEnable()
    {
        GetComponent<PressGesture>().Pressed += PressHandle;
        GetComponent<ReleaseGesture>().Released += ReleaseHandle;
        if (flick)
        {
            GetComponent<FlickGesture>().Flicked += FlickHandle;
        }
        if (tap)
        {
            GetComponent<TapGesture>().Tapped += TapHandle;
        }
        if (drag)
        {
            GetComponent<ScreenTransformGesture>().TransformStarted += TransformStartedHandle;
            GetComponent<ScreenTransformGesture>().StateChanged += StateChangedHandle;
            GetComponent<ScreenTransformGesture>().TransformCompleted += TransformCompletedHandle;
            GetComponent<ScreenTransformGesture>().Cancelled += CancelledHandle;
        }
    }

    void OnDisable()
    {
        UnsubscribeEvent();
    }

    void OnDestroy()
    {
        UnsubscribeEvent();
    }

    void UnsubscribeEvent()
    {
        GetComponent<PressGesture>().Pressed -= PressHandle;
        GetComponent<ReleaseGesture>().Released -= ReleaseHandle;
        if (flick)
        {
            GetComponent<FlickGesture>().Flicked -= FlickHandle;
        }
        if (tap)
        {
            GetComponent<TapGesture>().Tapped -= TapHandle;
        }
        if (drag)
        {
            GetComponent<ScreenTransformGesture>().TransformStarted -= TransformStartedHandle;
            GetComponent<ScreenTransformGesture>().StateChanged -= StateChangedHandle;
            GetComponent<ScreenTransformGesture>().TransformCompleted -= TransformCompletedHandle;
            GetComponent<ScreenTransformGesture>().Cancelled -= CancelledHandle;
        }
    }

    //プレス(押した時)
    protected virtual void PressHandle(object sender, System.EventArgs e)
    {
        //MyDebug.Instance.AdminLog("PressHandle");
        PressGesture gesture = sender as PressGesture;
        point = gesture.ScreenPosition;
        isDrag = false;
        isPinch = false;
        isTwist = false;
        totalPinch = 0;
        totalTwist = 0;
    }

    //リリース(離した時)
    protected virtual void ReleaseHandle(object sender, System.EventArgs e)
    {
        //MyDebug.Instance.AdminLog("ReleaseHandle");
        prePoint = point;
        point = Vector2.zero;
    }

    //タップ
    protected virtual void TapHandle(object sender, System.EventArgs e)
    {
        //MyDebug.Instance.AdminLog("TapHandle");
        TapGesture gesture = sender as TapGesture;
        Tap(gesture.ScreenPosition);
    }

    //フリック
    protected virtual void FlickHandle(object sender, System.EventArgs e)
    {
        //MyDebug.Instance.AdminLog("FlickHandle");
    }

    protected virtual void TransformStartedHandle(object sender, System.EventArgs e)
    {
        // 変形開始のタッチ時の処理
        //MyDebug.Instance.AdminLog("TransformStartedHandle");
    }

    protected virtual void StateChangedHandle(object sender, System.EventArgs e)
    {
        // 変形中のタッチ時の処理
        //Debug.Log("StateChangedHandle");
        ScreenTransformGesture gesture = sender as ScreenTransformGesture;
        Vector2 nowPoint = gesture.ScreenPosition;
        if (gesture.NumPointers == 1)
        {
            //Debug.Log(point + " >> " + gesture.ScreenPosition + "## " + gesture.DeltaPosition);
            if (dragBorder <= Vector2.Distance(point, gesture.ScreenPosition) || isDrag)
            {
                //ドラッグ
                isDrag = true;
                Drag(gesture.DeltaPosition.x, gesture.DeltaPosition.y);
            }
        }
        else if (gesture.NumPointers == 2)
        {
            totalPinch += gesture.DeltaScale - 1;
            totalTwist += gesture.DeltaRotation;
            if ((pinchBorder <= Mathf.Abs(totalPinch) || isPinch) && !isTwist)
            {
                //Debug.Log("Pinch");
                //ピンチイン・アウト
                isPinch = true;
                Pinch(gesture.DeltaScale - 1);
            }
            else if ((twistBorder <= Mathf.Abs(totalTwist) || isTwist) && !isPinch)
            {
                //Debug.Log("Twist");
                //回転
                isTwist = true;
                Twist(gesture.DeltaRotation);
            }
            //Debug.Log(gesture.DeltaScale+" ## "+ gesture.DeltaRotation);
        }
        //Debug.Log(gesture.ScreenPosition + " ## " + gesture.PreviousScreenPosition);
    }

    protected virtual void TransformCompletedHandle(object sender, System.EventArgs e)
    {
        // 変形終了のタッチ時の処理
        //MyDebug.Instance.AdminLog("TransformCompletedHandle");
    }
    protected virtual void CancelledHandle(object sender, System.EventArgs e)
    {
        // 変形終了のタッチ時の処理
        //MyDebug.Instance.AdminLog("CancelledHandle");
    }



    protected virtual void Drag(float deltaX, float deltaY)
    {
        //MyDebug.Instance.AdminLog("drag", deltaX + " / " + deltaY);
    }
    protected virtual void Pinch(float delta)
    {
        //MyDebug.Instance.AdminLog("pinch", delta);
    }
    protected virtual void Twist(float delta)
    {
        //MyDebug.Instance.AdminLog("twist", delta);
    }
    protected virtual void Tap(Vector2 screenPoint)
    {
        //MyDebug.Instance.AdminLog("Tap", screenPoint);
    }
}
