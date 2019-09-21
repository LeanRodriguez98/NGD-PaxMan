using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public enum DisplayedPanel
    {
        mainMenu = 0,
        credits = 1
    }

    public enum Options
    {
        play = 0,
        credits = 1
    }

    [System.Serializable]
    public struct MenuOptions
    {
        public GameObject selectPoint;
        public Options menuOptions;
    }

    public RectTransform selector;
    public MenuOptions[] menuOptions;
    private uint selectorIndex;

    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject creditsPanelSelectPoint;

    private DisplayedPanel displayedPanel;
    void Start()
    {
        selector.transform.SetParent(menuOptions[0].selectPoint.transform);
        selector.localPosition = Vector3.zero;
        selectorIndex = 0;
        displayedPanel = DisplayedPanel.mainMenu;
        mainMenuPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    void Update()
    {
        switch (displayedPanel)
        {
            case DisplayedPanel.mainMenu:
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (selectorIndex > 0)
                    {
                        selectorIndex--;
                        selector.transform.SetParent(menuOptions[selectorIndex].selectPoint.transform);
                        selector.localPosition = Vector3.zero;
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (selectorIndex < menuOptions.Length - 1)
                    {
                        selectorIndex++;
                        selector.transform.SetParent(menuOptions[selectorIndex].selectPoint.transform);
                        selector.localPosition = Vector3.zero;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    switch (menuOptions[selectorIndex].menuOptions)
                    {
                        case Options.play:
                            Play();
                            break;
                        case Options.credits:
                            ShowCredits();
                            break;
                    }
                }
                break;
            case DisplayedPanel.credits:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    BackToMainMenu();
                }
                break;
        }


    }

    public void Play()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void ShowCredits()
    {
        displayedPanel = DisplayedPanel.credits;
        selector.transform.SetParent(creditsPanelSelectPoint.transform);
        selector.localPosition = Vector3.zero;
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        displayedPanel = DisplayedPanel.mainMenu;
        selector.transform.SetParent(menuOptions[selectorIndex].selectPoint.transform);
        selector.localPosition = Vector3.zero;
        mainMenuPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }
}
