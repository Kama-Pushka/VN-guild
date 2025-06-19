using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Question // TODO переделать в объект Вопрос
{
    public enum StatusQuestions
    {
        Negative,
        Neutral,
        Positive
    }
    public enum BrifPoint
    {
        None,
        Purpose,
        Location,
        Resources,
        Period,
        Quality,
        Customer,
        Risks
    }

    public int ID {  get; private set; }

    public string TextQuestion { get; private set; }
    public string TextAnswer { get; private set; }
    public string Notes { get; set; }
    public int ParentQuestion { get; private set; }
    public List<Question> ChildQuestions { get; private set; }
    public List<JournalPoint> JournalPoints { get; private set; }

    public string DefaultQuestionCharacter { get; private set; } // TODO ??
    //public Dictionary<int, string> DictAllQuestions { get; private set; } // TODO ??
    //public Dictionary<int, string> DictAllAnswers { get; private set; } // TODO ??
    //public Dictionary<int, string> DictNotes { get; private set; } // TODO ??

    public Question(int id, string question, string answer, int parentQuestion, List<JournalPoint> journalPoints)
    {
        // Получаем данные и объявляем словари
        ID = id;
        TextQuestion = question;
        TextAnswer = answer;
        Notes = "";
        DefaultQuestionCharacter = "Ну, спрашивай, что ещё хочешь узнать?"; // TODO
        //DictAllQuestions = new Dictionary<int, string>();
        //DictAllAnswers = new Dictionary<int, string>();
        //DictNotes = new Dictionary<int, string>();

        // Разбор данных из типа string в Dictionary<int,string>, с помощью методов
        //DictAllQuestions = getDictFromStr(question);
        //DictAllAnswers = getDictFromStr(answer);

        ChildQuestions = new List<Question>();
        ParentQuestion = parentQuestion;
        JournalPoints = journalPoints;
    }

    //public Dictionary<int, string> getDictFromStr(string dict)
    //{
    //    Dictionary<int, string> result = new Dictionary<int, string>();
    //    string[] arrStr = dict.Split(';');
    //    foreach (var e in arrStr)
    //    {
    //        int key = result.Count;
    //        string value = e;
    //        result.Add(key, value);
    //    }
    //    return result;
    //}
    public string[] getFullAnswersFromStrDict(string answer) => answer.Split("|");
}
