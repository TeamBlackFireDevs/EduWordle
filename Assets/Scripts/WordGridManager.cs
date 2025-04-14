using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordGridManager : MonoBehaviour {

    public const string NUMBEROFPUZZLESPLAYED = "NUMBERPUZZLES", NUMBERWINS = "NUMBERWIN", WIN1 = "WIN1", WIN2 = "WIN2", WIN3 = "WIN3", WIN4 = "WIN4", WIN5 = "WIN5", WIN6 = "WIN6", CURRENTSTREAK = "CURRENTSTREAK", LARGESTSTREAK = "LARGESTSTREAK", HARDMODE = "HARDMODE", DICTIONARYCHECK = "DICTIONARYCHECK", MONTH = "MONTH", DAY = "DAY", YEAR = "YEAR", SELECTEDLENGTH = "SELECTEDLENGTH1", NUMBEROFGUESSES = "NUMBEROFGUESSES", SOUNDVOL = "SOUNDVOL", MUSICVOL = "MUSICVOL";



    public GameMasterManager manager;
    public string currentWord = "";
    public Transform wordRowGroupingTransform;
    public int currentRow;
    public CanvasKeyboard.CanvasKeyboard keyboard;
    public StateMachine<WordGridManager> controller;
    public Canvas canvas;
    [Header("Word Rows")]
    public VerticalLayoutGroup vertGroup;
    public WordRow wordRowPrefab;
    public List<WordRow> rows = new List<WordRow>();
    public RectTransform keyboardParent;
    [Header("WinScreen")]
    public List<GuessDistributionBar> bars = new List<GuessDistributionBar>();
    public GameObject winScreen;
    public WordInfoUI wordInfoUI;
    public GameObject guessDistribution;
    public TextMeshProUGUI NumTotal, winPercentText, currentStreakText, largestStreakText, winTitle, lossText;
    public float winLingerTime = 1f;
    public GameObject BackToWinScreenButton;
    public float maxColumnHeight = 1160f;
    public GameObject mainMenuButton;
    public Transform backTransform;
    [Header("Word Text")]
    public GameObject errorWindow;
    public TextMeshProUGUI errorText;
    public float errorLingerTime = 0.5f;
    
    float errorTimer = 0;
    bool showingError = false;

    public HashSet<char> necessaryCharacter = new HashSet<char>();

    public GameObject yellowKeySound, greenKeySound;
    public AudioSource winSound, loseSound;

    private void Awake() {
        greenEmoji = char.ConvertFromUtf32(0x1F7E9);
        yellowEmoji = char.ConvertFromUtf32(0x1F7E8);
        emptyEmoji = char.ConvertFromUtf32(0x2B1B);
    }


    public void Update() {
        if (showingError) {
            if (errorTimer < Time.time) {
                showingError = false;
                errorWindow.SetActive(false);
            }
        }
    }

    bool isDaily;

    public const float  defaultGridSize = 200f;


    public void BackToMainMenu() {

        SetInt(CURRENTSTREAK, 0);
        manager.GoToMainMenu();
    
    
    }


    public void Setup(string s, bool isDaily) {
        print("Screen Width " + canvas.GetComponent<RectTransform>().rect.width);
        // PlayerPrefs.DeleteAll();

        wordInfoUI.AcceptNewWord(s);
        
        currentWord = s;
        winScreen.SetActive(false);
        BackToWinScreenButton.SetActive(false);
        vertGroup.enabled = true;
        
        print(currentWord);
        errorWindow.SetActive(false);
        int numberPlayed = GetInt(NUMBEROFPUZZLESPLAYED) + 1;
        SetInt(NUMBEROFPUZZLESPLAYED, numberPlayed);
        keyboard.gameObject.SetActive(true);
        currentWord = currentWord.ExceptChars(new List<char> { ' ', '\n', '\t' });
        currentWord = currentWord.Substring(0, currentWord.Length);
        CreateWordRows();
        currentRow = 0;
        keyboard.maxBuildStringSize = currentWord.Length;
        keyboard.minBuildString = currentWord.Length;
        keyboard.SetBuildString("");
        controller = new StateMachine<WordGridManager>(new EnterWordState(), this);
        keyboard.SetAllKeysNormal();
        necessaryCharacter = new HashSet<char>();

        float screenWidth = canvas.GetComponent<RectTransform>().rect.width;
        float gridsize = defaultGridSize;
        float rowLength = (defaultGridSize + 10) * currentWord.Length;
       // print(rowLength);
        if (rowLength > screenWidth) {
            gridsize = (int)((screenWidth - (10 * currentWord.Length))/currentWord.Length);
            print(gridsize);
        }

        float columnlength = (gridsize + 10) * GameMasterManager.numberOfGuesses;
        print("Column Length: " + columnlength.ToString());
        if (columnlength > maxColumnHeight) {

            float currGridSize = gridsize;
            gridsize = (maxColumnHeight - (10 * GameMasterManager.numberOfGuesses)) / GameMasterManager.numberOfGuesses;


        }



        foreach (WordRow w in rows) {
            w.SetGridSize(gridsize);
        }

        if (screenWidth < 1080) {
            keyboardParent.transform.localScale = new Vector3(screenWidth / 1080f, screenWidth / 1080f, 1);
        } else {
            keyboardParent.transform.localScale = Vector3.one;
        }

    }

    public void ShowError(string s) {
        errorWindow.SetActive(true);
        errorText.SetText(s);
        errorTimer = Time.time + errorLingerTime;
        showingError = true;
    }

    public void SetInputString(string s) {
        if (canEnterWords) {
            rows[currentRow].SetTypeText(s);
        }
    }


    public IEnumerator LayoutWorkAround() {
        yield return null;
        for (int i = 0; i < GameMasterManager.numberOfGuesses; i++) {
            rows[i].gameObject.SetActive(true);
        }
        yield return null;
        vertGroup.enabled = false;
    }

    public void CreateWordRows() {

        int toadd = GameMasterManager.numberOfGuesses - rows.Count;
        for (int i = 0; i < toadd; i++) {
            WordRow row = Instantiate(wordRowPrefab, wordRowGroupingTransform);
            rows.Add(row);
        }

        foreach (WordRow r in rows) {
            r.gameObject.SetActive(false);
        }

        for (int i = 0; i < GameMasterManager.numberOfGuesses; i++) {
            rows[i].SetSize(currentWord.Length);
        }
        backTransform.SetSiblingIndex(backTransform.parent.childCount);
        StartCoroutine(LayoutWorkAround());

    }

    


    public void PlayerPressEnter() {

        if (canEnterWords) {
            print("here");
            bool ddddd = true;
            string s = keyboard.buildString.ToUpper();
            if (GetBool(DICTIONARYCHECK) && currentWord.Length < 6 && !GameMasterManager.answerWords[currentWord.Length].Contains(s) && !GameMasterManager.commonWords[currentWord.Length].Contains(s)) {
                rows[currentRow].Shake();
                ShowError("Not in word list");
                ddddd = false;
            } 
            
            if (ddddd && GetBool(HARDMODE)) {

                foreach (char c in necessaryCharacter) {
                    if (!s.Contains(c)) {
                        ddddd = false;
                    }
                }
                if (!ddddd) {
                    rows[currentRow].Shake();
                    ShowError("Must contain all previously revealed characters");
                }

            }

            
            if (ddddd) {
                controller.ChangeState(new WordAnimationState());
            }
        }


    }

    public bool canEnterWords = false;

    public float flipSpeed = 2f;


    public bool YellowCheck(string input, int i) {

        char a = input[i];


        for (int x = 0; x < input.Length; x++) {
            if (x != i && currentWord[x] == a) {
                if (input[x] != currentWord[x]) {
                    return true;
                }
            }
        }

        return false;
    }


    public IEnumerator WordAnimationFlip() {


        yield return null;
        string inputString = keyboard.buildString.ToUpper();

        for (int i = 0; i < currentWord.Length; i++) {

            WordGridButton currentButton = rows[currentRow].wordgridButtons[i];

            while (currentButton.transform.localScale.y != 0) {
                currentButton.transform.localScale = new Vector3(1, Mathf.MoveTowards(currentButton.transform.localScale.y, 0, flipSpeed * Time.deltaTime), 1);
                yield return null;
            }

            print(inputString[i].ToString());
            print(currentWord[i].ToString());

            if (inputString[i] == currentWord[i]) {
                currentButton.SetCorrect(inputString[i]);
                keyboard.GreenChar(inputString[i]);
                Instantiate(greenKeySound);
                //greenKeySound.Play();
                necessaryCharacter.Add(inputString[i]);
            } else if (currentWord.Contains(inputString[i]) && YellowCheck(inputString, i)) {
                currentButton.SetSemiCorrect(inputString[i]);
                keyboard.YellowChar(inputString[i]);
                Instantiate(yellowKeySound);
                //yellowKeySound.Play();
                necessaryCharacter.Add(inputString[i]);
            } else {
                currentButton.SetMissing(inputString[i]);
                keyboard.EmptyChar(inputString[i]);
            }

            while (currentButton.transform.localScale.y != 1) {
                currentButton.transform.localScale = new Vector3(1, Mathf.MoveTowards(currentButton.transform.localScale.y, 1, flipSpeed * Time.deltaTime), 1);
                yield return null;
            }

        }

        currentRow++;
        keyboard.SetBuildString("");

        print(inputString + " : " + currentWord);
        

        if (inputString == currentWord) {
           
            controller.ChangeState(new WinState(true));
            winSound.Play();
        } else if (currentRow < GameMasterManager.numberOfGuesses) {
            controller.ChangeState(new EnterWordState());
        } else {
            controller.ChangeState(new WinState(false));
            loseSound.Play();
        }
        
    }

    string emptyEmoji = "\u1567", greenEmoji = "\u1563", yellowEmoji = "\u1562";


    public void EmojiCopy () {
        string result = "";
        for (int i = 0; i < currentRow; i++) {
            WordRow row = rows[i];
            for (int j = 0; j < row.currentSize; j++) {
                WordGridButton button = row.wordgridButtons[j];
                switch (button.state) {
                    case WORDBUTTONSTATE.GREEN:
                        result += greenEmoji;
                        break;
                    case WORDBUTTONSTATE.YELLOW:
                        result += yellowEmoji;
                        break;
                    case WORDBUTTONSTATE.EMPTY:
                        result += emptyEmoji;
                        break;
                }
            }
            result += '\n';
        }
        try { 
        result.CopyToClipboard();
        } catch {

        }
        try { 
        ClipboardExtension.CopyToClipboard(result);
        } catch {

        }
        try {
            TextEditor t = new TextEditor();
            t.text = result;
            t.SelectAll();
            t.Copy();
        } catch {

        }

        try {
#if UNITY_WEBGL
            passCopyToBrowser(result);
#endif
        } catch {
            
        }

    }


#if UNITY_WEBGL

    class StringCallback {

    }


    [DllImport("__Internal")]
    private static extern void initWebGLCopyAndPaste(StringCallback cutCopyCallback, StringCallback pasteCallback);
    [DllImport("__Internal")]
    private static extern void passCopyToBrowser(string str);
#endif


    public void SetInt(string s, int val) {
        PlayerPrefs.SetInt(s, val);
    }

    public bool HasInt(string s) {
        return PlayerPrefs.HasKey(s);
    }

    public int GetInt(string s) {
        return PlayerPrefs.GetInt(s);
    }

    public void SetFloat(string s, float val)
    {
        PlayerPrefs.SetFloat(s, val);
    }

    public bool HasFloat(string s)
    {
        return PlayerPrefs.HasKey(s);
    }

    public float GetFloat(string s)
    {
        return PlayerPrefs.GetFloat(s);
    }


    public void SetBool(string s, bool b) {
        if (b)
        PlayerPrefs.SetInt(s, 1);
        else
        PlayerPrefs.SetInt(s, 0);
    }

    public bool HasBool(string s) {
        return PlayerPrefs.HasKey(s);
    }

    public bool GetBool(string s) {
        if (PlayerPrefs.GetInt(s) > 0) {
            return true;
        } else {
            return false;
        }
    }


    public void HideWinScreen() {
        BackToWinScreenButton.SetActive(true);
        winScreen.gameObject.SetActive(false);
    }

    public void ReShowWinScreen() {
        BackToWinScreenButton.SetActive(false);
        winScreen.gameObject.SetActive(true);
    }


    public void PlayAgain() {
        Setup(manager.GetWord(), false); ;
    }

}


public class EnterWordState : State<WordGridManager> {
    public override void Enter(StateMachine<WordGridManager> obj) {
        obj.target.keyboard.SetBuildString("");
        obj.target.mainMenuButton.SetActive(true);
        obj.target.canEnterWords = true;
        obj.target.keyboard.ReEnable();
        obj.target.keyboard.doneButton.gameObject.SetActive(true);
    }

    public override void Exit(StateMachine<WordGridManager> obj) {
        obj.target.canEnterWords = false;
        obj.target.mainMenuButton.SetActive(false);
        obj.target.keyboard.doneButton.gameObject.SetActive(false);
    }
}




public class WordAnimationState : State<WordGridManager> {

    

        public override void Enter(StateMachine<WordGridManager> obj) {
            obj.target.canEnterWords = false;
            obj.target.StartCoroutine(obj.target.WordAnimationFlip());
        }
    }

public class WinState : State<WordGridManager> {


    public bool win = true;

    public WinState(bool b) {
        win = b;
    }

    public IEnumerator ShowScreen(StateMachine<WordGridManager> obj) {

        yield return new WaitForSeconds(obj.target.winLingerTime);

        obj.target.errorWindow.SetActive(false);
        if (win) {
            obj.target.winTitle.SetText("You Win!");
            int toAdd = 0;
            if (obj.target.currentRow == 1) {
                toAdd = obj.target.GetInt(WordGridManager.WIN1) + 1;
                obj.target.SetInt(WordGridManager.WIN1, toAdd );
            } else if (obj.target.currentRow == 2) {
                toAdd = obj.target.GetInt(WordGridManager.WIN2) + 1;
                obj.target.SetInt(WordGridManager.WIN2, toAdd);
            } else if (obj.target.currentRow == 3) {
                toAdd = obj.target.GetInt(WordGridManager.WIN3) + 1;
                obj.target.SetInt(WordGridManager.WIN3, toAdd);
            } else if (obj.target.currentRow == 4) {
                toAdd = obj.target.GetInt(WordGridManager.WIN4) + 1;
                obj.target.SetInt(WordGridManager.WIN4, toAdd);
            } else if (obj.target.currentRow == 5) {
                toAdd = obj.target.GetInt(WordGridManager.WIN5) + 1;
                obj.target.SetInt(WordGridManager.WIN5, toAdd);
            } else if (obj.target.currentRow == 6) {
                toAdd = obj.target.GetInt(WordGridManager.WIN6) + 1;
                obj.target.SetInt(WordGridManager.WIN6,  toAdd);
            }
        } else {
            obj.target.winTitle.SetText("You Lose!");
        }
        yield return null;
        obj.target.guessDistribution.SetActive(win);
        int currentStreak = 0;

        if (win) {
            currentStreak = obj.target.GetInt(WordGridManager.CURRENTSTREAK) + 1;
        }

        obj.target.lossText.SetText("Word was: " + obj.target.currentWord);
        obj.target.lossText.gameObject.SetActive(!win);

        obj.target.SetInt(WordGridManager.CURRENTSTREAK, currentStreak);
        if (currentStreak > obj.target.GetInt(WordGridManager.LARGESTSTREAK)) {
            obj.target.SetInt(WordGridManager.LARGESTSTREAK, currentStreak);
        }

        float numberOfWins = obj.target.GetInt(WordGridManager.NUMBERWINS);
        if (win) numberOfWins++;
        obj.target.SetInt(WordGridManager.NUMBERWINS, (int)numberOfWins);
        float winPercent = (numberOfWins) / ((float)obj.target.GetInt(WordGridManager.NUMBEROFPUZZLESPLAYED));

        obj.target.currentStreakText.SetText(obj.target.GetInt(WordGridManager.CURRENTSTREAK).ToString());
        obj.target.largestStreakText.SetText(obj.target.GetInt(WordGridManager.LARGESTSTREAK).ToString());
        obj.target.NumTotal.SetText(obj.target.GetInt(WordGridManager.NUMBEROFPUZZLESPLAYED).ToString());
        obj.target.winPercentText.SetText((winPercent * 100).ToString("0.0") + "%");


        for (int i = 0; i < 6; i++) {
            float numberWinsGuess = obj.target.GetInt("WIN" + (i + 1).ToString());
            float percent = (numberWinsGuess) / numberOfWins;
            obj.target.bars[i].Set(numberWinsGuess, percent);
        }
        obj.target.keyboard.gameObject.SetActive(false);

        foreach (Transform child in obj.target.transform)
        {
            child.gameObject.SetActive(false);
        }
        obj.target.wordInfoUI.wordInfoPanel.SetActive(true);
        //obj.target.winScreen.SetActive(true);

        obj.target.BackToWinScreenButton.SetActive(false);
    }

    public override void Enter(StateMachine<WordGridManager> obj) {


        obj.target.StartCoroutine(ShowScreen(obj));
        LastPuzzle l = new LastPuzzle();
        l.grid = new WORDBUTTONSTATE[obj.target.currentWord.Length, obj.target.currentRow];
        
        for (int y = 0; y < obj.target.currentRow; y++) {
            for (int x = 0; x < obj.target.currentWord.Length;x++) {
            
                switch (obj.target.rows[y].wordgridButtons[x].state) {
                    case WORDBUTTONSTATE.GREEN:
                        l.grid[x, y] = WORDBUTTONSTATE.GREEN;
                        break;
                    case WORDBUTTONSTATE.YELLOW:
                        l.grid[x, y] = WORDBUTTONSTATE.YELLOW;
                        break;
                    case WORDBUTTONSTATE.EMPTY:
                        l.grid[x, y] = WORDBUTTONSTATE.EMPTY;
                        break;
                }
            }
        }
        obj.target.manager.SaveLastPuzzle(l);


    }
}


public class LoseState : State<WordGridManager> {

}


public static class ClipboardExtension {
    /// <summary>
    /// Puts the string into the Clipboard.
    /// </summary>
    public static void CopyToClipboard(this string str) {
        GUIUtility.systemCopyBuffer = str;
    }
}