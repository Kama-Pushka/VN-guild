using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadQuests : MonoBehaviour
{
    [SerializeField]
    GameObject LinkDatabase;
    [SerializeField]
    GameObject LinkQuests;
    [SerializeField]
    NavPageButtons LinkNavButtons;

    [SerializeField]
    GameObject QuestPrefab;
    [SerializeField]
    GameObject LinkQuestsList;

    [SerializeField]
    TextMeshProUGUI Head;
    [SerializeField]
    TextMeshProUGUI Difficult;
    [SerializeField]
    TextMeshProUGUI Content;
    Database Db { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Db = LinkDatabase.GetComponent<Database>();
        //Quest0 = LinkQuests.transform.GetChild(0).gameObject;
        //Quest1 = LinkQuests.transform.GetChild(1).gameObject;
        //TextMeshProUGUI quest0Text = Quest0.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        //TextMeshProUGUI quest1Text = Quest1.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        // Заполняем список квестов названиями из Db
        for (int i = 0; i < Db.AllQuests.Count; i++)
        {
            Quest quest = Db.AllQuests[i];
            GameObject gameObjQuest = Instantiate(QuestPrefab); // LinkQuests.transform.GetChild(i).gameObject
            gameObjQuest.name = $"Quest - {i}";
            gameObjQuest.transform.SetParent(LinkQuestsList.transform);

            TextMeshProUGUI tmpText = gameObjQuest.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            tmpText.text = quest.NameQuest;

            var button = gameObjQuest.GetComponent<Button>();
            button.onClick.AddListener(() => LinkNavButtons.OnQuestClick(tmpText));

            gameObjQuest.transform.localPosition = new Vector3(
                LinkQuestsList.transform.position.x,
                LinkQuestsList.transform.position.y,
                0);
            gameObjQuest.transform.localScale = Vector3.one;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Completion();
    }
    void Completion()
    {
        Head.text = Db.CurQuest.NameQuest;
        Difficult.text = getDifficult(Db.CurQuest.complexityQuest);
        Content.text = Db.CurQuest.Description;
    }

    string getDifficult(Quest.Complexity diff)
    {
        string result = "Сложность квеста: ";
        switch(diff)
        {
            case Quest.Complexity.Easy:
                result += "лёгкая";
                break;
            case Quest.Complexity.Normal:
                result += "обычная";
                break;
            case Quest.Complexity.Hard:
                result += "сложная";
                break;
            default:
                result += "обычная";
                break;
        }
        return result;
    }
}
