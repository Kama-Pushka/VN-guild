using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class NavPageButtons : MonoBehaviour
{
    // Ссылки на БД и камеры
    [SerializeField]
    GameObject LinkDatabase;
    [SerializeField]
    GameObject LinkNavBar;
    [SerializeField]
    GameObject LinkQuestCamera;
    [SerializeField]
    GameObject LinkOverlayCamera;

    [SerializeField]
    GameObject QuestionPrefabs; // TODO
    [SerializeField]
    GameObject QuestionInInterviewPrefab;

    [SerializeField]
    GameObject Journal; // TODO а надо ли на самом деле здесь иметь ссылку на объект журнала??

    // Взятие компонента из ссылок
    Database Db { get; set; }
    NavOverlayData NavData { get; set; }
    GameObject CanvasOver {  get; set; }
    GameObject CanvasQuest { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Db = LinkDatabase.GetComponent<Database>();
        NavData = LinkNavBar.GetComponent<NavOverlayData>();

        // Блок для отключения Canvas - Overlay, чтобы работали btn-ы
        // В начале переходим к Root, а от него к Canvas-ам
        CanvasOver = Db.transform.parent.transform.GetChild(2).gameObject;
        CanvasQuest = Db.transform.parent.transform.GetChild(1).gameObject;

        // Порядок камер при запуске игры
        LinkQuestCamera.SetActive(false);
        LinkOverlayCamera.SetActive(true);
    }

    /// <summary>
    /// Универсальная функция перехода по страницам
    /// </summary>
    /// <param name="requiredPage"></param>
    public void OnButtonClick(GameObject requiredPage)
    {
        // Далее нам надо поменять камеры местами, чтобы начать квест с фермером,
        // после, активация текстового архитектора, для прохождения квеста

        // Получаем текущий объект и объект с навигацией
        GameObject curPage = gameObject.transform.parent.gameObject;

        // Число 6, в GetChild отвечает за номер объекта, сверху вниз, в объекте Page => мы обращаемся к Navigation Bar
        GameObject navBar = curPage.transform.parent.gameObject.transform.GetChild(6).gameObject;

        // Подключаем БД, для реализации условия ниже
        // (Использовалась до метода Start(), актуальность не известна)
        //Db = navBar.GetComponent<NavOverlayData>().Database.GetComponent<Database>();

        // Изменяем curPage в navBar.getComponent()
        // Меняем страницу, в NavOverlayData.Update(), с помощью CurrentPage

        // Также делаем проверку по условию, для начала игры (Это относится к Preparation объекту)
        // Не работает обновление текста макета Preparation
        if (Db.IsNewGame && !Db.IsInterview)
        {
            //Db.IsNewGame = true;
            Db.IsInterview = true;
        }
        else
        {
            navBar.GetComponent<NavOverlayData>().CurrentPage = requiredPage;
        }
        if (!Db.IsNewGame)
        {
            navBar.GetComponent<NavOverlayData>().CurrentPage.GetComponent<LoadPreparation>().StartNew(); // TODO делать это не здесь
        }
        navBar.GetComponent<NavOverlayData>().CurrentPage.GetComponent<LoadPreparation>().Updater(); // TODO Только если запускаем Preparation!!!
    }

    public void OnQuestSelected(GameObject requiredPage)
    {
        var availableQuestionsButtons = requiredPage.transform.GetChild(1).GetChild(0).GetChild(0).gameObject; // Choose Questions > Lists > AvailableQuestions
        var selectedQuestionsButtons = requiredPage.transform.GetChild(1).GetChild(1).GetChild(0).gameObject; // Choose Questions > Lists > SelectedQuestions
        var questionCount = requiredPage.transform.GetChild(0).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>(); // Choose Questions > Backgrounds > QuestionCount (Text)

        Db.AvailableQuestions = new Dictionary<int, Question>(); // TODO ?

        var qcNewText = questionCount.text.Split("/");
        qcNewText[1] = Db.CurQuest.QuestionCount.ToString();
        questionCount.text = string.Join("/", qcNewText);
        var maxQuestions = Int32.Parse(qcNewText[1]);
        var currQuestions = Int32.Parse(qcNewText[0]); // базово - 0

        var questions = Db.CurQuest.Questions;
        foreach(var item in questions) {
            var q = Instantiate(QuestionPrefabs);
            q.name = $"question - {item.ID}";
            var button = q.GetComponent<Button>();
            button.onClick.AddListener(() =>  // TODO неоптимально, но лан
            {
                    if (!Db.AvailableQuestions.ContainsKey(item.ID)) { // TODO менять позицию кнопки и ее надпись
                        if (currQuestions >= maxQuestions) return;

                        Db.AvailableQuestions[item.ID] = item;
                        q.transform.SetParent(selectedQuestionsButtons.transform);
                        currQuestions++;
                        qcNewText[0] = currQuestions.ToString(); // TODO переделать увеличение счетчика выбранных вопросов
                        questionCount.text = string.Join("/", qcNewText);
                    } else {
                        Db.AvailableQuestions.Remove(item.ID);
                        q.transform.SetParent(availableQuestionsButtons.transform);
                        currQuestions--;
                        qcNewText[0] = currQuestions.ToString(); // TODO переделать уменьшение счетчика выбранных вопросов
                        questionCount.text = string.Join("/", qcNewText);
                    }
                });
            var tmpText = q.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            tmpText.text = item.TextQuestion;
            q.transform.SetParent(availableQuestionsButtons.transform);

            q.transform.localPosition = new Vector3(
                q.transform.position.x,
                q.transform.position.y,
                0);
            q.transform.localScale = Vector3.one;
        }

        JournalInfoUpdate(Db.CurQuest.NameQuest, Db.CurQuest.Description, Db.CurQuest.CharacterName);

        OnButtonClick(requiredPage);
    }

    public void JournalInfoUpdate(string questName, string questDescription, string customerName) 
    {
        var questTitle = Journal.transform.GetChild(1).GetChild(4).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>(); // JOURNAL > 1 - Background > 0 - Quest > Quest Title
        var questDesc = Journal.transform.GetChild(1).GetChild(4).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>(); // JOURNAL > 1 - Background > 0 - Quest > Quest Desc
        var customer = Journal.transform.GetChild(1).GetChild(4).GetChild(5).gameObject.GetComponent<TextMeshProUGUI>(); // JOURNAL > 1 - Background > 0 - Quest > Customer

        questTitle.text = questName;
        questDesc.text = questDescription;
        customer.text = customerName;
    }

    // TODO а не будет такого, что выйдя из квеста мы вернемся на выбор вопроса? (нет, кнопки назад при интерью быть не должно)
    // а точнее очищать оставшиеся объекты вопросов, ибо они все еще там (в логике по идее это обновляется)
    public void OnQuestionSelected(GameObject requiredPage) // Выбрали список вопросов и переходим к интервью
    {
        var questionList = requiredPage.transform.GetChild(5).GetChild(1).GetChild(0).gameObject; // LAYERS > 5 - Foreground > Questions > List
        foreach (var item in Db.AvailableQuestions)
        {
            CreateQuestion(questionList, item.Value);
        }

        OnChangeCameraClick(requiredPage);

        //// Обновляем список вопросов при выборе одного из вопросов
        //if (Db.SelectQuestionKey != -1 && !IsAddSelectedKey)
        //{
        //    IsAddSelectedKey = true;
        //}
    }

    private void CreateQuestion(GameObject questionList, Question question) 
    {
        var btn = Instantiate(QuestionInInterviewPrefab);
        btn.name = $"Qustion {question.ID}";
        var button = btn.GetComponent<Button>();
        button.onClick.AddListener(() => OnQuestionClick(btn)); // TODO неоптимально, но лан

        var tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        tmp.text = question.TextQuestion;

        btn.transform.SetParent(questionList.transform);

        btn.transform.localPosition = new Vector3(
            btn.transform.position.x,
            btn.transform.position.y,
            0);
        btn.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Функция для перехода между камерами с помощью флага IsQuestCamOn из Database (Db). Также отвечает за сохранение 1-ых 4-ёх вопросов квеста
    /// </summary>
    /// <param name="Db.IsQuestCamOn">По умолчанию = true</param>
    public void OnChangeCameraClick(GameObject nextPage)
    {

        // Сохраняем последнюю страницу, из который мы меняли камеру,
        // чтобы даже при переключении камер, кнопки с предыдущей страницы не работали,
        // также это поможет вернуться при переходе, между камерами.

        NavData.CurrentPage.SetActive(false);

        Db.LastPage = NavData.CurrentPage;
        NavData.CurrentPage = nextPage;

        // Тесты
        //Debug.Log($"ClickOnPage = {NavData.CurrentPage}");
        //Debug.Log($"Db.LastPage = {Db.LastPage}");
        //Debug.Log($"Nav.CurPage = {NavData.CurrentPage}");

        // Обнуление при переходе
        Db.DictKeySelectedQustion.Clear(); // TODO ???
        Db.CountQuestionsReplacement = 0;
        Db.CountEntrace = 0;

        // Сохраняем 1-ые 4 вопроса, для этапа игры "Интервью"
        for (int i = 0; i < 4; i++)
            Db.DictKeySelectedQustion.Add(Db.DictKeySelectedQustion.Count(), i);

        // Вызываем функцию смены камеры и перехода от текущего Canvas объекта, к следующему Canvas объекту
        ChangeCamera();

    }

    // TODO ??
    public void OnEndInterview() 
    {
        // Переключаем флаг
        Db.IsEndInterview = true;

        // Выключаем тукую страницу (Этап интервью)
        NavData.CurrentPage.SetActive(false);

        // Сохраняем последнюю страницу
        Db.LastPage = NavData.CurrentPage;

        // Делаем переход к NavData объекту и от него к странице Preparation - Id : 1
        NavData.CurrentPage = NavData.transform.parent.GetChild(1).gameObject;

        ChangeCamera(); // Db.LinkNavPageBtnGameObject.ChangeCamera(); TODO is null, why??
        NavData.GetComponent<NavOverlayData>().CurrentPage.GetComponent<LoadPreparation>().Updater(); // TODO Только если запускаем Preparation!!!
    }

    // TODO костыль?
    public void OnJournalActivate(GameObject journal) 
    {
        NavData.CurrentPage.SetActive(false);

        Db.LastPage = NavData.CurrentPage;
        NavData.CurrentPage = journal;

        journal.SetActive(true); // TODO сделать адекватное переключение между камерами

        LinkQuestCamera.SetActive(true);
        LinkOverlayCamera.SetActive(false);
        Db.IsQuestCamOn = false;

        // Отключение Over канвы
        CanvasOver.SetActive(false); // TODO сделать свою переключалку для журнала (временный костыль)
        CanvasQuest.SetActive(true);
    }

    /// <summary>
    /// Функция завершения игры
    /// </summary>
    public void OnExitClick() => Application.Quit();
    /// <summary>
    /// Функция для отображения квестов с правой стороны страницы Choose Quest
    /// </summary>
    /// <param name="tmp"></param>
    public void OnQuestClick(TextMeshProUGUI tmp)
    {
        // Выделяем переменные для работы с нумерацией квеста
        // (Забрать номер из названия "Quest - 0" и попадаем к gameObject-у по transform.parent)
        string[] splitName = tmp.gameObject.transform.parent.gameObject.name.Split(' ');
        int takeNumQuest = int.Parse(splitName[splitName.Length - 1]);

        // Сохраняем текущий нажатый квест, для вывода справа, на страницу Choose Quest
        Db.CurQuest = Db.AllQuests[takeNumQuest];
    }
    /// <summary>
    /// Функция обработки клика на вопрос, для сохранения Uid вопроса в квесте.
    /// </summary>
    /// <param name="btn">Берём Button объект, чтобы было проще обращаться к его .name компоненту</param>
    public void OnQuestionClick(GameObject btn)
    {
        string[] str = btn.name.Split(" ");
        Db.SelectQuestionKey = int.Parse(str[1]);

        // Алгоритм работы функции после выбора вопроса:
        // 1) Для начала мы перебираем словарь всех вопросов;
        // 2) Находим не добавленный ключ вопроса из Db.DictKeySelectedQustion;
        // 3) Добавляем этот ключ и выводим в список вопросов, как новый оставшийся вопрос;

        // Получаем нужный список вопросов
        //int keyListQuestions = Db.CurQuest.ID_ListQuestions;
        //var curListQuestions = Db.AllListQuestions[keyListQuestions];

        // Получаем нужный нам Count-ры и вычисляем разницу, для условия
        //int countSelectDict = Db.DictKeySelectedQustion.Count();
        //int countQuestionsDict = Db.CurQuestions.Count; // curListQuestions.DictAllQuestions.Count();
        //int mathDifferenceCountDicts = 0;// (countQuestionsDict - 4);

        // Счётчик нажатий на вопрос (Временный костыль)
        Db.CountEntrace++;

        var questionList = btn.transform.parent.gameObject; // LAYERS > 5 - Foreground > Questions > List
        var selectQuestion = Db.AvailableQuestions[Db.SelectQuestionKey]; // стартовые вопросы уже находятся там, последующие добавляются
        foreach (var que in selectQuestion.ChildQuestions) 
        {
            Db.AvailableQuestions[que.ID] = que;
            CreateQuestion(questionList, que);
        }

        Db.CurQuestion = selectQuestion;
        btn.SetActive(false);

        // Сохраняем ссылку на объект для перехода к следующему этапу игры
        if (Db.CountEntrace == Db.AvailableQuestions.Count)
        {
            Db.LinkNavPageBtnGameObject = this;
        }

        // TODO Legacy
        // Если счётчик замены не достиг разницы mathDifferenceCountDicts
        // то мы делаем замену вопроса в списке вопросов
        // иначе вопрос скрывается
        //if (Db.CountQuestionsReplacement < mathDifferenceCountDicts)
        //{
        //    bool IsFoundNewQuestions = false;

        //    // Делаем замену названия кнопки и вопроса
        //    // В начале перебираем все вопросы, из основного словаря вопросов
        //    foreach (var item in curListQuestions.DictAllQuestions)
        //    {
        //        // Проверка на наличие ключа, в словаре выбранных вопросов
        //        IsFoundNewQuestions = Db.DictKeySelectedQustion.Values.Contains(item.Key);

        //        // Тесты
        //        //Debug.Log($"IsFoundNewQuestions = {IsFoundNewQuestions} and item.key = {item.Key}");

        //        if (!IsFoundNewQuestions)
        //        {
        //            // Смена имени + ключ вопроса
        //            btn.name = "Question " + item.Key;

        //            // Получаем tmp obj и делаем смену вопроса
        //            var tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        //            tmp.text = item.Value;

        //            // Счётчик для работы условия + сохранение нового вопроса
        //            Db.CountQuestionsReplacement++;
        //            Db.DictKeySelectedQustion.Add(Db.DictKeySelectedQustion.Count, item.Key);
        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    btn.SetActive(false);
        //}
    }
    /// <summary>
    /// Функция для смены камер и Canvas объектов
    /// </summary>
    public void ChangeCamera()
    {
        // Работает
        if (Db.IsQuestCamOn)
        {
            // Порядок камер при запуске игры
            LinkQuestCamera.SetActive(true);
            LinkOverlayCamera.SetActive(false);
            Db.IsQuestCamOn = false;

            // Отключение Over канвы
            CanvasOver.SetActive(false); // TODO сделать свою переключалку для журнала (временный костыль)
            CanvasQuest.SetActive(true);

            //Debug.Log($"canvOver = {canvOver.activeInHierarchy}");
            //Debug.Log($"canvQuest = {canvQuest.activeInHierarchy}");
        }
        else
        {
            // Переход к камере с квестами
            LinkQuestCamera.SetActive(false);
            LinkOverlayCamera.SetActive(true);
            Db.IsQuestCamOn = true;

            // Отключение Quest канвы
            CanvasOver.SetActive(true);
            CanvasQuest.SetActive(false);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    //
    //}
}
