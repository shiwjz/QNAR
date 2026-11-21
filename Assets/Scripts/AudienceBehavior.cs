using System.Collections;
using UnityEngine;

public class AudienceBehavior : MonoBehaviour
{
    private Animator animator;
    private Transform head;

    public float minWaitTime = 5f;
    public float maxWaitTime = 10f;

    public float tiltAngle = 12f;
    public float tiltMoveTime = 0.4f;
    public float tiltHoldTime = 0.7f;
    public float nodLockTime = 1.5f;
    public bool isPresentationActive = false;

    [Header("Applause - check on cat and squirrel only")]
    public bool repeatJumpDuringApplause = false;
    // Cat/Squirrel Animator Override Controller에서 Jump가 들어간 트리거.
    public string jumpTrigger = "clap1";
    [Min(0.2f)] public float jumpRepeatInterval = 0.95f;
    [Min(0.1f)] public float jumpClipSeconds = 0.7f;

    [Header("Hand animation lengths (seconds)")]
    [Min(0f)] public float raiseHandSeconds = 1.0f;
    [Min(0f)] public float lowerHandSeconds = 0.7f;

    private float timer;
    private float nextActionTime;
    private float currentTiltAngle = 0f;
    private bool isBusy = false;
    private Coroutine applauseCoroutine;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning(name + ": Animator를 찾지 못했습니다.");
            return;
        }

        // 동물은 Generic Avatar일 수 있으므로 Humanoid에서만 머리 본을 조회.
        if (animator.isHuman && animator.avatar != null && animator.avatar.isValid)
            head = animator.GetBoneTransform(HumanBodyBones.Head);

        SetNextActionTime();
    }

    void Update()
    {
        if (!isPresentationActive || isBusy || animator == null)
            return;

        timer += Time.deltaTime;
        if (timer >= nextActionTime)
        {
            RandomListeningAction();
            timer = 0f;
            SetNextActionTime();
        }
    }

    void LateUpdate()
    {
        if (head == null)
            return;

        if (Mathf.Abs(currentTiltAngle) > 0.01f)
            head.localRotation = head.localRotation *
                                 Quaternion.Euler(0f, 0f, currentTiltAngle);
    }

    void SetNextActionTime()
    {
        nextActionTime = Random.Range(minWaitTime, maxWaitTime);
    }

    void RandomListeningAction()
    {
        int action = Random.Range(0, 3);
        if (action == 0)
            StartCoroutine(Nod());
        else if (action == 1)
            StartCoroutine(TiltHead());
    }

    IEnumerator Nod()
    {
        isBusy = true;
        animator.SetTrigger("nod");
        yield return new WaitForSeconds(nodLockTime);
        isBusy = false;
    }

    IEnumerator TiltHead()
    {
        isBusy = true;
        float direction = Random.value < 0.5f ? -1f : 1f;
        float targetAngle = tiltAngle * direction;
        float time = 0f;

        while (time < tiltMoveTime)
        {
            time += Time.deltaTime;
            currentTiltAngle = Mathf.Lerp(0f, targetAngle, time / tiltMoveTime);
            yield return null;
        }

        currentTiltAngle = targetAngle;
        yield return new WaitForSeconds(tiltHoldTime);
        time = 0f;

        while (time < tiltMoveTime)
        {
            time += Time.deltaTime;
            currentTiltAngle = Mathf.Lerp(targetAngle, 0f, time / tiltMoveTime);
            yield return null;
        }

        currentTiltAngle = 0f;
        isBusy = false;
    }

    public void RaiseHand()
    {
        StopAllCoroutines(); // 이전 점프/박수가 손들기 이후에 다시 실행되지 않도록 함.
        applauseCoroutine = null;
        isPresentationActive = false;
        isBusy = false;
        currentTiltAngle = 0f;

        if (animator == null)
            return;

        ResetClapTriggers();
        animator.ResetTrigger("lowerHand");
        animator.SetTrigger("RaiseHand");
    }

    public void LowerHand()
    {
        if (animator == null)
            return;
        animator.ResetTrigger("RaiseHand");
        animator.SetTrigger("lowerHand");
    }

    // 기존에 다른 스크립트에서 인자 없이 호출해도 동작하도록 유지.
    public void PlayApplause()
    {
        PlayApplause(4.5f);
    }

    public void PlayApplause(float applauseSeconds)
    {
        StopAllCoroutines();
        applauseCoroutine = null;
        isPresentationActive = false;
        isBusy = false;
        currentTiltAngle = 0f;

        if (animator == null)
            return;

        animator.ResetTrigger("RaiseHand");
        animator.ResetTrigger("lowerHand");
        ResetClapTriggers();

        applauseCoroutine = StartCoroutine(Applause(applauseSeconds));
    }

    void ResetClapTriggers()
    {
        animator.ResetTrigger("clap1");
        animator.ResetTrigger("clap2");
        animator.ResetTrigger("standClap1");
        animator.ResetTrigger("standClap2");
    }

    IEnumerator Applause(float duration)
    {
        float startDelay = Random.Range(0f, 0.22f);
        yield return new WaitForSeconds(startDelay);

        if (!repeatJumpDuringApplause)
        {
            // 사람: 네 가지 박수 중 랜덤 선택. 기존 방식 유지.
            string[] clapTriggers = { "clap1", "clap2", "standClap1", "standClap2" };
            animator.SetTrigger(clapTriggers[Random.Range(0, clapTriggers.Length)]);
            applauseCoroutine = null;
            yield break;
        }

        // 고양이 / 다람쥐: 짧은 점프 애니메이션을 박수 구간 동안 반복.
        // 마지막 점프가 손들기 직전에 잘리지 않도록 여유를 남김.
        float elapsed = startDelay;
        float interval = Mathf.Max(0.2f, jumpRepeatInterval);
        float clipSeconds = Mathf.Max(0.1f, jumpClipSeconds);
        while (elapsed + clipSeconds <= duration + 0.001f)
        {
            animator.ResetTrigger(jumpTrigger);
            animator.SetTrigger(jumpTrigger);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        applauseCoroutine = null;
    }
}
