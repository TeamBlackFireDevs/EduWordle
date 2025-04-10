using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WordInfoGrabber : MonoBehaviour
{
    
    private HuggingFaceAPI _api;

    private void Start()
    {
        _api = new HuggingFaceAPI();
        //GetWordInfo("daunting");
    }

    public async Task<WordInfo> GetWordInfo(string word)
    {
        var inputText = $"Provide only the meaning and one interesting fact about the word \"{word}\". Format your response as:\r\nMeaning: [definition]\r\nFact: [interesting fact]\r\nKeep the total response under 50 words. Do not include any extra text or explanation.";
        var maxTokens = 500;
        var completion = await _api.Query(inputText, maxTokens);
        Debug.Log(completion);

        List<string> splittedInfo = completion.Split("\n").ToList();
        string meaning = splittedInfo[0].Replace("Meaning:","").Trim();
        string fact = splittedInfo[1].Replace("Fact:", "").Trim();

        Debug.Log(meaning);
        Debug.Log(fact);

        WordInfo wordInfo = new WordInfo();
        wordInfo.Word = word;
        wordInfo.Meaning = meaning;
        wordInfo.Fact = fact;

        return wordInfo;
    }
}
