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

    protected BulletController bulletCtrl;
    protected List<Transform> muzzleList = new List<Transform>();

    protected override void Start()
    {
        base.Start();
        bulletCtrl = bullet.GetComponent<BulletController>();
        SetMuzzle();
    }

    protected override void AttackProcess(Transform target)
    {
        StartCoroutine(RapidShoot(target));
    }

    IEnumerator RapidShoot(Transform target)
    {
        if (muzzleList.Count == 0) yield break;

        for (int i = 1; i <= rapidCount; i++)
        {
            int muzzleNo = i % muzzleList.Count;
            if (target != null) LockOn(target, muzzleNo);
            AttackMotion(i);
            yield return new WaitForSeconds(attackWait);
            Shoot();
            yield return new WaitForSeconds(rapidInterval);
        }
        AttackMotion(0);
    }

    protected virtual void LockOn(Transform target, int muzzleNo)
    {
        Vector3 pos = target.position;
        if (shootDiff > 0)
        {
            float rate = Vector3.Distance(myTran.position, pos) / range;
            pos += muzzleList[muzzleNo].up * Random.Range(-shootDiff, shootDiff);
            pos += muzzleList[muzzleNo].right * Random.Range(-shootDiff, shootDiff);
            pos += muzzleList[muzzleNo].forward * Random.Range(-shootDiff, shootDiff);
            pos *= (rate < 0.3f) ? 0.3f : rate;
        }
        muzzleList[muzzleNo].LookAt(pos);
    }

    protected virtual GameObject Shoot(int muzzleNo = 0)
    {
        GameObject obj = Instantiate(bullet, muzzleList[muzzleNo].position, muzzleList[muzzleNo].rotation);
        SetEffectOwner(obj);
        return obj;
    }

    protected void SetEffectOwner(GameObject obj)
    {
        DamageEffectController effectCtrl = obj.GetComponent<DamageEffectController>();
        effectCtrl.SetOwner(ownerTran);
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
}
