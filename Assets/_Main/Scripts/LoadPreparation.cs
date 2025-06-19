using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadPreparation : MonoBehaviour
{
    public GameObject LinkDatabase;
    Database Db { get; set; }
    //
    [SerializeField]
    GameObject Head;
    [SerializeField]
    GameObject Content;
    [SerializeField]
    GameObject ButtonsContinue;

    [SerializeField]
    GameObject Journal; // TODO убрать отсюда

    [SerializeField]
    GameObject Nav; // TODO убрать отсюда
    //
    string[] result { get; set; }

    TextMeshProUGUI head {  get; set; }
    TextMeshProUGUI content { get; set; }
    TextMeshProUGUI btn { get; set; }

    // Start is called before the first frame update
    public void StartNew()
    {
        Db = LinkDatabase.GetComponent<Database>();
        result = new string[2];

        head = Head.GetComponent<TextMeshProUGUI>();
        content = Content.GetComponent<TextMeshProUGUI>();
        btn = ButtonsContinue.GetComponent<Button>().transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        //var btnText = ButtonsContinue.GetComponent<TextMeshProUGUI>();
    }

    public void Updater()
    {
        //if (Db.IsNewGame)
        //{
        //    Update();
        //}
        if (!Db.IsEndInterview)
        {
            if (!Db.IsNewGame)
            {
                result = Db.StartingText.Split(";");

                head.text = result[0];
                content.text = result[1];
                btn.alignment = TextAlignmentOptions.Center;
                btn.text = "Начать новую игру";

                Db.IsNewGame = true;
            }
            if (Db.IsInterview)
            {
                result = Db.PreparationText.Split(";");
                head.text = result[0];
                content.text = result[1];
                btn.alignment = TextAlignmentOptions.MidlineRight;
                btn.text = "Продолжить";
            }
        }
        else // TODO добавить if
        {
            result = Db.EndInterviewText.Split(";");
            head.text = result[0];
            content.text = result[1];
            content.alignment = TextAlignmentOptions.Baseline;
            //btn.enabled = false;
            btn.alignment = TextAlignmentOptions.MidlineRight;
            btn.text = "Продолжить";

            var nav = Nav.GetComponent<NavPageButtons>(); // TODO переделать эту функцию изменения листенера кнопки
            var b = ButtonsContinue.GetComponent<Button>();
            ButtonsContinue.transform.parent.GetChild(2).gameObject.SetActive(false); // TODO а что с той кнопкой?
            ButtonsContinue.SetActive(true);
            //b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => nav.OnJournalActivate(Journal)); // TODO в инспектор
        }
    }
}
