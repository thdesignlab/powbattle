using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeaponController : MonoBehaviour
{
    protected Transform myTran;
    protected Transform ownerTran;
    protected UnitMotionController motionCtrl;
    protected float maxRange;
    protected float minRange;

    [SerializeField]
    protected float range;
    [SerializeField]
    protected float reload;
    [SerializeField]
    protected float attackWait;
    [SerializeField]
    protected float moveDelay;

    protected float leftReload = 0;
    protected int attackRate = 0;

    protected virtual void Awake()
    {
        myTran = transform;
    }

    protected virtual void Update()
    {
        if (leftReload > 0)
        {
            leftReload -= Time.deltaTime;
        }
    }

    public bool Attack(Transform target, int rate = 0)
    {
        if (!isEnabledAttack()) return false;
        //Transform targetPoint = Common.Func.SearchChildTag(target, Common.CO.TAG_UNIT_BODY);
        //if (targetPoint == null) targetPoint = target;
        attackRate = rate;
        AttackProcess(target);
        Reload();
        return true;
    }

    protected virtual void AttackMotion(int count)
    {
        if (motionCtrl == null) return;
        motionCtrl.Attack(count);
    }

    protected virtual void AttackProcess(Transform target)
    {
        return;
    }

    public virtual bool isEnabledAttack()
    {
        if (leftReload > 0) return false;
        return true;
    }

    protected virtual void Reload()
    {
        leftReload = reload;
    }

    public void SetOwner(Transform t)
    {
        ownerTran = t;
    }

    public void SetMotionCtrl(UnitMotionController ctrl)
    {
        motionCtrl = ctrl;
    }

    public float GetReload()
    {
        return reload;
    }

    public virtual float GetMinRange(Transform target = null)
    {
        return 0;
    }

    public virtual float GetMaxRange(Transform target = null)
    {
        return range;
    }

    public float GetMoveDelay()
    {
        return moveDelay;
    }

    //射程内判定
    public bool IsWithinRange(Transform target, float distance = -1)
    {
        if (distance < 0) distance = Vector3.Distance(myTran.position, target.position);
        return (GetMinRange(target) <= distance && distance <= GetMaxRange(target));
    }
}
