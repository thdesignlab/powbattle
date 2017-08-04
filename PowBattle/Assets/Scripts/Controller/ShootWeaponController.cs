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
        for (int i = 0; i < rapidCount; i++)
        {
            if (target != null) LockOn(target, i);
            Shoot();
            yield return new WaitForSeconds(rapidInterval);
        }
    }

    protected virtual void LockOn(Transform target, int muzzleNo)
    {
        Vector3 pos = target.position;
        if (shootDiff > 0)
        {
            pos += muzzleList[0].up * Random.Range(-shootDiff, shootDiff);
            pos += muzzleList[0].right * Random.Range(-shootDiff, shootDiff);
            pos += muzzleList[0].forward * Random.Range(-shootDiff, shootDiff);
        }
        muzzleList[0].LookAt(pos);
    }

    protected virtual void Shoot(int muzzleNo = 0)
    {
        GameObject obj = Instantiate(bullet, muzzleList[muzzleNo].position, muzzleList[muzzleNo].rotation);
        SetEffectOwner(obj);
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
