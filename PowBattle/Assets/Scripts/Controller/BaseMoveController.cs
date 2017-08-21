using UnityEngine;
using System.Collections;

public abstract class BaseMoveController : MonoBehaviour
{
    protected Transform myTran;
    protected Rigidbody myRigidbody;
    const float FORCE_REGIST = 10;
    
    protected virtual void Awake()
    {
        myTran = transform;
        myRigidbody = GetComponent<Rigidbody>();
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
    
    //指定ポイントへ回転
    protected bool LookTarget(Transform targetTran, float rotateSpeed, Vector3 rotateVector = default(Vector3))
    {
        if (targetTran == null) return false;
        return Common.Func.LookTarget(myTran, targetTran.position, rotateSpeed, rotateVector, 5.0f);
    }

    protected void Skip(Vector3 force)
    {
        if (myRigidbody == null || myRigidbody.isKinematic) return;
        force /= myRigidbody.mass;
        if (force.magnitude < FORCE_REGIST) return;
        myRigidbody.AddForce(force, ForceMode.Impulse);
    }
}
