using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShootWeaponController : WeaponController
{
    [SerializeField]
    protected float shootDiff;
    [SerializeField]
    protected float addHeightRangeRate;

    protected BulletController bulletCtrl;
    protected float bulletSpeed;
    protected NavMeshAgent targetAgent;

    protected override void Awake()
    {
        base.Awake();
        bulletCtrl = bullet.GetComponent<BulletController>();
        bulletSpeed = (bulletCtrl != null) ? bulletCtrl.GetSpeed() : 0;
    }
    protected override void LockOn()
    {
        if (targetTran == null) return;
        Vector3 pos = targetTran.position;
        pos += Common.Func.GetUnitMoveDiff(myTran.position, bulletSpeed, targetTran);
        pos += GetShootDiff(pos);
        myTran.LookAt(pos);
    }

    protected Vector3 GetShootDiff(Vector3 targetPos)
    {
        Vector3 diffpos = Vector3.zero;
        if (shootDiff <= 0) return diffpos;
        float rate = Vector3.Distance(myTran.position, targetPos) / range;
        diffpos += Vector3.up * Random.Range(-shootDiff, shootDiff);
        diffpos += Vector3.right * Random.Range(-shootDiff, shootDiff);
        diffpos += Vector3.forward * Random.Range(-shootDiff, shootDiff);
        diffpos *= (rate < 0.2f) ? 0.2f : rate;
        return diffpos;
    }

    public override float GetMinRange(Transform target = null)
    {
        if (target == null) return 0;

        float diffRange = Mathf.Abs(myTran.position.y - target.position.y) * 1.0f;
        return diffRange;
    }

    public override float GetMaxRange(Transform target = null)
    {
        if (addHeightRangeRate <= 0 || target == null) return range;
        float heightDiff = myTran.position.y - target.position.y;
        if (heightDiff <= 0) return range;
        float diffRange = heightDiff * (100 + addHeightRangeRate) / 100.0f;
        return range + diffRange;
    }
}
