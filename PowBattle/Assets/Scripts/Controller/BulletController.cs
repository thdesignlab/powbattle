using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BulletController : DamageEffectController
{
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected bool isPenetrate;

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Update()
    {
        MoveForward(speed * Time.deltaTime);
    }

    protected override bool IsHitBreak(string hitTag)
    {
        if (!base.IsHitBreak(hitTag)) return false;
        if (isPenetrate)
        {
            switch (hitTag)
            {
                case Common.CO.TAG_OBSTACLE:
                case Common.CO.TAG_ARTILLERY_OBSTACLE:
                    return false;
            }
        }
        return true;
    }

    public float GetSpeed()
    {
        return speed;
    }
}
