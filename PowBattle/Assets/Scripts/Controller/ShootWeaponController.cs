using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShootWeaponController : WeaponController
{
    [SerializeField]
    protected GameObject bullet;
    [SerializeField]
    protected int rapidCount;
    [SerializeField]
    protected float rapidInterval;
    [SerializeField]
    protected float shootDiff;
    [SerializeField]
    protected float addHeightRangeRate;

    protected BulletController bulletCtrl;
    protected List<Transform> muzzleList = new List<Transform>();

    protected override void Awake()
    {
        base.Awake();
        bulletCtrl = bullet.GetComponent<BulletController>();
        SetMuzzle();
    }

    protected override void AttackProcess()
    {
        StartCoroutine(RapidShoot());
    }

    protected virtual IEnumerator RapidShoot()
    {
        if (muzzleList.Count == 0) yield break;

        for (int i = 1; i <= rapidCount; i++)
        {
            LockOn();
            AttackMotion(i);
            yield return new WaitForSeconds(attackWait);

            PlaySE(i - 1);
            for (int j = 0; j < muzzleList.Count; j++)
            {
                Shoot(j);
            }
            yield return new WaitForSeconds(rapidInterval);
        }
        AttackMotion(0);
    }

    protected virtual void LockOn()
    {
        if (targetTran == null) return;
        Vector3 pos = targetTran.position;
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

    protected virtual GameObject Shoot(int muzzleNo = 0)
    {
        GameObject obj = Instantiate(bullet, muzzleList[muzzleNo].position, muzzleList[muzzleNo].rotation);
        DamageEffectController effectCtrl = obj.GetComponent<DamageEffectController>();
        effectCtrl.SetOwner(ownerTran);
        effectCtrl.SetTarget(targetTran);
        effectCtrl.SetDamageRate(attackRate);
        return obj;
    }

    protected void SetMuzzle()
    {
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE)
            {
                muzzleList.Add(child);
            }
        }
        if (muzzleList.Count == 0)
        {
            muzzleList.Add(myTran);
        }
    }

    public override float GetMinRange(Transform target = null)
    {
        if (target == null) return 0;

        float diffRange = Mathf.Abs(myTran.position.y - target.position.y) * 1.3f;
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
