using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ListQuestions
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
    public int ID_listQuestions {  get; private set; }
    public string AllQuestions { get; private set; }
    public string AllAnswers { get; private set; }
    public string Notes { get; set; }
    public string DefaultQuestionCharacter { get; private set; }
    public Dictionary<int, string> DictAllQuestions { get; private set; }
    public Dictionary<int, string> DictAllAnswers { get; private set; }
    public Dictionary<int, string> DictNotes { get; private set; }

    public ListQuestions(int id, string allQuestions, string allAnswers)
    {
        // Получаем данные и объявляем словари
        ID_listQuestions = id;
        AllQuestions = allQuestions;
        AllAnswers = allAnswers;
        Notes = "";
        DefaultQuestionCharacter = "Ну, спрашивай, что ещё хочешь узнать?";
        DictAllQuestions = new Dictionary<int, string>();
        DictAllAnswers = new Dictionary<int, string>();
        DictNotes = new Dictionary<int, string>();

        // Разбор данных из типа string в Dictionary<int,string>, с помощью методов
        DictAllQuestions = getDictFromStr(allQuestions);
        DictAllAnswers = getDictFromStr(allAnswers);
    }

    public Dictionary<int, string> getDictFromStr(string dict)
    {
        Dictionary<int, string> result = new Dictionary<int, string>();
        string[] arrStr = dict.Split(';');
        foreach (var e in arrStr)
        {
            int key = result.Count;
            string value = e;
            result.Add(key, value);
        }
        return result;
    }
    public string[] getFullAnswersFromStrDict(string answer) => answer.Split("|");
}
