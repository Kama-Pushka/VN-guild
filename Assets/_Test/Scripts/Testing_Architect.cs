using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

// TODO выносить в основной проект
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
        [SerializeField]
        GameObject LinkNavBar;

        Database Db { get; set; }
        NavOverlayData NavData { get; set; }
        Question CurQuestion { get; set; }
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
            NavData = LinkNavBar.GetComponent<NavOverlayData>();

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
        { // TODO а это должно ДО ЗАГРУЗКИ КВЕСТА грузиться? а оно это делает
            // Обновление первичного текста на странице диалога
            Head.text = Db.CurQuest.CharacterName;

            // Берём внешний ключ текущего квеста
            KeyListQuestions = Db.CurQuest.ID_ListQuestions;

            // Получаем список вопросов с помощью KeyListQuestions
            CurQuestion = Db.CurQuestion;

            // Заполняем раздел вопросов (макс 4)
            
            // Первичная загрузка 4 вопросов
            //if (Db.DictKeySelectedQustion.Count == 0)
            //{
            //    tmp.text = CurListQuestions.DictAllQuestions[i];
            //}

            // Обновляем список вопросов при выборе одного из вопросов
            //if (Db.SelectQuestionKey != -1 && !IsAddSelectedKey) // TODO ??
            //{
            //    IsAddSelectedKey = true;
            //}

            if (bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                architect.Stop();
            }

            // Баги:
            // 1) Есть баг с завершением вывода ответа на вопрос и выводом вопроса персонажа, после ответа.
            // ~ Приходится нажимать 2 раза Space, чтобы появился вопрос персонажа по умолчанию.
            // ~ Также я заметил, что данный баг случается Только при нескольких ответах на вопрос. (Надо будет исправить)

            // 2) Баг с сильным ускорением текста при повторном нажатии на Space, если ответов несколько. (-||-)

            // Условие для обновления Content при достижении конца или перехода к следующему ответу
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Исправлена ошибка с Selected компонентом кнопки (Чтобы не забыть)
                // ~ Из-за нажатия на вопрос, NavPageButtons срабатывал повторно, при нажатии на Space.

                // Обновление текста в Content
                if (Db.SelectQuestionKey == -1)
                {
                    //Debug.Log($"Вызов setDefault при нажатии на Space : {Db.SelectQuestionKey} =? -1");
                    setDefaultContent();

                    // Возвращаем флаг к исходному состоянию
                    IsAddSelectedKey = false;

                    // Включаем список вопросов
                    LinkListQuestions.SetActive(true);

                    // TODO убрать?
                    // Завершение этапа игры "Интервью"
                    if (Db.CountEntrace == Db.AvailableQuestions.Count) // CurListQuestions.DictAllQuestions.Count    TODO
                    {
                        // Переключаем флаг
                        Db.IsEndInterview = true;

                        // Выключаем тукую страницу (Этап интервью)
                        NavData.CurrentPage.SetActive(false);

                        // Сохраняем последнюю страницу
                        Db.LastPage = NavData.CurrentPage;

                        // Делаем переход к NavData объекту и от него к странице Preparation - Id : 1
                        NavData.CurrentPage = NavData.transform.parent.GetChild(1).gameObject;

                        Db.LinkNavPageBtnGameObject.ChangeCamera(); // TODO null(но не null если задать 4 вопроса) (преверить как было до) (ну и почему при задании всех вопросов сцена слетает)
                    }
                }

                // Если ответов несколько, то при нажатии на Space, мы переходим дальше
                if (Db.SelectQuestionKey != -1)
                {
                    StartKeyAnswer++;
                }
            }

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
                    string answerStr = CurQuestion.TextAnswer;
                    if (answerStr.Contains("|"))
                    {
                        // Список ответов
                        string[] outAllAnswer = CurQuestion.getFullAnswersFromStrDict(answerStr);

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
                    }
                }
            }

            // Доп. код (Для себя):

            // + 1 клавиша для остановки архитектора текста
            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    architect.Stop();
            //}

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
            string str = CurQuestion.DefaultQuestionCharacter;
            architect.Build(str);

            // Обновляем последний введённый текст
            LastContentText = str;

            // Обнуляем счётчик
            StartKeyAnswer = 0;
        }
    }
}
