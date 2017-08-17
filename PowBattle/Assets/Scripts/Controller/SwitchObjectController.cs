using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchObjectController : MonoBehaviour
{
    [SerializeField]
    protected List<GameObject> onList;
    [SerializeField]
    protected List<GameObject> offList;

    private void Start()
    {
        OnGameObject(false);
        OffGameObject(true);
    }

    public void OnOffGameObject()
    {
        OnGameObject();
        OffGameObject();
    }

    public void OnGameObject(bool flg = true)
    {
        SwitchGameObject(onList, flg);
    }

    public void OffGameObject(bool flg = false)
    {
        SwitchGameObject(offList, flg);
    }

    private void SwitchGameObject(List<GameObject> objList, bool flg)
    {
        foreach (GameObject obj in objList)
        {
            obj.SetActive(flg);
        }
    }
}
