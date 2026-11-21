using System.Collections;
using UnityEngine;

public class HomeManager : MonoBehaviour
{
    [Header("Home")]
    public GameObject homePanel;
    public GameObject homeCharacters;
    public GameObject homeBackground;

    [Header("AR")]
    public GameObject audienceBottomSheet;
    public GameObject audienceEditUI;
    public GameObject settingPanel;

    [Header("Manager")]
    public PresentationTimer presentationTimer;


    void Start()
    {
        ShowHome();
    }


    public void StartAR()
    {
        StartCoroutine(StartARRoutine());
    }


    IEnumerator StartARRoutine()
    {
        // 메인 화면 제거
        homePanel.SetActive(false);
        homeCharacters.SetActive(false);
        homeBackground.SetActive(false);

        // 혹시 남은 편집 UI 제거
        audienceEditUI.SetActive(false);
        settingPanel.SetActive(false);

        // 같은 클릭이 기존 발표 시작 버튼까지
        // 전달되는 것을 막기 위해 한 프레임 기다림
        yield return null;

        // AR 준비 화면 표시
       // AR 준비 화면 표시
audienceBottomSheet.SetActive(true);

Debug.Log("HomeManager가 바텀시트를 켰음!");

presentationTimer.EnterReadyMode();
    }


    public void ShowHome()
    {
        homePanel.SetActive(true);
        homeCharacters.SetActive(true);
        homeBackground.SetActive(true);

        audienceBottomSheet.SetActive(false);
        audienceEditUI.SetActive(false);
        settingPanel.SetActive(false);
    }
}