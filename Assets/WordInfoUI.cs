using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordInfoUI : MonoBehaviour
{
    public TMP_Text wordText, titleText, infoText, hintText;
    int showState = 0;

    public WordInfoGrabber grabber;

    WordInfo wordInfo;

    public GameObject wordInfoPanel;

    public GameObject winScreen;

    public Button prevBtn, nextBtn;

    public async void AcceptNewWord(string word)
    {
        hintText.gameObject.SetActive(false);
        showState = 0;
        wordText.text = word;
        wordInfo = await grabber.GetWordInfo(word);

        hintText.text = wordInfo.Hint;

        UpdateWordInfo();
    }

    public void Next()
    {
        showState ++;
        if(showState > 2)
        {
            showState = 2;
        }

        UpdateWordInfo();
    }

    public void Previous()
    {
        showState --;
        if(showState < 0)
        {
            showState = 0;
        }

        UpdateWordInfo();
    }

    void UpdateWordInfo()
    {
        switch(showState)
        {
            case 0:
                titleText.text = "MEANING";
                infoText.text = wordInfo.Meaning;
                prevBtn.interactable = false;
                break;
            case 1:
                titleText.text = "FACT";
                infoText.text = wordInfo.Fact;
                prevBtn.interactable = true;
                break;
            case 2:
                prevBtn.interactable = true;
                DisplayWinScreen();
                break;
        }
    }

    public void DisplayWinScreen()
    {
        wordInfoPanel.SetActive(false);
        winScreen.SetActive(true);
    }

    public void ShowHint()
    {
        hintText.gameObject.SetActive(true);
    }
}
