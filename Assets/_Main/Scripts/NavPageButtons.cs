using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        Db = navBar.GetComponent<NavOverlayData>().Database.GetComponent<Database>();

        // Изменяем curPage в navBar.getComponent()
        // Меняем страницу, в NavOverlayData.Update(), с помощью CurrentPage
        
        // Также делаем проверку по условию, для начала игры (Это относится к Preparation объекту)
        // Не работает обновление текста макета Preparation
        if (Db.IsNewGame && !Db.IsInterview) // TODO откуда он вопросы подтягивает?
        {
            Db.IsNewGame = true;
            Db.IsInterview = true;
            navBar.GetComponent<NavOverlayData>().CurrentPage.GetComponent<LoadPreparation>().Updater();
        }
        else
        {
            navBar.GetComponent<NavOverlayData>().CurrentPage = requiredPage;
        }
    }

    public void OnQuestSelected(GameObject requiredPage)
    {
        var availableQuestionsButtons = requiredPage.transform.GetChild(1).GetChild(0).GetChild(0).gameObject; // Choose Questions > Lists > AvailableQuestions
        var selectedQuestionsButtons = requiredPage.transform.GetChild(1).GetChild(1).GetChild(0).gameObject; // Choose Questions > Lists > SelectedQuestions
        var questionCount = requiredPage.transform.GetChild(0).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>(); // Choose Questions > Backgrounds > QuestionCount (Text)

        Db.CurQuestions = new Dictionary<int, string>(); // TODO ?

        var qcNewText = questionCount.text.Split("/");
        qcNewText[1] = Db.CurQuest.QuestionCount.ToString();
        questionCount.text = string.Join("/", qcNewText);
        var maxQuestions = Int32.Parse(qcNewText[1]);
        var currQuestions = Int32.Parse(qcNewText[0]); // базово - 0

        var questions = Db.AllListQuestions[Db.CurQuest.ID_ListQuestions];
        foreach(var item in questions.DictAllQuestions) {
            var q = Instantiate(QuestionPrefabs);
            q.name = $"question - {item.Key}";
            var test = q.GetComponent<Button>();
            test.onClick.AddListener(() => 
                {
                    if (!Db.CurQuestions.ContainsKey(item.Key)) { // TODO менять позицию кнопки и ее надпись
                        if (currQuestions >= maxQuestions) return;

                        Db.CurQuestions[item.Key] = item.Value;
                        q.transform.SetParent(selectedQuestionsButtons.transform);
                        currQuestions++;
                        qcNewText[0] = currQuestions.ToString(); // TODO переделать увеличение счетчика выбранных вопросов
                        questionCount.text = string.Join("/", qcNewText);
                    } else {
                        Db.CurQuestions.Remove(item.Key);
                        q.transform.SetParent(availableQuestionsButtons.transform);
                        currQuestions--;
                        qcNewText[0] = currQuestions.ToString(); // TODO переделать уменьшение счетчика выбранных вопросов
                        questionCount.text = string.Join("/", qcNewText);
                    }
                });
            var tmpText = q.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            tmpText.text = item.Value;
            q.transform.SetParent(availableQuestionsButtons.transform);

            q.transform.localPosition = new Vector3(
                q.transform.position.x,
                q.transform.position.y,
                0);
            q.transform.localScale = Vector3.one;
        }
        OnButtonClick(requiredPage);
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
        Db.DictKeySelectedQustion.Clear();
        Db.CountQuestionsReplacement = 0;
        Db.CountEntrace = 0;

        // Сохраняем 1-ые 4 вопроса, для этапа игры "Интервью"
        for (int i = 0; i < 4; i++)
            Db.DictKeySelectedQustion.Add(Db.DictKeySelectedQustion.Count(), i);

        // Вызываем функцию смены камеры и перехода от текущего Canvas объекта, к следующему Canvas объекту
        ChangeCamera();
        
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
        int keyListQuestions = Db.CurQuest.ID_ListQuestions;
        var curListQuestions = Db.AllListQuestions[keyListQuestions];

        // Получаем нужный нам Count-ры и вычисляем разницу, для условия
        int countSelectDict = Db.DictKeySelectedQustion.Count();
        int countQuestionsDict = curListQuestions.DictAllQuestions.Count();
        int mathDifferenceCountDicts = (countQuestionsDict - 4);

        // Счётчик нажатий на вопрос (Временный костыль)
        Db.CountEntrace++;

        // Если счётчик замены не достиг разницы mathDifferenceCountDicts
        // то мы делаем замену вопроса в списке вопросов
        // иначе вопрос скрывается
        if (Db.CountQuestionsReplacement < mathDifferenceCountDicts)
        {
            bool IsFoundNewQuestions = false;

            // Делаем замену названия кнопки и вопроса
            // В начале перебираем все вопросы, из основного словаря вопросов
            foreach (var item in curListQuestions.DictAllQuestions)
            {
                // Проверка на наличие ключа, в словаре выбранных вопросов
                IsFoundNewQuestions = Db.DictKeySelectedQustion.Values.Contains(item.Key);

                // Тесты
                //Debug.Log($"IsFoundNewQuestions = {IsFoundNewQuestions} and item.key = {item.Key}");
                
                if (!IsFoundNewQuestions)
                {
                    // Смена имени + ключ вопроса
                    btn.name = "Question " + item.Key;

                    // Получаем tmp obj и делаем смену вопроса
                    var tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    tmp.text = item.Value;

                    // Счётчик для работы условия + сохранение нового вопроса
                    Db.CountQuestionsReplacement++;
                    Db.DictKeySelectedQustion.Add(Db.DictKeySelectedQustion.Count, item.Key);
                    break;
                }
            }
        }
        else
        {
            btn.SetActive(false);
        }

        // Сохраняем ссылку на объект для перехода к следующему этапу игры
        if (Db.CountEntrace == countQuestionsDict)
        {
            Db.LinkNavPageBtnGameObject = this;
        }
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
            CanvasOver.SetActive(false);
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
