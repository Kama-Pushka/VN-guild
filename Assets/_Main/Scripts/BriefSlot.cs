using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BriefSlot : MonoBehaviour
{
    public int BriefSlotID;
    public GameObject BriefPointPrefab;
    public GameObject checkboxField;

    public Database Db; // TODO ??

    public void CreateBriefPoint(int id, List<BriefPointItem> briefPointItems) 
    {
        if (checkboxField == null)
            return;

        var i = 0;
        foreach (var item in briefPointItems)
        {
            var p = Instantiate(BriefPointPrefab);
            p.name = $"point{id}-{item.BriefPointID}-{i++}";

            var tmpText = p.transform.GetComponent<Toggle>().transform.GetChild(1).GetComponent<Text>();
            tmpText.text = item.Text; // TODO BriefPointID
            p.transform.SetParent(checkboxField.transform.GetChild(0).transform);

            var t = p.GetComponent<Toggle>();
            t.isOn = false;

            
            t.onValueChanged.AddListener(on => {
                if (on)
                {
                    if (item.BriefPointID == BriefSlotID) Db.BriefPointCount++;
                    Db.FinalBrief[BriefSlotID].Add(item.Text);
                }
                else
                {
                    if (item.BriefPointID == BriefSlotID) Db.BriefPointCount--;
                    Db.FinalBrief[BriefSlotID].RemoveAt(Db.FinalBrief[BriefSlotID].IndexOf(item.Text)); // TODO !!!!!
                }
                Debug.Log(Db.BriefPointCount); 
            });

            p.transform.localPosition = new Vector3(
                p.transform.position.x,
                p.transform.position.y,
                0);
            p.transform.localScale = Vector3.one;
        }
    }

    public void RemoveBriefPoint(int id) 
    {
        if (checkboxField == null)
            return;
        
        var childCount = checkboxField.transform.GetChild(0).childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            var child = checkboxField.transform.GetChild(0).GetChild(i);
            var toggle = child.GetComponent<Toggle>();
            if (toggle != null && toggle.name.StartsWith($"point{id}-")) 
            {
                var briefPointId = Int32.Parse(child.name.Split("-")[1]);
                if (briefPointId == BriefSlotID && toggle.isOn) Db.BriefPointCount--;
                Destroy(child.gameObject);  // Уничтожаем дочерний объект  
            }
        }
    }
}
