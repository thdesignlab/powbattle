﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeaponController : MonoBehaviour
{
    protected Transform myTran;
    protected Transform ownerTran;
    protected Transform targetTran;
    protected UnitMotionController motionCtrl;

    [SerializeField]
    protected float range;
    [SerializeField]
    protected float reload;
    [SerializeField]
    protected float attackWait;
    [SerializeField]
    protected float moveDelay;

    protected float leftReload = 0;

    protected virtual void Start()
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

    public bool Attack(Transform target)
    {
        if (!isEnabledAttack()) return false;
        Transform targetPoint = Common.Func.SearchChildTag(target, Common.CO.TAG_UNIT_BODY);
        if (targetPoint == null) targetPoint = target;
        AttackProcess(targetPoint);
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

    public float GetRange()
    {
        return range;
    }

    public float GetMoveDelay()
    {
        return moveDelay;
    }
}
