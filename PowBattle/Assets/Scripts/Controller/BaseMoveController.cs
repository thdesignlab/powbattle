using UnityEngine;
using System.Collections;

public abstract class BaseMoveController : MonoBehaviour
{
    protected Transform myTran;

    protected virtual void Awake()
    {
        myTran = transform;
    }

    //移動
    protected void Move(Vector3 vector, float speed = 1.0f)
    {
        myTran.position += vector * speed;
    }
    protected void MoveForward(float speed)
    {
        Move(myTran.forward, speed);
    }

    //回転
    protected void Rotate()
    {

    }

    //指定ポイントへ回転
    protected bool LookTarget(Transform targetTran, float rotateSpeed, Vector3 rotateVector = default(Vector3))
    {
        if (targetTran == null) return false;
        if (rotateVector == default(Vector3)) rotateVector = Vector3.one;

        Vector3 targetPos = targetTran.position;

        //対象へのベクトル
        float x = (targetPos.x - myTran.position.x) * rotateVector.x;
        float y = (targetPos.y - myTran.position.y) * rotateVector.y;
        float z = (targetPos.z - myTran.position.z) * rotateVector.z;
        Vector3 targetVector = new Vector3(x, y, z).normalized;

        if (targetVector == Vector3.zero) return true;

        //対象までの角度
        float angleDiff = Vector3.Angle(myTran.forward, targetVector);
        // 回転角
        float angleAdd = rotateSpeed * Time.deltaTime;
        // ターゲットへ向けるクォータニオン
        Quaternion rotTarget = Quaternion.LookRotation(targetVector);
        if (angleDiff <= angleAdd)
        {
            // ターゲットが回転角以内なら完全にターゲットの方を向く
            myTran.rotation = rotTarget;
            return true;
        }
        else
        {
            // ターゲットが回転角の外なら、指定角度だけターゲットに向ける
            float t = (angleAdd / angleDiff);
            myTran.rotation = Quaternion.Slerp(myTran.rotation, rotTarget, t);
        }
        return false;
    }
}
