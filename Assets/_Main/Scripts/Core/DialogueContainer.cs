using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueContainer
{
    public GameObject root;
    // Используем TextMeshProUGUI вместо TMP_Text, так как это текст для интерфейса
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
}
