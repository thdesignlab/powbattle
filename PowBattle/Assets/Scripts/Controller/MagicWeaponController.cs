using System.Collections;
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

    protected Vector3 targetPos;
    protected Quaternion targetRot;

    protected override void Awake()
    {
        base.Awake();
        if (magicCircle == null) magicCircle = Common.Func.GetEffectResource("MagicCircle");
    }

    protected override void AttackProcess()
    {
        StartCoroutine(RapidShoot());
    }

    protected IEnumerator RapidShoot()
    {
        SwitchMagicCircle(true);
        float wait = attackWait;
        for (;;)
        {
            SetShootTarget();
            wait -= Time.deltaTime;
            if (wait <= 0) break;
            yield return null;
        }

        for (int i = 1; i <= rapidCount; i++)
        {
            SetShootTarget(true);
            Vector3 shootPos = GetShootPosition();
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

    protected void SetShootTarget(bool isCalcDiff = false)
    {
        if (targetTran == null) return;
        targetPos = targetTran.position;
        if (isCalcDiff && shootDiff > 0)
        {
            Vector3 diffpos = Vector3.zero;
            diffpos += Vector3.up * Random.Range(-shootDiff, shootDiff);
            diffpos += Vector3.right * Random.Range(-shootDiff, shootDiff);
            diffpos += Vector3.forward * Random.Range(-shootDiff, shootDiff);
            float rate = Vector3.Distance(myTran.position, targetPos) / range;
            diffpos *= (rate < 0.2f) ? 0.2f : rate;
            targetPos += diffpos;
        }
        targetRot = targetTran.rotation;

    }

    protected Vector3 GetShootPosition()
    {
        Vector3 diffpos = Vector3.zero;
        //diffpos += targetTran.up * shootPoint.y;
        //diffpos += targetTran.right * shootPoint.x;
        //diffpos += targetTran.forward * shootPoint.z;
        diffpos += targetRot * shootPoint;
        Vector3 shootPos = targetPos + diffpos;
        return shootPos;
    }

    //protected Vector3 GetTargetPosition()
    //{
    //    if (shootDiff <= 0) return targetTran.position;

    //    Vector3 diffpos = Vector3.zero;
    //    float rate = Vector3.Distance(myTran.position, targetTran.position) / range;
    //    diffpos += Vector3.up * Random.Range(-shootDiff, shootDiff);
    //    diffpos += Vector3.right * Random.Range(-shootDiff, shootDiff);
    //    diffpos += Vector3.forward * Random.Range(-shootDiff, shootDiff);
    //    diffpos *= (rate < 0.2f) ? 0.2f : rate;
    //    return targetTran.position + diffpos;
    //}

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
