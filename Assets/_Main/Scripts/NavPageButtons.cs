using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NavPageButtons : MonoBehaviour
{
    // Метод перехода со страницы, на страницу
    public void OnButtonClick(GameObject requiredPage)
    {
        // Получаем текущий объект и объект с навигацией
        GameObject curPage = gameObject.transform.parent.gameObject;
        GameObject navBar = curPage.transform.parent.gameObject.transform.GetChild(6).gameObject;

        // Изменяем curPage в navBar.getComponent()
        // Меняем страницу, в NavOverlayData.Update(), с помощью CurrentPage
        //Debug.Log($"requared = {requiredPage}");
        navBar.GetComponent<NavOverlayData>().CurrentPage = requiredPage;

    }
    /// <summary>
    /// Метод закрытия игры
    /// </summary>
    public void OnExitClick() => Application.Quit();

    // Start is called before the first frame update
    //void Start()
    //{
    //    //
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    //
    //}
}
