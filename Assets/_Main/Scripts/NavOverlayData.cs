using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavOverlayData : MonoBehaviour
{
    // Область для хранения ссылок на все страницы
    public GameObject[] AllPages;
    public GameObject CurrentPage;
    public GameObject Database;
    [SerializeField]
    GameObject LayersPage;

    // Start is called before the first frame update
    void Start()
    {
        //LayersPage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Предполагаю, что тут ошибка с отрисовкой блока LAYERS gameObject

        CurrentPage.SetActive(true);
        foreach (var page in AllPages)
        {
            if (page != CurrentPage)
                page.SetActive(false);
        }
    }
}
