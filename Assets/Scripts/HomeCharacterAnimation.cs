using System.Collections;
using UnityEngine;

public class HomeCharacterAnimation : MonoBehaviour
{
    private Animator animator;

    private string[] applauseTriggers =
    {
        "clap1",
        "clap2",
        "standClap1",
        "standClap2"
    };

    void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
            StartCoroutine(PlayHomeAnimation());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator PlayHomeAnimation()
    {
        // 캐릭터들이 동시에 움직이지 않게 시작 시간 조금 다르게
        yield return new WaitForSeconds(
            Random.Range(0f, 0.8f));

        while (true)
        {
            string trigger =
                applauseTriggers[
                    Random.Range(0, applauseTriggers.Length)
                ];

            animator.SetTrigger(trigger);

            yield return new WaitForSeconds(
                Random.Range(2.5f, 4f));
        }
    }
}