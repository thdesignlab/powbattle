﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MagicWeaponController : WeaponController
{
    [SerializeField]
    protected GameObject bullet;
    [SerializeField]
    protected GameObject magicCircle;
    [SerializeField]
    protected int rapidCount;
    [SerializeField]
    protected float rapidInterval;
    [SerializeField]
    protected float shootDiff;
    [SerializeField]
    protected Vector3 shootPoint;
    [SerializeField]
    protected bool isLookAt;

    protected Transform groundTran;
    protected GameObject magicCircleObj;

    protected override void Awake()
    {
        base.Awake();
        if (magicCircle == null) magicCircle = Common.Func.GetEffectResource("MagicCircle");
    }

    protected override void AttackProcess(Transform target)
    {
        StartCoroutine(RapidShoot(target));
    }

    IEnumerator RapidShoot(Transform target)
    {
        SwitchMagicCircle(true);
        yield return new WaitForSeconds(attackWait);

        for (int i = 1; i <= rapidCount; i++)
        {
            if (target == null) break;
            Vector3 shootPos = GetShootPosition(target);
            Vector3 targetPos = GetTargetPosition(target);
            AttackMotion(i);
            Shoot(shootPos, targetPos);
            yield return new WaitForSeconds(rapidInterval);
        }

        SwitchMagicCircle(false);
        AttackMotion(0);
    }

    protected void SwitchMagicCircle(bool flg)
    {
        if (magicCircle == null) return;

        if (magicCircleObj == null)
        {
            magicCircleObj = Instantiate(magicCircle, groundTran.position, groundTran.rotation);
            magicCircleObj.transform.SetParent(ownerTran, true);
        }

        magicCircleObj.SetActive(flg);
    }

    protected Vector3 GetShootPosition(Transform target)
    {
        Vector3 diffpos = Vector3.zero;
        diffpos += target.up * shootPoint.y;
        diffpos += target.right * shootPoint.x;
        diffpos += target.forward * shootPoint.z;
        Vector3 shootPos = target.position + diffpos;
        return shootPos;
    }

    protected Vector3 GetTargetPosition(Transform target)
    {
        if (shootDiff <= 0) return target.position;

        Vector3 diffpos = Vector3.zero;
        float rate = Vector3.Distance(myTran.position, target.position) / range;
        diffpos += Vector3.up * Random.Range(-shootDiff, shootDiff);
        diffpos += Vector3.right * Random.Range(-shootDiff, shootDiff);
        diffpos += Vector3.forward * Random.Range(-shootDiff, shootDiff);
        diffpos *= (rate < 0.2f) ? 0.2f : rate;
        return target.position + diffpos;
    }

    protected virtual GameObject Shoot(Vector3 shootPos, Vector3 targetPos)
    {
        GameObject obj = Instantiate(bullet, shootPos, Quaternion.identity);
        if (isLookAt && shootPos != targetPos) obj.transform.LookAt(targetPos);
        DamageEffectController effectCtrl = obj.GetComponent<DamageEffectController>();
        effectCtrl.SetOwner(ownerTran);
        effectCtrl.SetDamageRate(attackRate);
        return obj;
    }

    public override void SetOwner(Transform t)
    {
        base.SetOwner(t);
        if (ownerTran != null)
        {
            groundTran = ownerTran.GetComponent<UnitController>().GetGround();
        }
    }
}
