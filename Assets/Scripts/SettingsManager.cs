using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject settingsPanel;

    [Header("Time")]
    public TMP_InputField presentationTimeInput;
    public TMP_InputField answerTimeInput;

    [Header("Question")]
    public TMP_InputField questionInputField;
    public GameObject questionItemPrefab;
    public Transform questionListContent;

    [Header("Keyword")]
    public TMP_InputField keywordInputField;
    public GameObject keywordItemPrefab;
    public Transform keywordListContent;

    [Header("Managers")]
    public PresentationTimer presentationTimer;
    public QuestionManager questionManager;

    private List<string> questions = new List<string>();
    private List<GameObject> questionItems = new List<GameObject>();

    private List<string> keywords = new List<string>();
    private List<GameObject> keywordItems = new List<GameObject>();
    [Header("Presentation Keyword Display")]
    public Transform presentationKeywordContent;
    public GameObject presentationKeywordItemPrefab;

    

    private List<GameObject> presentationKeywordItems =
        new List<GameObject>();

    void Start()
{
    settingsPanel.SetActive(false);

    // QuestionManager의 기본 질문을 설정창에도 표시
    foreach (string question in questionManager.questions)
    {
        questions.Add(question);

        GameObject item =
            Instantiate(
                questionItemPrefab,
                questionListContent);

        questionItems.Add(item);

        Button deleteButton =
            item.GetComponentInChildren<Button>();

        deleteButton.onClick.AddListener(
            () => DeleteQuestion(item));
    }

    RefreshQuestionList();
}


    public void OpenSettings()
    {
        // 다시 설정을 열 때 Slider의 초(60, 180...)를 분으로 착각하지 않도록 함.
        presentationTimeInput.SetTextWithoutNotify(
            presentationTimer.GetPresentationMinutes().ToString());
        answerTimeInput.SetTextWithoutNotify(
            Mathf.RoundToInt(questionManager.answerTime).ToString());
        settingsPanel.SetActive(true);
    }


    public void CloseSettings()
    {
        
        settingsPanel.SetActive(false);
    }


    public void AddQuestion()
    {
        string text = questionInputField.text.Trim();

        if (text == "")
            return;

        questions.Add(text);

        GameObject item =
            Instantiate(questionItemPrefab, questionListContent);

        questionItems.Add(item);

        Button deleteButton =
            item.GetComponentInChildren<Button>();

        deleteButton.onClick.AddListener(
            () => DeleteQuestion(item));

        questionInputField.text = "";

        RefreshQuestionList();
    }


    void DeleteQuestion(GameObject item)
    {
        int index = questionItems.IndexOf(item);

        if (index < 0)
            return;

        questions.RemoveAt(index);
        questionItems.RemoveAt(index);

        Destroy(item);

        RefreshQuestionList();
    }


    void RefreshQuestionList()
    {
        for (int i = 0; i < questionItems.Count; i++)
        {
            TMP_Text text =
                questionItems[i].GetComponentInChildren<TMP_Text>();

            text.text =
                "Q" + (i + 1) + ". " + questions[i];
        }
    }


    public void AddKeyword()
    {
        string text = keywordInputField.text.Trim();

        if (text == "")
            return;

        keywords.Add(text);

        GameObject item =
            Instantiate(keywordItemPrefab, keywordListContent);

        keywordItems.Add(item);

        Button deleteButton =
            item.GetComponentInChildren<Button>();

        deleteButton.onClick.AddListener(
            () => DeleteKeyword(item));

        keywordInputField.text = "";

        RefreshKeywordList();
    }


    void DeleteKeyword(GameObject item)
    {
        int index = keywordItems.IndexOf(item);

        if (index < 0)
            return;

        keywords.RemoveAt(index);
        keywordItems.RemoveAt(index);

        Destroy(item);

        RefreshKeywordList();
    }


    void RefreshKeywordList()
    {
        for (int i = 0; i < keywordItems.Count; i++)
        {
            TMP_Text text =
                keywordItems[i].GetComponentInChildren<TMP_Text>();

            text.text = keywords[i];
        }
    }


    public void SaveSettings()
    {
        int presentationMinutes;
        int answerSeconds;

        if (!int.TryParse(
            presentationTimeInput.text,
            out presentationMinutes))
        {
            presentationMinutes = 5;
        }

        if (!int.TryParse(
            answerTimeInput.text,
            out answerSeconds))
        {
            answerSeconds = 60;
        }

        presentationMinutes =
            Mathf.Clamp(presentationMinutes, 1, 60);

        answerSeconds =
            Mathf.Clamp(answerSeconds, 10, 600);

        presentationTimer.SetPresentationTime(
            presentationMinutes);

        questionManager.answerTime =
            answerSeconds;

        if (questions.Count > 0)
        {
            questionManager.questions =
                questions.ToArray();
        }
        RefreshPresentationKeywords();

        settingsPanel.SetActive(false);
    }
    void RefreshPresentationKeywords()
{
    foreach (GameObject item in presentationKeywordItems)
    {
        Destroy(item);
    }

    presentationKeywordItems.Clear();

    foreach (string keyword in keywords)
    {
        GameObject item =
            Instantiate(
                presentationKeywordItemPrefab,
                presentationKeywordContent);

        TMP_Text text =
            item.GetComponentInChildren<TMP_Text>();

        text.text = keyword;

        presentationKeywordItems.Add(item);
    }
}

    public List<string> GetKeywords()
    {
        return keywords;
    }
}