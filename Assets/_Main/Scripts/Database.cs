using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class Database : MonoBehaviour
{
    private string _databaseName = "URI=file:Assets/StreamingAssets/game-database.db";

    // Нужен string объект для вкладки "Подготовка"
    public string StartingText { get; private set; }
    public string PreparationText { get; private set; }
    public string EndInterviewText { get; private set; }
    //public List<Question> AllListQuestions { get; private set; } // TODO выпилить
    public List<Quest> AllQuests { get; private set; }
    public GameObject LastPage { get; set; }
    public int SelectQuestionKey { get; set; } // TODO ??
    public Dictionary<int, int> DictKeySelectedQustion { get; set; }
    public int CountQuestionsReplacement { get; set; } // TODO выпилить
    public NavPageButtons LinkNavPageBtnGameObject { get; set; }

    // Счётчик для завершения этапа интервью (временный костыль)
    public int CountEntrace {  get; set; }
    public bool IsQuestCamOn { get; set; }
    public bool IsNewGame { get; set; } // TODO ??
    public bool IsInterview { get; set; } // TODO ??
    public bool IsEndInterview { get; set; } // TODO ??
    public Quest CurQuest { get; set; }
    // Ссылка на объект навигации меню
    public GameObject Navigation;

    public Dictionary<int, Question> AvailableQuestions; // TODO сделать полноценный объект вопроса чтобы перекидывать его через всю игру
    public Question CurQuestion;

    // Start is called before the first frame update
    // Заменил Start на Awake, чтобы сработали функции и присвоение эл-ов, для работы как с БД
    void Awake()
    {
        // Объявление
        //AllListQuestions = new List<Question>();
        AllQuests = new List<Quest>();
        IsQuestCamOn = true;
        IsNewGame = false;
        IsInterview = false;
        IsEndInterview = false;
        SelectQuestionKey = -1;

        // Временный костыль
        CountEntrace = 0;

        // Функции заполнения данных
        AddQuests();
        AddListQuestions();

        // Заполняем окна подготовки
        StartingText = "Добро пожаловать, стажёр!;" +
            "Здесь вас ждут захватывающие квесты, загадки и возможность погрузиться в атмосферу старых времён.\n\n" +
            "Приготовьтесь к незабываемым приключениям и не забывайте задавать вопросы - ведь именно так вы откроете для себя тайны этого увлекательного мира!";

        PreparationText = "Подготовка к интервью...;" +
            "Выберите вопросы, которые спросите у заказчика, чтобы узнать подробности квеста.\n\n" +
            "Перетащите выбранные вопросы из левой области, где представлены все доступные вопросы, в правую область.\n\n" +
            "Будьте внимательны! Выбирайте с умом.";

        EndInterviewText = "Завершение интервью;" +
            "Поздравляем! Вы успешно провели интервью.\n\n" +
            "Теперь вы готовы перейти к следующему этапу - заполнение брифа.";

        // Делаем 1-ый квест (по умолчанию) выбранным
        CurQuest = AllQuests[0];
    }

    void AddQuests()
    {
        var i = 0;
        using (var conn = new SqliteConnection(_databaseName)) // TODO под ORM
        { 
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Quests";
                using (var reader = cmd.ExecuteReader()) 
                {
                    while (reader.Read())
                    {
                        AllQuests.Add(new Quest(
                            AllQuests.Count,
                            (string)reader["name"],
                            "Фермер", // TODO
                            (string)reader["description"],
                            reader.GetInt32("questionCount"),
                            i++ // TODO
                            ));
                    }
                    reader.Close();
                }
            }
            conn.Close();
        }

        // Квест фермера
        /*AllQuests.Add(new Quest(
            AllQuests.Count,
            "Сбор урожая для фермера",
            "Фермер",
            "Фермер получил от предсказателя погоды предупреждение о том, что сезон дождей начнётся раньше на несколько недель.\n\n" +
            "Сам он не успеет собрать весь свой урожай до этого времени, а сын должен уехать на ярмарку с урожаем, часть из которого до сих пор не собрана.\n\n" +
            "Нужная ваша помощь.",
            "предсказателя погоды предупреждение;не успеет собрать;этого времени;часть;не собрана",
            0
            ));
        AllQuests.Add(new Quest(
            AllQuests.Count,
            "Помощь охотнику",
            "Охотник",
            "Данный квест ещё в разработке...",
            1
            ));*/
    }
    void AddListQuestions()
    {
        // Списки вопросов и ответов
        // Разделители:
        // ';' - разделение вопросов и ответов
        // '|' - разделяет все ответы на 1 вопрос, чтобы ответы выходили попорядку
        try
        {
            using (var conn = new SqliteConnection(_databaseName)) // TODO под ORM
            {
                conn.Open();
                foreach (var quest in AllQuests)
                {
                    var queDct = new Dictionary<int, Question>();
                    var queChild = new List<Question>();
                    var que = "";
                    var ans = "";
                    var id = 0;
                    var parentQueId = 0;

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT * FROM Questions WHERE questId={quest.ID_quest}";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                id = reader.GetInt32("id");
                                que = (string)reader["questionText"];
                                ans = (string)reader["answerText"];
                                ans = ans.Replace("\n", "|");
                                parentQueId = reader.GetInt32("questionId");

                                var q = new Question(id, que, ans, parentQueId);
                                queDct[id] = q;
                                if (parentQueId == -1)
                                    quest.Questions.Add(q);
                                else
                                    queChild.Add(q);
                            }
                            reader.Close();
                        }
                    }

                    foreach (var q in queChild)
                    {
                        queDct[q.ParentQuestion].ChildQuestions.Add(q);
                    }
                }
                conn.Close();
            }
        }
        catch 
        {
            throw new Exception("НЕКОРРЕКТНЫЕ ДАННЫЕ В БД");
        }

        /*AllListQuestions.Add(new ListQuestions(
            AllListQuestions.Count,
            "Какие культуры необходимо собрать?;" +
            "Есть ли какие-то особенности сбора культур?;" +
            "К какому сроку нужно успеть собрать урожай?;" +
            "Сколько нужно собрать урожая?;" +
            "Что прорицатель сказал вам о погоде в ближайшие дни?;" +
            "Что может случится во время сбора урожая?;" +
            "Как вы обычно одеваетесь на сбор урожая?;" +
            "Кто обычно помогает вам на ферме?",
            "Нужно собрать: Картофель, Томаты, Кукурузу, Тыкву и Сафлор.;" +
            "Сафлор нужно собирать с помощником-магом, хранить в тканевых мешках.|" +
            "Тыкву нельзя собирать в дождь, надо оставлять длинный хвостик, срезая ножом.|" +
            "Листья у початков кукурузы обрывать нельзя.|" +
            "Томаты срезать ножом.|" +
            "Картофель надо очистить от земли.;" +
            "Урожай надо собрать за 4 дня.|" +
            "Картофель надо собрать в первый день, сын повезёт его на ярмарку.;" +
            "Нужно собрать 100-120 вёдер картошки.|" +
            "Нужно собрать 90-100 ящиков кукурузы.|" +
            "Нужно собрать 70-80 ящиков томатов.|" +
            "Нужно собрать 30-40 мешков сафлора.|" +
            "Нужно собрать 120 тыкв.;" +
            "В какой-то день будет дождь, можно собрать томаты в теплицах.;" +
            "Можно получить солнечный удар при сборе кукурузы.|" +
            "Можно простудиться, если собирать томаты в дождливый день.|" +
            "Если уронить тыкву на ногу, можно получить ушиб.|" +
            "Также я обычно надеваю массивные ботинки во время сбора тыкв.;" +
            "Я надевая шляпу в солнечные дни и ботинки, когла собираю тыкву.;" +
            "Сын сможет помочь в первый день.|" +
            "Помощники придут на четвёртый день."
            ));
        AllListQuestions.Add(new ListQuestions(
            AllListQuestions.Count,
            "Вопрос 1;" +
            "Вопрос 2;" +
            "Вопрос 3;" +
            "Вопрос 4",
            "Ответ 1;" +
            "Ответ 2;" +
            "Ответ 3;" +
            "Ответ 4"
            ));*/
    }
    // Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
