using UnityEngine;
using System.Collections;

public class ObjectController : MonoBehaviour {

    [SerializeField]
    private GameObject effectSpawn;
    [SerializeField]
    private float activeLimitTime = 0;
    [SerializeField]
    private float activeLimitDistance = 0;
    [SerializeField]
    private bool isNotAutoBreak = false;

    private Transform myTran;
    private Transform ownerTran;
    private Transform targetTran;

    private float activeTime = 0;
    private float activeDistance = 0;

    private bool isEffectCustom = false;

    protected bool isDestroyProc = false;

    void Start()
    {
        myTran = transform;
        if (activeLimitTime == 0 && !isNotAutoBreak) activeLimitTime = 15;
        if (activeLimitTime > 0) StartCoroutine(CountDown());
        if (activeLimitDistance > 0) StartCoroutine(CheckDistance());
    }

    IEnumerator CountDown()
    {
        for (;;)
        {
            activeTime += Time.deltaTime;
            if (activeTime >= activeLimitTime) break;
            yield return null;
        }
        DestoryObject();
    }

    IEnumerator CheckDistance()
    {
        Vector3 prePos = myTran.position;
        for (;;)
        {
            activeDistance += Mathf.Abs(Vector3.Distance(myTran.position, prePos));
            if (activeDistance >= activeLimitDistance)
            {
                DestoryObject();
                break;
            }
            prePos = myTran.position;
            yield return null;
        }
    }

    public void DestoryObject(bool isSendRpc = false)
    {
        if (isDestroyProc) return;
        isDestroyProc = true;
        DestroyProccess();
    }

    private void DestroyProccess()
    {
        if (effectSpawn != null)
        {
            GameObject effectObj = Instantiate(effectSpawn, myTran.position, effectSpawn.transform.rotation);
        }
        Destroy(gameObject);
    }

    public void ObjectSetting(Transform owner, Transform target)
    {
        SetOwner(owner);
        SetTarget(targetTran);
    }
    public void SetOwner(Transform owner)
    {
        ownerTran = owner;
    }
    public void SetTarget(Transform target)
    {
        targetTran = target;
    }

    public void Reset()
    {
        activeTime = 0;
        activeDistance = 0;
    }
}
