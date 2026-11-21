using UnityEngine;

public class CharacterPlacementManager : MonoBehaviour
{
    [Header("Character Prefabs")]
    public GameObject[] characterPrefabs;

    [Header("Bottom Sheet")]
    public BottomSheetDrag bottomSheet;

    [Header("Plane Finder")]
    public GameObject planeFinder;

    private int selectedCharacterIndex = -1;
    private bool isPlacementMode = false;


    void Start()
    {
        // 처음에는 배치 모드 OFF
        if (planeFinder != null)
            planeFinder.SetActive(false);
    }


    // 바텀시트에서 캐릭터 선택
    public void SelectCharacter(int index)
    {
        if (index < 0 ||
            index >= characterPrefabs.Length)
            return;

        selectedCharacterIndex = index;
        isPlacementMode = true;

        // 캐릭터를 선택했을 때만 바닥 배치 가능
        if (planeFinder != null)
            planeFinder.SetActive(true);

        // 선택 후 바텀시트 닫기
        if (bottomSheet != null)
            bottomSheet.Close();

        Debug.Log(
            "배치 모드 ON : " +
            characterPrefabs[index].name);
    }


    // Vuforia On Content Placed에서 호출
    public void PlaceSelectedCharacter(GameObject placedStage)
    {
        // 배치 모드가 아닐 때는 아무것도 생성하지 않음
        if (!isPlacementMode)
            return;

        if (placedStage == null)
            return;

        if (selectedCharacterIndex < 0 ||
            selectedCharacterIndex >= characterPrefabs.Length)
            return;

        GameObject prefab =
            characterPrefabs[selectedCharacterIndex];

        if (prefab == null)
            return;


        Transform characterSlot =
            placedStage.transform.Find("CharacterSlot");

        Transform parent =
            characterSlot != null
                ? characterSlot
                : placedStage.transform;


        GameObject newCharacter =
            Instantiate(
                prefab,
                parent,
                false);

        newCharacter.transform.localPosition =
            Vector3.zero;

        newCharacter.transform.localRotation =
            Quaternion.identity;


        Debug.Log(
            "청중 배치 완료 : " +
            prefab.name);


        // ★ 한 명 배치했으므로 배치모드 종료
        isPlacementMode = false;

        // ★ 바닥을 눌러도 더 이상 새 캐릭터 안 생김
        if (planeFinder != null)
            planeFinder.SetActive(false);
    }


    public bool IsPlacementMode()
    {
        return isPlacementMode;
    }
}