using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class BriefPointItem
{
    public int BriefPointID { get; private set; }
    public string Text { get; private set; }

    public BriefPointItem(int briefPointID, string text) 
    {
        BriefPointID = briefPointID;
        Text = text;
    }
}
