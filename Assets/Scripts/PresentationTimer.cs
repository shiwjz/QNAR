using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PresentationTimer : MonoBehaviour
{
    public Slider timeSlider;
    public TMP_Text timerText;
    public TMP_Text countdownText;

    public GameObject startButton;
    public GameObject settingButton;
    public GameObject restartButton;
    public GameObject endPresentationButton;

    public PresentationManager presentationManager;
    public AudioManager audioManager;
    public AudienceEditManager audienceEditManager;

    public GameObject planeFinder;
    public GameObject audienceBottomSheet;

    [Header("Time Settings")]
    [Min(1)] public int defaultPresentationMinutes = 3;

    // 발표 중 키워드 표시 영역
    public GameObject presentationKeywordScrollView;

    private float selectedTime;
    private float remainingTime;
    private float elapsedPresentationTime = 0f;
    private bool isPresenting = false;


    public float GetElapsedPresentationTime()
    {
        return elapsedPresentationTime;
    }

    public int GetPresentationMinutes()
    {
        // 설정 화면에는 초 단위 Slider 값이 아니라 '분' 값만 전달.
        if (selectedTime <= 0f)
            return Mathf.Clamp(defaultPresentationMinutes, 1, 60);
        return Mathf.Clamp(Mathf.RoundToInt(selectedTime / 60f), 1, 60);
    }


    void Start()
    {
        presentationKeywordScrollView.SetActive(false);

        // 슬라이더는 남은 시간 표시용(초)이며, 시작 시간은 분 단위로 설정
        SetPresentationTime(defaultPresentationMinutes);
        timeSlider.interactable = false;

        // 앱 처음 실행 = 메인 화면
        timerText.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        startButton.SetActive(false);
        settingButton.SetActive(false);
        restartButton.SetActive(false);
        endPresentationButton.SetActive(false);

        // 메인 화면에서는 캐릭터 배치 불가
        planeFinder.SetActive(false);
    }


    void Update()
    {
        if (!isPresenting)
            return;

        // 제한 시간이 지나도 발표를 자동 종료하지 않고 초과 시간을 기록
        remainingTime -= Time.deltaTime;
        elapsedPresentationTime += Time.deltaTime;

        // 슬라이더는 0에서 멈추되, 숫자 타이머는 초과 시간을 표시
        timeSlider.SetValueWithoutNotify(Mathf.Max(0f, remainingTime));
        UpdateTimerText();
    }


    // 메인 화면의 "시작하기"를 눌렀을 때 호출
    public void EnterReadyMode()
    {
        StopAllCoroutines();

        isPresenting = false;
        remainingTime = selectedTime;
        elapsedPresentationTime = 0f;

        if (audienceEditManager != null)
            audienceEditManager.SetEditingEnabled(true);

        timerText.gameObject.SetActive(true);
        timeSlider.gameObject.SetActive(true);

        timeSlider.minValue = 0f;
        timeSlider.maxValue = selectedTime;
        timeSlider.SetValueWithoutNotify(selectedTime);
        timeSlider.interactable = false;

        UpdateTimerText();

        startButton.SetActive(true);
        settingButton.SetActive(true);

        restartButton.SetActive(false);
        endPresentationButton.SetActive(false);

        countdownText.gameObject.SetActive(false);
        presentationKeywordScrollView.SetActive(false);

        SetAudiencePresentation(false);

        // 여기서는 아직 배치할 캐릭터를 선택하지 않았으므로 OFF
        planeFinder.SetActive(false);
    }


    public void ChangePresentationTime(float value)
    {
        // UI에서 분 값을 넘길 때만 사용. 시간 표시 슬라이더에는 연결하지 않음.
        if (!isPresenting)
            SetPresentationTime(Mathf.Max(1, Mathf.RoundToInt(value)));
    }


    public void StartPresentation()
    {
        // 발표 시작부터 청중 배치/편집 금지
        planeFinder.SetActive(false);
        if (audienceBottomSheet != null)
            audienceBottomSheet.SetActive(false);
        if (audienceEditManager != null)
            audienceEditManager.SetEditingEnabled(false);
        if (presentationManager != null)
            presentationManager.DisableAudienceSelection();

        StopAllCoroutines();
        StartCoroutine(StartCountdown());
    }


    public void RestartPresentation()
    {
        StopAllCoroutines();

        isPresenting = false;
        elapsedPresentationTime = 0f;

        if (audienceBottomSheet != null)
            audienceBottomSheet.SetActive(false);
        if (audienceEditManager != null)
            audienceEditManager.SetEditingEnabled(false);
        if (presentationManager != null)
            presentationManager.DisableAudienceSelection();

        presentationKeywordScrollView.SetActive(false);

        remainingTime = selectedTime;

        timeSlider.minValue = 0f;
        timeSlider.maxValue = selectedTime;
        timeSlider.SetValueWithoutNotify(selectedTime);

        UpdateTimerText();

        SetAudiencePresentation(false);

        StartCoroutine(StartCountdown());
    }


    public void ResetForRetry()
    {
        StopAllCoroutines();

        presentationKeywordScrollView.SetActive(false);

        isPresenting = false;
        remainingTime = selectedTime;
        elapsedPresentationTime = 0f;

        if (audienceBottomSheet != null)
            audienceBottomSheet.SetActive(false);
        if (audienceEditManager != null)
            audienceEditManager.SetEditingEnabled(false);
        if (presentationManager != null)
            presentationManager.DisableAudienceSelection();

        timerText.gameObject.SetActive(true);
        timeSlider.gameObject.SetActive(true);

        timeSlider.minValue = 0f;
        timeSlider.maxValue = selectedTime;
        timeSlider.SetValueWithoutNotify(selectedTime);
        timeSlider.interactable = false;

        UpdateTimerText();

        startButton.SetActive(true);
        settingButton.SetActive(true);
        restartButton.SetActive(false);
        endPresentationButton.SetActive(false);

        countdownText.gameObject.SetActive(false);

        SetAudiencePresentation(false);

        // 같은 청중으로 다시 연습하므로
        // 기존 청중은 유지하고 배치만 잠금
        planeFinder.SetActive(false);
    }


    IEnumerator StartCountdown()
    {
        isPresenting = false;
        elapsedPresentationTime = 0f;

        if (audienceBottomSheet != null)
            audienceBottomSheet.SetActive(false);
        if (audienceEditManager != null)
            audienceEditManager.SetEditingEnabled(false);

        presentationKeywordScrollView.SetActive(false);

        startButton.SetActive(false);
        settingButton.SetActive(false);

        restartButton.SetActive(true);
        endPresentationButton.SetActive(false);

        countdownText.gameObject.SetActive(true);

        SetAudiencePresentation(false);

        for (int i = 5; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            if (audioManager != null)
                audioManager.PlayCountdown();
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "START!";
        if (audioManager != null)
            audioManager.PlayStart();

        yield return new WaitForSeconds(0.7f);

        countdownText.gameObject.SetActive(false);

        remainingTime = selectedTime;

        timeSlider.minValue = 0f;
        timeSlider.maxValue = selectedTime;
        timeSlider.SetValueWithoutNotify(selectedTime);
        timeSlider.interactable = false;

        UpdateTimerText();

        isPresenting = true;

        restartButton.SetActive(true);
        endPresentationButton.SetActive(true);

        presentationKeywordScrollView.SetActive(true);

        SetAudiencePresentation(true);

        Debug.Log("발표 타이머 시작");
    }


    public void FinishPresentation()
    {
        if (!isPresenting)
            return;

        isPresenting = false;

        presentationKeywordScrollView.SetActive(false);

        SetAudiencePresentation(false);

        restartButton.SetActive(false);
        endPresentationButton.SetActive(false);

        timerText.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(false);

        presentationManager.EndPresentation();
    }


    void SetAudiencePresentation(bool active)
    {
        AudienceBehavior[] audiences =
            FindObjectsByType<AudienceBehavior>(
                FindObjectsSortMode.None);

        foreach (AudienceBehavior audience in audiences)
        {
            audience.isPresentationActive = active;
        }
    }


    public void ResetForHome()
    {
        StopAllCoroutines();

        isPresenting = false;
        remainingTime = selectedTime;
        elapsedPresentationTime = 0f;

        if (audienceBottomSheet != null)
            audienceBottomSheet.SetActive(false);
        if (audienceEditManager != null)
            audienceEditManager.SetEditingEnabled(false);
        if (presentationManager != null)
            presentationManager.DisableAudienceSelection();

        UpdateTimerText();

        // 메인 화면으로 돌아가므로 AR UI 전부 숨김
        timerText.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        startButton.SetActive(false);
        settingButton.SetActive(false);
        restartButton.SetActive(false);
        endPresentationButton.SetActive(false);

        presentationKeywordScrollView.SetActive(false);

        SetAudiencePresentation(false);

        planeFinder.SetActive(false);

        // 실제 AR에 배치했던 청중 삭제
        AudienceBehavior[] audiences =
            FindObjectsByType<AudienceBehavior>(
                FindObjectsSortMode.None);

        foreach (AudienceBehavior audience in audiences)
        {
            Destroy(audience.gameObject);
        }
    }


    public void SetPresentationTime(int minutes)
    {
        selectedTime = Mathf.Clamp(minutes, 1, 60) * 60f;
        remainingTime = selectedTime;

        timeSlider.minValue = 0f;
        timeSlider.maxValue = selectedTime;
        timeSlider.SetValueWithoutNotify(selectedTime);

        UpdateTimerText();
    }


    void UpdateTimerText()
    {
        bool isOvertime = remainingTime < 0f;
        int displaySeconds = Mathf.CeilToInt(Mathf.Abs(remainingTime));
        int minutes = displaySeconds / 60;
        int seconds = displaySeconds % 60;

        timerText.text = string.Format(
            isOvertime ? "+{0:00}:{1:00}" : "{0:00}:{1:00}",
            minutes,
            seconds);
    }
}