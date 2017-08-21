using UnityEngine;
using System.Collections;

public class LaserPointerController : MonoBehaviour
{
    [SerializeField]
    private bool IsVisibleNoTarget;
    //[SerializeField]
    private LineRenderer laser;
    [SerializeField]
    private GameObject impactPoint;
    [SerializeField]
    private float pointSize = 1;
    [SerializeField]
    private float maxLength;

    private Transform myTran;
    private Transform pointTran;
    private Transform targetTran;
    private bool isActive = false;
    private Vector3 impactScale;
    private float procTime = 0;
    private float pointScaleTime = 0.3f;
    private bool isDispPoint = false;
    private int layerMask;

    void Awake()
    {
        myTran = transform;
        laser = GetComponent<LineRenderer>();
        if (impactPoint != null) impactScale = new Vector3(pointSize, impactPoint.transform.localScale.y, pointSize);
        SetOff();
    }

    void Update()
    {
        if (isActive)
        {
            if (!BattleManager.Instance.isVisibleTarget)
            {
                SetOff();
                return;
            }

            if (maxLength <= 0) maxLength = 100.0f;
            RaycastHit hit;
            //bool isHit = Physics.Raycast(myTran.position, GetDirection(), out hit, maxLength, layerMask);
            Ray ray = new Ray(myTran.position, GetDirection());
            bool isHit = Physics.SphereCast(ray, 0.5f, out hit, maxLength, layerMask);
            //レーザー
            SetLaser(isHit, hit);
            //ポイント
            SetImpactPoint(isHit, hit);
        }
    }

    //着弾点
    private void SetImpactPoint(bool isHit, RaycastHit hit)
    {
        if (impactPoint == null) return;
        if (isHit)
        { 
            isDispPoint = true;
            if (pointTran != null)
            {
                //再表示
                if (pointTran.localScale == Vector3.zero) StartCoroutine(SetImpactPointScale());
                pointTran.position = hit.point;
            }
            else
            {
                //生成
                pointTran = Instantiate(impactPoint, hit.point, Quaternion.identity).transform;
                StartCoroutine(SetImpactPointScale());
            }
        }
        else
        {

            //非表示
            isDispPoint = false;
            if (pointTran != null) pointTran.localScale = Vector3.zero;
        }
    }
    IEnumerator SetImpactPointScale()
    {
        procTime = 0;
        for (;;)
        {
            if (!isDispPoint) break;
            procTime += Time.deltaTime;
            pointTran.localScale = Vector3.Slerp(Vector3.zero, impactScale, procTime / pointScaleTime); ;
            if (procTime / pointScaleTime >= 1) break;
            yield return null;
        }
    }

    //レーザー
    private void SetLaser(bool isHit, RaycastHit hit)
    {
        if (laser == null) return;

        //float distance = maxLength;
        Vector3 direction = myTran.forward * maxLength;
        if (isHit)
        {
            float distance = Vector3.Distance(myTran.position, hit.point);
            direction = myTran.InverseTransformDirection(hit.point - myTran.position).normalized * distance;
            //Debug.Log("my >>" + myTran.position + " / hit >>" + hit.point + " / target >> " + targetTran.position + " / direction >> " + direction);
        }
        else
        {
            direction = Vector3.zero;
        }
        //Debug.Log("direction >> "+ direction);
        laser.SetPosition(1, direction);
    }
    private Vector3 GetDirection()
    {
        //Debug.Log("GetDirection >> "+ targetTran.position +" - "+ myTran.position);
        return (targetTran == null) ? myTran.forward : targetTran.position - myTran.position;
    }

    public void SetOn(Transform target = null)
    {
        targetTran = target;
        isActive = true;
        //if (laser != null) laser.gameObject.SetActive(true);
    }

    public void SetOff()
    {
        isActive = false;
        //if (laser != null) laser.gameObject.SetActive(false);
        if (pointTran != null) Destroy(pointTran);
    }

    public void SetLayerMask(int mask)
    {
        layerMask = mask;
    }

    public void SetMaxLength(float len)
    {
        maxLength = len;
    }

    public void SetLaserColor(Color color)
    {
        if (laser == null) return;
        Material[] mats = laser.materials;
        float a = mats[0].color.a;
        mats[0].color = new Color(color.r, color.g, color.b, a);
        mats[0].SetColor("_EmissionColor", color);
        laser.materials = mats;
    }

    void OnDestroy()
    {
        SetOff();
    }
}
