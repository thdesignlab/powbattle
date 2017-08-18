using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeaponController : MonoBehaviour
{
    protected Transform myTran;
    protected Transform ownerTran;
    protected Transform targetTran;
    protected UnitMotionController motionCtrl;
    protected AudioManager audioMgr;
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
        audioMgr = myTran.GetComponentInChildren<AudioManager>();
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
        targetTran = target;
        attackRate = rate;
        AttackProcess();
        Reload();
        return true;
    }

    protected virtual void AttackMotion(int count)
    {
        if (motionCtrl == null) return;
        motionCtrl.Attack(count);
    }

    protected virtual void AttackProcess()
    {
        PlaySE();
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

    public virtual void SetOwner(Transform t)
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

    protected void PlaySE(int no = 0)
    {
        if (audioMgr == null) return;
        audioMgr.Play(no, true);
    }
}
