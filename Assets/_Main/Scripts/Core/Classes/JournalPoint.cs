using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JournalPoint
{
    public int ID { get; private set; }
    public string Text { get; private set; }
    public List<BriefPointItem> BriefPointItems { get; private set; }

    public JournalPoint(int id, string text, List<BriefPointItem> briefPointItems) 
    {
        ID = id;
        Text = text;
        BriefPointItems = briefPointItems;
    }
}
