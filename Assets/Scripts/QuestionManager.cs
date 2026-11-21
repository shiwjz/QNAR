using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    public GameObject qaPanel;

    // 고정 2D UI: 질문 번호 + 질문 완료 버튼을 옮겨 놓을 별도 패널
    public GameObject qaHudPanel;

    public TMP_Text questionCountText;
    public TMP_Text questionText;

    [Header("Korean Question Font")]
    public TMP_FontAsset koreanQuestionFont;

    public TMP_Text timerText;
    public Slider timeSlider;

    public float answerTime = 60f;
    public AudioManager audioManager;

    [Header("Q&A Position")]
    public Camera arCamera;
    public RectTransform qaPanelRect;

    private AudienceBehavior currentAudience;

    public string[] questions =
    {
        "기존 발표 연습 방식과 어떤 차이가 있나요?",
        "이 콘텐츠에서 AR을 사용한 이유는 무엇인가요?",
        "향후 어떤 기능을 추가하고 싶나요?"
    };

    // 결과 화면
    public GameObject resultPanel;
    public TMP_Text presentationResultText;
    public TMP_Text questionResultText;
    public TMP_Text answerResultText;

    public PresentationTimer presentationTimer;
    public HomeManager homeManager;

    private int currentQuestionIndex = 0;

    private float remainingTime;
    private bool isAnswering = false;

    private float totalAnswerTime = 0f;

    private AudienceBehavior selectedAudience;
    private PresentationManager presentationManager;


    void Start()
    {
        // 한글이 포함된 TMP Font Asset을 Inspector에서 연결했을 때 적용
        if (koreanQuestionFont != null && questionText != null)
            questionText.font = koreanQuestionFont;

        qaPanel.SetActive(false);
        if (qaHudPanel != null)
            qaHudPanel.SetActive(false);
        resultPanel.SetActive(false);
    }


    void Update()
    {
        if (!isAnswering)
            return;

        // 시간이 초과되어도 질문 완료 버튼을 누를 때까지 계속 측정
        remainingTime -= Time.deltaTime;

        // 슬라이더는 0에서 멈추고, 글자만 +00:01 방식으로 증가
        timeSlider.SetValueWithoutNotify(Mathf.Max(0f, remainingTime));
        UpdateTimerText();
    }


    public void StartQuestion(
        AudienceBehavior audience,
        PresentationManager manager)
    {
        // 이번에 선택한 캐릭터 저장
        selectedAudience = audience;
        currentAudience = audience;

        presentationManager = manager;

        qaPanel.SetActive(true);
        if (qaHudPanel != null)
            qaHudPanel.SetActive(true);

        remainingTime = answerTime;
        timerText.gameObject.SetActive(true);
        timeSlider.gameObject.SetActive(true);

        questionCountText.text =
            "질문 " + (currentQuestionIndex + 1) +
            " / " + questions.Length;

        StartCoroutine(
    ShowQuestionRoutine(
        questions[currentQuestionIndex]
    )
);

        timeSlider.minValue = 0f;
        timeSlider.maxValue = answerTime;
        timeSlider.SetValueWithoutNotify(answerTime);
        timeSlider.interactable = false;

        UpdateTimerText();
    }

    IEnumerator ShowQuestionRoutine(string question)
{
    questionText.text = "";

    // 질문 출력 중에는 답변 타이머 정지
    isAnswering = false;

    foreach (char c in question)
    {
        questionText.text += c;

        // 띄어쓰기/문장부호에는 소리 안 냄
        if (!char.IsWhiteSpace(c) &&
            !char.IsPunctuation(c))
        {
            if (audioManager != null)
                audioManager.PlayBabble();
        }

        yield return new WaitForSeconds(0.055f);
    }

    // 질문이 전부 나온 다음 답변 타이머 시작
    remainingTime = answerTime;

    timeSlider.minValue = 0f;
    timeSlider.maxValue = answerTime;
    timeSlider.SetValueWithoutNotify(answerTime);

    UpdateTimerText();

    isAnswering = true;
}


    public void CompleteAnswer()
    {
        // 중복 터치로 질문을 두 번 완료하지 않도록 방어.
        if (!qaPanel.activeSelf || selectedAudience == null)
            return;

        // 글자 출력 중에 완료한 경우 남은 말소리/글자 출력 중지.
        StopAllCoroutines();
        // 제한 시간을 초과한 답변 시간도 결과에 포함
        totalAnswerTime += Mathf.Max(0f, answerTime - remainingTime);
        isAnswering = false;

        AudienceBehavior answeringAudience = selectedAudience;
        selectedAudience = null;
        currentAudience = null;
        currentQuestionIndex++;

        qaPanel.SetActive(false);
        if (qaHudPanel != null)
            qaHudPanel.SetActive(false);
        timerText.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(false);

        // 손 내리기와 박수를 같은 프레임에 보내면 Animator 전환이 충돌할 수 있음.
        StartCoroutine(FinishAnswerRoutine(answeringAudience));
    }


    IEnumerator FinishAnswerRoutine(AudienceBehavior answeringAudience)
    {
        float lowerDuration = 0f;
        if (answeringAudience != null)
        {
            answeringAudience.LowerHand();
            lowerDuration = answeringAudience.lowerHandSeconds;
        }

        yield return new WaitForSeconds(lowerDuration + 0.15f);

        if (presentationManager == null)
        {
            Debug.LogError("QuestionManager: PresentationManager 연결을 확인하세요.");
            yield break;
        }

        if (currentQuestionIndex < questions.Length)
        {
            // 첫 번째, 두 번째 질문 후 모두 동일하게 박수 -> 손들기.
            presentationManager.PrepareNextQuestion();
        }
        else
        {
            // 마지막 질문: 박수를 끝까지 보여준 후 결과 화면으로.
            presentationManager.PlayFinalApplause(ShowResult);
        }
    }


    public void RetryPresentation()
    {
        StopAllCoroutines();

        resultPanel.SetActive(false);
        qaPanel.SetActive(false);
        if (qaHudPanel != null)
            qaHudPanel.SetActive(false);

        currentQuestionIndex = 0;
        totalAnswerTime = 0f;
        remainingTime = 0f;
        isAnswering = false;

        selectedAudience = null;
        currentAudience = null;

        presentationTimer.ResetForRetry();
    }


    void ShowResult()
    {
        resultPanel.SetActive(true);

        float presentationTime =
            presentationTimer.GetElapsedPresentationTime();

        presentationResultText.text =
            "발표 시간  " +
            FormatTime(presentationTime);

        questionResultText.text =
            "질문 수  " +
            questions.Length + "개";

        answerResultText.text =
            "총 답변 시간  " +
            FormatTime(totalAnswerTime);
    }


    void UpdateTimerText()
    {
        // 표시용 초를 올림하여 00:00 다음 +00:01로 자연스럽게 전환
        if (remainingTime > 0f)
        {
            timerText.text = FormatTime(Mathf.CeilToInt(remainingTime));
        }
        else if (remainingTime < 0f)
        {
            timerText.text = "+" + FormatTime(Mathf.CeilToInt(-remainingTime));
        }
        else
        {
            timerText.text = "00:00";
        }
    }

    public void GoHome()
    {
        StopAllCoroutines();

        resultPanel.SetActive(false);
        qaPanel.SetActive(false);
        if (qaHudPanel != null)
            qaHudPanel.SetActive(false);

        currentQuestionIndex = 0;
        totalAnswerTime = 0f;
        remainingTime = 0f;
        isAnswering = false;

        selectedAudience = null;
        currentAudience = null;

        presentationTimer.ResetForHome();
        homeManager.ShowHome();
    }


    string FormatTime(float time)
    {
        int minutes =
            Mathf.FloorToInt(time / 60f);

        int seconds =
            Mathf.FloorToInt(time % 60f);

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds);
    }


    void LateUpdate()
    {
        if (currentAudience == null)
            return;

        if (!qaPanel.activeSelf)
            return;

        Animator animator =
            currentAudience.GetComponentInChildren<Animator>();

        Vector3 worldPosition;

        if (animator != null)
        {
            Transform head =
                animator.GetBoneTransform(
                    HumanBodyBones.Head);

            if (head != null)
            {
                worldPosition =
                    head.position +
                    Vector3.up * 0.25f;
            }
            else
            {
                worldPosition =
                    currentAudience.transform.position +
                    Vector3.up * 1.5f;
            }
        }
        else
        {
            worldPosition =
                currentAudience.transform.position +
                Vector3.up * 1.5f;
        }

        Vector3 screenPosition =
            arCamera.WorldToScreenPoint(
                worldPosition);

        qaPanelRect.position =
            screenPosition +
            new Vector3(180f, 30f, 0f);
    }
}