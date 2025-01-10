using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;

// Чтобы это не касалось нашего проекта, мы отделяем через namespace, наш набор тестов
// (создаём изоляцию)
namespace Testing
{
    public class Testing_Architect : MonoBehaviour
    {
        // Получаем ссылку на Бд и 2 текстовых блока
        [SerializeField]
        GameObject LinkDatabase;
        [SerializeField]
        GameObject LinkHead;
        [SerializeField]
        GameObject LinkContent;
        [SerializeField]
        GameObject LinkListQuestions;
        Database Db { get; set; }
        ListQuestions CurListQuestions { get; set; }
        TextMeshProUGUI Head { get; set; }
        TextMeshProUGUI Content { get; set; }
        // Получаем доступ к диалоговой системе, через DialogueSystem
        DialogueSystem ds;

        // Так как мы хотим создать текстового архитектора, то мы создаём переменную
        TextArchitect architect;

        // public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.typewriter;
        public TextArchitect.BuildMethod bm;

        // Uid списка квестов и счётчик
        int KeyListQuestions {  get; set; }

        // Счётчик для прохождения по ответам
        int StartKeyAnswer { get; set; }

        // Флаг для добавления выбранных вопросов в список, без цикличности (как ограничитель)
        bool IsAddSelectedKey { get; set; }
        // Строка для сравнения последнего полученного текста
        public string LastContentText { get; set; }
        //Dictionary<int, int> DictSelectedQuestions { get; set; }

        // Для тестирования
        public bool CheckIf = false;

        // Start is called before the first frame update
        void Start()
        {
            Db = LinkDatabase.GetComponent<Database>();
            Head = LinkHead.GetComponent<TextMeshProUGUI>();
            Content = LinkContent.GetComponent<TextMeshProUGUI>();

            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.typewriter;
            //architect.speed = 0.5f;

            // Dictionary выбранных вопросов, чтобы появлялись следующие, из обекта ListQuestions
            Db.DictKeySelectedQustion = new Dictionary<int, int>();
            Db.CountQuestionsReplacement = 0;

            // Счётчик для нажатия клавиши Space (Вывод ответа на вопрос в опр. порядке)
            StartKeyAnswer = 0;

            // По умолчанию false
            IsAddSelectedKey = false;

            // Обозначим Content по умолчанию
            setDefaultContent();
        }

        // Update is called once per frame
        void Update()
        {
            // Обновление первичного текста на странице диалога
            Head.text = Db.CurQuest.CharacterName;

            // Берём внешний ключ текущего квеста
            KeyListQuestions = Db.CurQuest.ID_ListQuestions;

            // Получаем список вопросов с помощью KeyListQuestions
            CurListQuestions = Db.AllListQuestions[KeyListQuestions];

            // Заполняем раздел вопросов (макс 4)
            for (int i = 0; i < 4; i++)
            {
                GameObject btn = LinkListQuestions.transform.GetChild(i).gameObject;
                TextMeshProUGUI tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                
                // Первичная загрузка 4 вопросов
                if (Db.DictKeySelectedQustion.Count == 0)
                {
                    tmp.text = CurListQuestions.DictAllQuestions[i];
                }
                
                // Обновляем список вопросов
                if (Db.SelectQuestionKey != -1 && !IsAddSelectedKey)
                {
                    IsAddSelectedKey = true;
                }
            }

            if (bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                architect.Stop();
            }

            // + 1 клавиша для остановки архитектора текста
            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    architect.Stop();
            //}

            // Условие для обновления Content при достижении конца или перехода к следующему ответу
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Если всего 1 ответ, то при нажатии на Space, мы оновляем поле Content
                // Ошибка в том, что Btn Object выделен и вызывается повторно!!!
                if (Db.SelectQuestionKey == -1)
                {
                    //Debug.Log($"Вызов setDefault при нажатии на Space : {Db.SelectQuestionKey} =? -1");
                    setDefaultContent();

                    // Возвращаем флаг к исходному состоянию
                    IsAddSelectedKey = false;

                    // Включаем список вопросов
                    //LinkListQuestions.SetActive(true);
                }

                // Если ответов несколько, то при нажатии на Space, мы переходим дальше
                if (Db.SelectQuestionKey != -1)
                {
                    StartKeyAnswer++;

                    // Выключаем список вопросов
                    //LinkListQuestions.SetActive(false);
                }
            }
            
            // Работает при запуске игры
            //Debug.Log($"SelectQuestionKey! {Db.SelectQuestionKey}");

            // Настраиваем управление (Вывод ответов персонажем)
            if (Db.SelectQuestionKey != -1)
            {
                // Выключаем список вопросов
                LinkListQuestions.SetActive(false);

                if (architect.isBuilding)
                {
                    // Ускорение при повторном нажатии
                    if (!architect.hurryUp)
                    {
                        architect.hurryUp = true;
                    }
                    else
                    {
                        architect.ForceComplete();
                    }
                }
                else
                {
                    // Тут скрывается блок вопросов и выводится 1-ый ответ на вопрос
                    string answerStr = CurListQuestions.DictAllAnswers[Db.SelectQuestionKey];
                    if (answerStr.Contains("|"))
                    {
                        // Список ответов
                        string[] outAllAnswer = CurListQuestions.getFullAnswersFromStrDict(answerStr);

                        // Ограничение вывода ответов, чтобы не уйти за список
                        if (StartKeyAnswer < outAllAnswer.Count())
                        {
                            string answer = outAllAnswer[StartKeyAnswer];
                            if (answer != LastContentText)
                            {
                                architect.Build(answer);
                                LastContentText = answer;
                            }
                        }
                        else if (StartKeyAnswer == outAllAnswer.Count())
                        {
                            // Заканчиваем печатать ответов
                            Db.SelectQuestionKey = -1;
                        }
                    }
                    else
                    {
                        architect.Build(answerStr);

                        LastContentText = answerStr;

                        // Заканчиваем печатать ответа
                        Db.SelectQuestionKey = -1;

                        // Выключаем список вопросов
                        //LinkListQuestions.SetActive(false);
                    }
                }
            }
            else if (Db.SelectQuestionKey == -1)
            {
                // Включаем список вопросов
                LinkListQuestions.SetActive(true);
            }

            // Дополнительное добавление строк в Content (Работает с условием на нажатие кнопки)
            //else if (Input.GetKeyDown(KeyCode.A))
            //{
            //    architect.Append(longLine);
            //    //architect.Append(lines[Random.Range(0, lines.Length)]);
            //}
        }
        /// <summary>
        /// Функция для возвращения блока Content в исходное состояние, чтобы не было проблем с TextArchitect
        /// (И я не знаю, что приводит к сбоям, так как я перепробывал Абсолютно Всё! Механику, условия и тд.)
        /// </summary>
        void setDefaultContent()
        {
            string str = Db.AllListQuestions[0].DefaultQuestionCharacter;
            architect.Build(str);
            // Обновляем последний введённый текст
            LastContentText = str;
            // Обнуляем счётчик
            StartKeyAnswer = 0;
        }
    }
}
