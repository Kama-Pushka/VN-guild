using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavOverlayData : MonoBehaviour
{
    // Область для хранения ссылок на все страницы
    public GameObject[] AllPages;
    public GameObject CurrentPage;

    // page.activeInHierarchy - отвечает за скрытие/показ объекта
    // Флаг для проверки
    //bool IsCheckAllPages = false;

    // Start is called before the first frame update
    void Start()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        CurrentPage.SetActive(true);
        foreach (var page in AllPages)
        {
            if (page != CurrentPage)
                page.SetActive(false);
        }
    }
}
