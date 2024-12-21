using System.Collections;
using UnityEngine;
using TMPro;

// Тут все переменные связанные с работой текста
public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    // Текущий текст
    public string currentText => tmpro.text;
    // Целевой текст => Это то что мы пытаемся создать или добавить
    public string targetText { get; private set; } = "";
    public string preText { get; private set; } = "";
    private int preTextLength = 0;
    // Полный целевой текст
    public string fullTargetText => preText + targetText;

    // Тут создаём коллекцию, по типу печати и присваиваем значение по умолчанию (версия печатной машинки)
    public enum BuildMethod { instant, typewriter, fade}
    public BuildMethod buildMethod = BuildMethod.typewriter;

    // Цвет текста
    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

    // Базовая скорость, которая будет умножать скорость на множитель скорости
    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    // Скорость вывода текста
    private const float baseSpeed = 1;
    // Множитель скорости
    private float speedMultiplier = 1;

    // Настройки скорости вывода текста на экран
    public int characterPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier = 2 : characterMultiplier = 3; } }
    private int characterMultiplier = 1;

    // Флаг для определения ускорения при повторных нажатиях
    public bool hurryUp = false;

    // Конструкторы для назначения текста в наше пространство
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }
    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    // Метод для создания текста
    public Coroutine Build(string text)
    {
        // Обязательно должен быть пуст, так как мы в него ничего не добавляем
        preText = "";
        // Целевой текст
        targetText = text;

        Stop();

        // Запуск процесса сборки
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    // Метод для создания текста
    public Coroutine Append(string text)
    {
        // Обязательно должен быть пуст, так как мы в него ничего не добавляем
        preText = tmpro.text;
        // Целевой текст
        targetText = text;

        Stop();

        // Запуск процесса сборки
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    // Если наш архитектор ещё создаёт текст, то мы должны остановить повторное создание
    private Coroutine buildProcess = null;
    // bool переменная для проверки
    public bool isBuilding => buildProcess != null;
    // Метод для остановки совместной процедуры, если она запущена
    public void Stop()
    {
        if (!isBuilding)
            return;

        // Если процесс запущен, то он его остановит
        tmpro.StopCoroutine(buildProcess);
        // Обнуление
        buildProcess = null;
    }

    // Запуск процесса сборки
    IEnumerator Building()
    {
        Prepare();

        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }

        OnComplete();
    }
    // Метод для завершения работы печати текста на экран
    private void OnComplete()
    {
        // С помощью присваивания null, мы останавливаем работу печати текста
        buildProcess = null;
        // Остановка ускарения, при завершении строительства
        hurryUp = false;
    }
    // Метод для проверки завершённости построения текста
    public void ForceComplete()
    {
        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                tmpro.ForceMeshUpdate();
                break;
        }
    }
    // Метод подготовки, в зависимости от выбранного нами построения текста
    private void Prepare()
    {
        switch(buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }

    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }
    // Метод эффекта пишущей машинки (вывод текста)
    private void Prepare_Typewriter()
    {
        tmpro.color = tmpro.color;
        // Мы не хотим чтобы что-то было видно в данный момент
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        // Обновление контекста
        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        // Тут уже в не зависимости от контекста мы устанавливаем текст += наш текст
        tmpro.text += targetText;
        // По сути это повторный сброс (Принудительно обновление сетки)
        tmpro.ForceMeshUpdate();
    }
    // Метода для эффекта затухания (вывод текста), режим скрытия
    private void Prepare_Fade()
    {
        tmpro.text = preText;

        // Если текст есть, то идёт обновление
        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            preTextLength = tmpro.textInfo.characterCount;
        }
        else
            preTextLength = 0;
        // Иначе preTextLen = 0

        tmpro.text += targetText;
        
        // Чтобы мы не сталкивались с ограниченим, как в режиме пишущей машинки
        // мы устанавливаем max значение для видимости
        tmpro.maxVisibleCharacters = int.MaxValue;

        // Обновляем текстовую сетку
        tmpro.ForceMeshUpdate();

        // Далее занимаемя самим эффектом через TMP_TextInfo (сетку)
        TMP_TextInfo textInfo = tmpro.textInfo;

        // Благодаря действиями с Color, нам не придётся каждый раз создавать новые цвета,
        // Для каждой вершины, так как тут мы уже сохраняем значения и будем использовать для каждого символа

        // Устанавливаем цвет для текста на 1 (по RGB)
        Color colorVisable = new Color(textColor.r, textColor.g, textColor.b, 1);
        // Тоже самое, но для скрытого текста на 0
        Color colorHidden = new Color(textColor.r, textColor.g, textColor.b, 0);

        // Далее получаем массив цветов, для изменения цветов текста
        // мы берём всю нужную информацию в 1-ой строке [0] и там будут все нужные нам цвета
        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        // Теперь мы проходимся по каждому символу списка, чтобы убедиться в правильности цветов
        // (Занимаемся редактированием массива и оно никак не повлияет на текст)
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            // Берём инфу по символу
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Делаем проверку на отсутствие символа,
            // чтобы не окрашивалась пустота/первый символ (ничего - это надо воспринимать как невидимый, но существующий символ)
            if (!charInfo.isVisible)
                continue;

            // Установка цвета текста, в зависимости от preText-а
            if (i < preTextLength)
            {
                // Тут мы получаем инфу, обращаясь к массиву цветов вершин,
                // чтобы получить индекс вершин, чтобы получить правильное значение в этом массиве
                // и увеличиваем значение на + v, так как это находится в preText-е, то делаем текст видимым
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v] = colorVisable;
            }
            else
            {
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v] = colorHidden;
            }
        }

        tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // Метод переключения режима на тип "Печатная машинка" typewriter
    private IEnumerator Build_Typewriter()
    {
        while(tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            // Тут мы делаем отображение текста по установленной нами скоростью, с проверкой по флагу, для ускоренного отображения
            tmpro.maxVisibleCharacters += hurryUp ? characterPerCycle * 5 : characterPerCycle;

            // Но чтобы мы видели эффект машинки, то вывод текста будет с задержкой, а не сразу.
            // С помощью yield return WaitForSecond(0.015f / speed)
            // где 0.015f - оптимальное время
            yield return new WaitForSeconds(0.015f / speed);
        }
    }
    // Метод переключения режима на тип "Затухание" fade
    // Но он почему-то немного багованный (игнорит символ "!" и бывают мерцания)
    private IEnumerator Build_Fade()
    {
        // Объявляем границы, для определения границ отображения текста
        int minRange = preTextLength;
        int maxRange = minRange + 1;

        // Так как мы работаем с Color32, то это = 1 byte =>
        byte alphaThreshold = 15;

        // Получаем инфу о тексте
        TMP_TextInfo textInfo = tmpro.textInfo;

        // Снова получаем массив Color32
        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        // Байты не могут содержать 10-ти значения как числа с плавающей точкой
        // Поэтому мы создадим новый массив типа float,
        // который будет содержать значения альфа канала,
        // где размер массива будет означать нужный нам байт
        float[] alphas = new float[textInfo.characterCount];
        
        
        // Пока что вопспользуемся циклом while (Но это опасно)
        while(true)
        {
            // Скорость затухания
            // Пример определения скорости, но для печатной машинки
            // tmpro.maxVisibleCharacters += hurryUp ? characterPerCycle * 5 : characterPerCycle;
            float fadeSpeed = ((hurryUp ? characterPerCycle * 5 : characterPerCycle) * speed) * 4f;

            for (int i = minRange; i < maxRange; i++)
            {
                // Берём инфу по символу
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                // Делаем проверку на отсутствие символа
                if (!charInfo.isVisible)
                    continue;

                // Теперь получаем индекс вершины объекта,
                // чтобы у нас была начальная точка цветов
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Тут мы будем использовать наш массив float[]
                // и будем начинать с указанной точки, вплоть до 255, по определённой скорости
                alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);

                // Теперь мы применяем alphas к каждой вершине и делаем проверку по байту
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];

                if (alphas[i] >= 255)
                    minRange++;
            }

            // Теперь когда закончился цикл for мы обновляем наши вершины и цвета
            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // И увеличиваем макс диапозон, но для начала делаем проверку
            bool lastCharacterIsInvisible = !textInfo.characterInfo[maxRange - 1].isVisible;

            // Делаем проверку последнего символа на видимость или его прозрачность превышает порог
            if (alphas[maxRange - 1] > alphaThreshold || lastCharacterIsInvisible)
            {
                // То мы увеличиваем диапозон
                if (maxRange < textInfo.characterCount)
                    maxRange++;
                else if (alphas[maxRange - 1] >= 255 || lastCharacterIsInvisible)
                    break;
                // Если оно превысило 255, то break
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
