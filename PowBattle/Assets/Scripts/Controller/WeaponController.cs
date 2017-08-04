using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeaponController : MonoBehaviour
{
    protected Transform myTran;
    protected Transform ownerTran;
    protected Transform targetTran;

    [SerializeField]
    protected float range;
    [SerializeField]
    protected float reload;
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
        
        AttackProcess(target);
        Reload();
        return true;
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

    public float GetRange()
    {
        return range;
    }

    public float GetMoveDelay()
    {
        return moveDelay;
    }
}
