using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    // [SerializeField] - нужен для отображения в Instance части Unity, нашего объекта + присваивание ссылок

    // Тут мы решили сделать поля доступные и получить к ним доступ
    // было [SerializeField] private
    // сделано public (Это не безопастно)
    public DialogueContainer dialogueContainer = new DialogueContainer();

    // Создаём одноэлементную систему управления диалогами
    public static DialogueSystem instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            DestroyImmediate(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
