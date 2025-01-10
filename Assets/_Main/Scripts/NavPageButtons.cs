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

    // Взятие компонента из ссылок
    Database Db { get; set; }
    NavOverlayData NavData { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {
        Db = LinkDatabase.GetComponent<Database>();
        NavData = LinkNavBar.GetComponent<NavOverlayData>();

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
        if (Db.IsNewGame && !Db.IsInterview)
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
    /// <summary>
    /// Функция для перехода между камерами с помощью флага IsQuestCamOn из Database (Db)
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

        //Debug.Log($"ClickOnPage = {NavData.CurrentPage}");
        //Debug.Log($"Db.LastPage = {Db.LastPage}");
        //Debug.Log($"Nav.CurPage = {NavData.CurrentPage}");

        // Блок для отключения Canvas - Overlay, чтобы работали btn-ы
        GameObject canvOver = Db.transform.parent.transform.GetChild(2).gameObject;
        GameObject canvQuest = Db.transform.parent.transform.GetChild(1).gameObject;

        // Работает
        if (Db.IsQuestCamOn)
        {
            // Порядок камер при запуске игры
            LinkQuestCamera.SetActive(true);
            LinkOverlayCamera.SetActive(false);
            Db.IsQuestCamOn = false;
            // Отключение Over канвы
            canvOver.SetActive(false);
            canvQuest.SetActive(true);
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
            canvOver.SetActive(true);
            canvQuest.SetActive(false);
        }
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

        // Также обновляем список выбранных вопросов, для выбранного квеста (Без всяких доп. ластов объектов)
        Db.DictKeySelectedQustion.Clear();
        Db.CountQuestionsReplacement = 0;

        // Записываем 1-ые 4-е вопроса, для будущей работы механики замены
        // (Чтобы начальные, не выбранные вопросы, не заменялись сами на себя)
        for (int i = 0; i < 4; i++)
        {
            Db.DictKeySelectedQustion.Add(Db.DictKeySelectedQustion.Count(), i);
            //Debug.Log($"Dict[{i}] = {Db.DictKeySelectedQustion[i]}");
        }
    }
    /// <summary>
    /// Функция обработки клика на вопрос, для сохранения Uid вопроса в квесте.
    /// </summary>
    /// <param name="tmp">Берём TMP объект, чтобы было проще обращаться к его .name компоненту</param>
    public void OnQuestionClick(GameObject btn)
    {
        string[] str = btn.name.Split(" ");
        Db.SelectQuestionKey = int.Parse(str[1]);

        // Проверка перед добавлением ключа к Dictionary
        bool IsNotFirstKey = false;
        foreach (var val in Db.DictKeySelectedQustion.Values)
        {
            if (val == Db.SelectQuestionKey)
                IsNotFirstKey = true;
        }
        // Добавляем, если это уникальный ключ
        if (!IsNotFirstKey)
            Db.DictKeySelectedQustion.Add(Db.DictKeySelectedQustion.Count, Db.SelectQuestionKey);

        // Инфо для дебагера (тесты)
        //Debug.Log($"Dict.Count() = {Db.DictKeySelectedQustion.Count}");

        // Получаем нужный список вопросов
        int keyListQuestions = Db.CurQuest.ID_ListQuestions;
        var curListQuestions = Db.AllListQuestions[keyListQuestions];

        // Получаем нужный нам Count-ры и вычисляем разницу, для условия
        int countSelectDict = Db.DictKeySelectedQustion.Count();
        int countQuestionsDict = curListQuestions.DictAllQuestions.Count();
        int mathDifferenceCountDicts = (countQuestionsDict - 4);

        // Если счётчик замены не достиг разницы mathDifferenceCountDicts
        // то мы делаем замену вопроса в списке вопросов
        // иначе вопрос скрывается
        if (Db.CountQuestionsReplacement < mathDifferenceCountDicts)
        {
            bool IsFoundNewQuestions = false;

            // Где-то тут ошибка в работе словарей... Надо перепроверить...

            // Делаем замену названия кнопки и вопроса
            foreach (var val in Db.DictKeySelectedQustion.Values)
            {
                foreach (var item in curListQuestions.DictAllQuestions)
                {
                    if (item.Key != val)
                    {
                        // Инфо для дебагера (тесты)
                        //Debug.Log($"item.Key = {item.Key}");
                        //Debug.Log($"val = {val}");
                        // Смена имени + ключ вопроса
                        btn.name = "Question " + item.Key;
                        // Получаем tmp obj и делаем смену вопроса
                        var tmp = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                        tmp.text = item.Value;

                        Db.CountQuestionsReplacement++;
                        IsFoundNewQuestions = true;
                        break;
                    }
                }
                // Останавливаем работу цикла, после нахождения замены
                if (IsFoundNewQuestions)
                    break;
            }
        }
        else
        {
            btn.SetActive(false);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    //
    //}
}
