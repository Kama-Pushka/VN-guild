using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Чтобы это не касалось нашего проекта, мы отделяем через namespace, наш набор тестов
// (создаём изоляцию)
namespace Testing
{
    public class Testing_Architect : MonoBehaviour
    {
        // Получаем доступ к диалоговой системе, через DialogueSystem
        DialogueSystem ds;

        // Так как мы хотим создать текстового архитектора, то мы создаём переменную
        TextArchitect architect;

        // public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.typewriter;
        public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.instant;

        // Текст для проверки
        string[] lines = new string[5]
        {
            "Это рандомная диалоговая линия.",
            "Я хочу тебе что-то сказать.",
            "Этот мир имеет сумашедшии места.",
            "Не растраивайся, будь сильнее!",
            "Эта птица? Это самолёт? Нет! - Это Супер Шелти!"
        };

        // Start is called before the first frame update
        void Start()
        {
            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.fade;
            // Замедление текста
            //architect.speed = 0.5f;
        }

        // Update is called once per frame
        void Update()
        {
            if (bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                architect.Stop();
            }

            // + 1 клавиша для остановки архитектора текста
            if (Input.GetKeyDown(KeyCode.S))
            {
                architect.Stop();
            }

            // Строка для теста ускорения
            string longLine = "Это очень длинная строка! Она не имеет никакого смысла и нужная чисто для проверки всякой разной всячины." +
                " Не спрашивайте зачем мне нужна данная очень длинная строка. Я вообще хз, зачем она тут! И да, я не люблю очень большое кол-во текста." +
                " Я попросту устаю читать!";

            // Настраиваем управление
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (architect.isBuilding)
                {
                    // Ускорение при повторном нажатии
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else
                        architect.ForceComplete();
                }
                else
                {
                    architect.Build(longLine);
                    //architect.Build(lines[Random.Range(0, lines.Length)]);
                }
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                architect.Append(longLine);
                //architect.Append(lines[Random.Range(0, lines.Length)]);
            }
        }
    }
}
