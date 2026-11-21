using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PresentationManager : MonoBehaviour
{
    public Camera arCamera;
    public QuestionManager questionManager;

    [Header("Applause sequence")]
    [Min(1f)] public float applauseTime = 4.8f;
    [Range(0f, 0.3f)] public float handRaiseStagger = 0.10f;
    [Min(0f)] public float animationFinishBuffer = 0.2f;

    public AudioManager audioManager;

    private bool canSelectAudience = false;
    private Coroutine sequenceCoroutine;

    public void DisableAudienceSelection()
    {
        canSelectAudience = false;
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
    }

    void Update()
    {
        if (!canSelectAudience)
            return;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 position = Touchscreen.current.primaryTouch.position.ReadValue();
            if (!IsPointerOverUI(position))
                SelectAudience(position);
        }

        // 한 프레임에 터치와 마우스 입력이 동시에 잡혀도 한 명만 선택.
        if (!canSelectAudience)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 position = Mouse.current.position.ReadValue();
            if (!IsPointerOverUI(position))
                SelectAudience(position);
        }
    }

    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void EndPresentation()
    {
        BeginApplause(true, null);
    }

    public void PrepareNextQuestion()
    {
        BeginApplause(true, null);
    }

    public void PlayFinalApplause(Action onFinished)
    {
        BeginApplause(false, onFinished);
    }

    void BeginApplause(bool raiseHandsAfter, Action onFinished)
    {
        DisableAudienceSelection();
        sequenceCoroutine = StartCoroutine(
            ApplauseSequence(raiseHandsAfter, onFinished));
    }

    IEnumerator ApplauseSequence(bool raiseHandsAfter, Action onFinished)
    {
        AudienceBehavior[] audiences = FindObjectsByType<AudienceBehavior>(
            FindObjectsSortMode.None);

        if (audioManager != null)
            audioManager.PlayApplause();

        float duration = Mathf.Max(1f, applauseTime);
        foreach (AudienceBehavior audience in audiences)
        {
            if (audience != null)
                audience.PlayApplause(duration);
        }

        // 모든 캐릭터가 동일한 박수 구간을 사용한다.
        yield return new WaitForSeconds(duration);

        if (raiseHandsAfter)
        {
            // 아주 짧은 시간차를 두되 모두 손을 다 든 뒤에만 선택 가능.
            float maxRaiseSeconds = 0f;
            foreach (AudienceBehavior audience in audiences)
            {
                if (audience == null)
                    continue;

                audience.RaiseHand();
                maxRaiseSeconds = Mathf.Max(maxRaiseSeconds, audience.raiseHandSeconds);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, handRaiseStagger));
            }

            yield return new WaitForSeconds(
                maxRaiseSeconds + animationFinishBuffer);
            canSelectAudience = true;
        }
        else
        {
            // 마지막 질문은 손을 다시 들지 않고 결과 화면으로 이동.
            if (onFinished != null)
                onFinished();
        }

        sequenceCoroutine = null;
    }

    void SelectAudience(Vector2 screenPosition)
    {
        if (arCamera == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            AudienceBehavior selected =
                hit.collider.GetComponentInParent<AudienceBehavior>();
            if (selected == null)
                return;

            canSelectAudience = false;
            sequenceCoroutine = StartCoroutine(SelectAudienceAfterLowering(selected));
        }
    }

    IEnumerator SelectAudienceAfterLowering(AudienceBehavior selected)
    {
        AudienceBehavior[] audiences = FindObjectsByType<AudienceBehavior>(
            FindObjectsSortMode.None);
        float maxLowerSeconds = 0f;

        foreach (AudienceBehavior audience in audiences)
        {
            if (audience != null && audience != selected)
            {
                audience.LowerHand();
                maxLowerSeconds = Mathf.Max(maxLowerSeconds, audience.lowerHandSeconds);
            }
        }

        // 다른 청중의 손 내리기가 끝난 다음 질문을 표시.
        yield return new WaitForSeconds(maxLowerSeconds + animationFinishBuffer);
        if (selected != null && questionManager != null)
            questionManager.StartQuestion(selected, this);

        sequenceCoroutine = null;
    }
}
