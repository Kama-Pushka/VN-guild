using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    //
    string[] result { get; set; }
    //
    TextMeshProUGUI head {  get; set; }
    TextMeshProUGUI content { get; set; }
    TextMeshProUGUI btn { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Db = LinkDatabase.GetComponent<Database>();
        result = new string[2];

        head = Head.GetComponent<TextMeshProUGUI>();
        content = Content.GetComponent<TextMeshProUGUI>();
        btn = ButtonsContinue.GetComponent<TextMeshProUGUI>();
        //var btnText = ButtonsContinue.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
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
        else
        {
            result = Db.EndInterviewText.Split(";");
            head.text = result[0];
            content.text = result[1];
            content.alignment = TextAlignmentOptions.Baseline;
            btn.enabled = false;
            btn.alignment = TextAlignmentOptions.MidlineRight;
            btn.text = "Продолжить";
        }
    }
    public void Updater()
    {
        if (Db.IsNewGame)
        {
            Update();
        }

    }
}
