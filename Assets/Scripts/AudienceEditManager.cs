using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AudienceEditManager : MonoBehaviour
{
    public Camera arCamera;

    [Header("UI")]
    public GameObject editUI;
    public RectTransform deleteButton;
    public RectTransform rotateHandle;

    [Header("Placement")]
    public CharacterPlacementManager placementManager;

    private AudienceBehavior selectedAudience;

    private bool canEdit = true;


    void Start()
    {
        editUI.SetActive(false);
    }


    void Update()
    {
        if (!canEdit)
            return;

        // 지금 새 캐릭터 배치 중이면 기존 캐릭터 선택 금지
        if (placementManager != null &&
            placementManager.IsPlacementMode())
            return;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 pos =
                Touchscreen.current.primaryTouch.position.ReadValue();

            TrySelectAudience(pos);
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos =
                Mouse.current.position.ReadValue();

            TrySelectAudience(pos);
        }
    }


    void LateUpdate()
    {
        if (selectedAudience == null)
            return;

        Animator animator =
            selectedAudience.GetComponentInChildren<Animator>();

        Vector3 headPosition =
            selectedAudience.transform.position +
            Vector3.up * 1.5f;

        Vector3 feetPosition =
            selectedAudience.transform.position;

        if (animator != null)
        {
            Transform head =
                animator.GetBoneTransform(HumanBodyBones.Head);

            if (head != null)
                headPosition = head.position;
        }

        Vector3 headScreen =
            arCamera.WorldToScreenPoint(headPosition);

        Vector3 feetScreen =
            arCamera.WorldToScreenPoint(feetPosition);

        // 오른쪽 위 X
        deleteButton.position =
            headScreen +
            new Vector3(80f, 80f, 0f);

        // 발밑 회전 핸들
        rotateHandle.position =
            feetScreen +
            new Vector3(0f, -80f, 0f);
    }


    // UI 버튼을 누른 터치는 캐릭터 선택 / 선택 해제에 사용하지 않는다.
    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = screenPosition;
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);
        return hits.Count > 0;
    }

    void TrySelectAudience(Vector2 screenPosition)
    {
        // X 삭제 버튼 등을 누르면 여기서 반환. 선택 상태를 지우지 않아야
        // 버튼의 OnClick -> DeleteSelected()가 같은 캐릭터를 삭제할 수 있다.
        if (IsPointerOverUI(screenPosition))
            return;

        Ray ray =
            arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            AudienceBehavior audience =
                hit.collider.GetComponentInParent<AudienceBehavior>();

            if (audience != null)
            {
                selectedAudience = audience;

                editUI.SetActive(true);

                return;
            }
        }

        // 빈 곳 누르면 선택 해제
        ClearSelection();
    }


    public void Rotate(float amount)
    {
        if (selectedAudience == null)
            return;

        selectedAudience.transform.Rotate(
            0f,
            amount,
            0f,
            Space.World);
    }


    public void DeleteSelected()
    {
        if (selectedAudience == null)
            return;

        Destroy(selectedAudience.gameObject);

        selectedAudience = null;

        editUI.SetActive(false);
    }


    public void ClearSelection()
    {
        selectedAudience = null;

        editUI.SetActive(false);
    }


    public void SetEditingEnabled(bool enabled)
    {
        canEdit = enabled;

        if (!enabled)
            ClearSelection();
    }
}