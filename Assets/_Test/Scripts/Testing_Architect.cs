using System.Collections;
using System.Collections.Generic;
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
        int StartKeyAnswer { get; set; }
        bool IsEndOutText { get; set; }
        Dictionary<int, int> DictSelectedQuestions { get; set; }

        // Текст для проверки
        //string[] lines = new string[5]
        //{
        //    "Это рандомная диалоговая линия.",
        //    "Я хочу тебе что-то сказать.",
        //    "Этот мир имеет сумашедшии места.",
        //    "Не растраивайся, будь сильнее!",
        //    "Эта птица? Это самолёт? Нет! - Это Супер Шелти!"
        //};

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
            DictSelectedQuestions = new Dictionary<int, int>();

            // Счётчик для нажатия клавиши Space (Вывод ответа на вопрос в опр. порядке)
            StartKeyAnswer = 0;
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
            
            //Вывод сообщения Character по умолчанию
            if (!IsEndOutText)
            {
                Content.text = CurListQuestions.DefaultQuestionCharacter;
                IsEndOutText = true;
            }

            // Заполняем раздел вопросов (макс 4)
            for (int i = 0; i < 4; i++)
            {
                GameObject btn = LinkListQuestions.transform.GetChild(i).gameObject;
                TextMeshProUGUI tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                
                // Первичная загрузка 4 вопросов
                // Error - 1
                if (DictSelectedQuestions.Count == 0)
                {
                    tmp.text = CurListQuestions.DictAllQuestions[i];
                }
                
                // Выбрали вопрос
                if (Db.SelectQuestionKey != -1)
                {
                    // Добавляем текущий вопрос с ключём
                    //DictSelectedQuestions.Add(Db.SelectQuestionKey, CurListQuestions.DictAllQuestions[i]);
                    DictSelectedQuestions.Add(DictSelectedQuestions.Count, i);

                    // Пересобираем список квестов
                    //

                    // Далее должен вызываться архитектор текста с ответами и список вопросов должен скрыться на время
                    LinkListQuestions.SetActive(false);
                }

                // Если вопрос не выбран, то возвращаем видимость списка вопросов
                if (Db.SelectQuestionKey == -1)
                {
                    LinkListQuestions.SetActive(true);
                    IsEndOutText = false;
                }
            }

            if (bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                IsEndOutText = false;
                architect.Stop();
            }

            // + 1 клавиша для остановки архитектора текста
            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    architect.Stop();
            //}

            // Строка для теста текста с ускорением
            //string longLine = "Это очень длинная строка! Она не имеет никакого смысла и нужная чисто для проверки всякой разной всячины." +
            //    " Не спрашивайте зачем мне нужна данная очень длинная строка. Я вообще хз, зачем она тут! И да, я не люблю очень большое кол-во текста." +
            //    " Я попросту устаю читать!";

            // if (Input.GetKeyDown(KeyCode.Space))
            
            // Настраиваем управление (Вывод текста персонажем)
            if (Db.SelectQuestionKey != -1)
            {
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
                        //Debug.Log($"Завершился вывод?");
                    }
                }
                else
                {
                    // Отправляем архитектору текста наши ответы на вопросы. (Не доделано!)
                    // Так как нужно это всё свзать с вопросами, а привязки пока что нету.
                    if (Db.SelectQuestionKey == -1)
                    {
                        string outTextContent = Db.AllListQuestions[KeyListQuestions].DefaultQuestionCharacter;
                        architect.Build(outTextContent);
                    }
                    else
                    {
                        // (Не доделано!)
                        // Тут скрывается блок вопросов и выводятся ответы по порядку
                        string answerStr = CurListQuestions.DictAllAnswers[Db.SelectQuestionKey];
                        if (answerStr.Contains("|"))
                        {
                            string[] outAllAnswer = CurListQuestions.getFullAnswersFromStrDict(answerStr);
                            bool IsFirstAnswer = true;
                            for (int i = 0; i < outAllAnswer.Length;)
                            {
                                if (IsFirstAnswer)
                                {
                                    architect.Build(outAllAnswer[i]);
                                    Debug.Log($"str[_0_] = {outAllAnswer[i]}");
                                    i++;
                                    IsFirstAnswer = false;
                                    continue;
                                }
                                else
                                {
                                    if (Input.GetKeyDown(KeyCode.Space))
                                    {
                                        architect.Build(outAllAnswer[i]);
                                        Debug.Log($"str[{i}] = {outAllAnswer[i]}");
                                        i++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            architect.Build(answerStr);
                            Debug.Log($"str = {answerStr}");
                        }

                        // Заканчиваем печатать ответы
                        Db.SelectQuestionKey = -1;
                    }
                    //string outText = CurListQuestions.DictAllAnswers[StartKeyAnswer];
                    //architect.Build(outText);
                    //Debug.Log($"outText  = {outText}");

                    //architect.Build(lines[Random.Range(0, lines.Length)]);
                }
                // Дополнительное добавление строк в Content (Работает с условием на нажатие кнопки)
                //else if (Input.GetKeyDown(KeyCode.A))
                //{
                //    architect.Append(longLine);
                //    //architect.Append(lines[Random.Range(0, lines.Length)]);
                //}
            }
        }
    }
}
