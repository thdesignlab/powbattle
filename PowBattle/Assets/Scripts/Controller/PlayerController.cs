using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using TouchScript.Gestures.TransformGestures;

public class PlayerController : GestureManager
{
    protected Transform myTran;
    protected Camera mainCam;
    protected Transform camTran;
    protected int tapLayerMask;

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

    [SerializeField]
    protected float moveTime;

    protected void Awake()
    {
        myTran = transform;
        mainCam = Camera.main;
        camTran = mainCam.transform;
        Init();
        StartCoroutine(KeyboardInput());
    }

    protected virtual void Init()
    {
        return;
    }

    //タップ対象のレイヤーをセット
    protected void SetTapLayerMask(string[] layers)
    {
        tapLayerMask = LayerMask.GetMask(layers);
    }

    //ドラッグ時処理
    protected override void Drag(float deltaX, float deltaY)
    {
        Vector3 deltaMove = myTran.forward * deltaY + myTran.right * deltaX;
        deltaMove *= dragRate;
        if (!isEnabledMove(deltaMove)) return;
        myTran.position += deltaMove;
    }

    //ピンチイン・アウト時処理
    protected override void Pinch(float delta)
    {
        Vector3 deltaMove = camTran.forward * delta * pinchRate;
        if (!isEnabledMove(deltaMove)) return;
        myTran.position += deltaMove;
    }

    //ひねり時処理
    protected override void Twist(float delta)
    {
        delta *= twistRate;
        if (delta > 30) delta = 30;
        myTran.Rotate(0, delta, 0);
    }

    //移動可能判定
    protected bool isEnabledMove(Vector3 deltaMove = default(Vector3))
    {
        Vector3 expectPos = myTran.position + deltaMove;
        //x方向制限
        if (expectPos.x < -xLimit || xLimit < expectPos.x) return false;
        //z方向制限
        if (expectPos.z < -zLimit || zLimit < expectPos.z) return false;
        //y方向制限
        if (expectPos.y < yLimitMin || yLimitMax < expectPos.y) return false;
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
        return;
    }

    //カメラの中心を対象に設定
    Coroutine moveCenterCoroutine;
    public void SetTargetCenter(Transform target, UnityAction callback = null, float time = -1)
    {
        if (time < 0) time = moveTime;

        float y = yLimitMin;
        float z = y / Common.Func.Tan(camTran.localRotation.eulerAngles.x);
        float x = z * Common.Func.Sin(camTran.localRotation.eulerAngles.y);
        Vector3 centerTargetPos = target.position - myTran.forward * z - myTran.right * x + myTran.up * y;
        if (moveCenterCoroutine != null) StopCoroutine(moveCenterCoroutine);
        moveCenterCoroutine = StartCoroutine(MoveCenter(centerTargetPos, time, callback));
    }
    IEnumerator MoveCenter(Vector3 targetPos, float time, UnityAction callback = null)
    {
        if (time < 0) yield break;

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
}
