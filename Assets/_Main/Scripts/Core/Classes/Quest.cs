using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public enum Complexity
    {
        Easy,
        Normal,
        Hard
    }
    public int ID_quest { get; private set; }
    public string NameQuest { get; private set; }
    public Complexity complexityQuest { get; set; }
    public string Description { get; private set; }
    public int QuestionCount { get; private set; }
    public string CharacterName { get; private set; } // TODO у квеста может быть много заказчиков
    public string HighlightedWords { get; private set; } // TODO возможно не надо
    public int ID_ListQuestions { get; private set; } // TODO переделать?
    public Quest(int Id, string nameQuest, string characterName, string description, int questionCount, int id_ListQuestions)
    {
        ID_quest = Id;
        NameQuest = nameQuest;
        CharacterName = characterName;
        ID_ListQuestions = id_ListQuestions;

        // По умолчанию сложность Normal - Обычная
        complexityQuest = Complexity.Normal;

        QuestionCount = questionCount;

        // Описание квеста и отсутствие подсказок
        Description = description;
        HighlightedWords = null;
    }
    public Quest(int Id, string nameQuest, string characterName, string description, int questionCount, 
        string highlitedWords, int id_ListQuestions)
    {
        ID_quest = Id;
        NameQuest = nameQuest;
        CharacterName = characterName;
        ID_ListQuestions = id_ListQuestions;

        // По умолчанию сложность Normal - Обычная
        complexityQuest = Complexity.Normal;

        QuestionCount = questionCount;

        // Описание квеста и подсказки
        Description = description;
        HighlightedWords = highlitedWords;
    }
}
