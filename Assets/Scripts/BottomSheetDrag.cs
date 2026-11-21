using UnityEngine;
using UnityEngine.EventSystems;

public class BottomSheetDrag :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public RectTransform sheet;
    public Canvas canvas;

    // 닫혀 있을 때 화면에 보일 높이
    public float peekHeight = 100f;

    public float snapSpeed = 12f;

    private float collapsedY;
    private float expandedY = 0f;

    private bool isDragging = false;
    private bool isOpen = false;

    private Vector2 targetPosition;


    void Start()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        CalculatePositions();

        Vector2 position = sheet.anchoredPosition;
        position.y = collapsedY;

        sheet.anchoredPosition = position;
        targetPosition = position;
    }


    void Update()
    {
        if (isDragging)
            return;

        sheet.anchoredPosition =
            Vector2.Lerp(
                sheet.anchoredPosition,
                targetPosition,
                Time.deltaTime * snapSpeed);
    }


    void CalculatePositions()
    {
        float sheetHeight = sheet.rect.height;

        // 패널 대부분을 화면 밑으로 숨기고
        // 위쪽 peekHeight 만큼만 보여줌
        collapsedY =
            -(sheetHeight - peekHeight);

        // 완전히 올라온 상태
        expandedY = 0f;
    }


    public void OnBeginDrag(
        PointerEventData eventData)
    {
        isDragging = true;
    }


    public void OnDrag(
        PointerEventData eventData)
    {
        float deltaY =
            eventData.delta.y /
            canvas.scaleFactor;

        Vector2 position =
            sheet.anchoredPosition;

        position.y += deltaY;

        position.y =
            Mathf.Clamp(
                position.y,
                collapsedY,
                expandedY);

        sheet.anchoredPosition = position;
    }


    public void OnEndDrag(
        PointerEventData eventData)
    {
        isDragging = false;

        float middle =
            (collapsedY + expandedY) / 2f;

        if (sheet.anchoredPosition.y > middle)
        {
            Open();
        }
        else
        {
            Close();
        }
    }


    public void Open()
    {
        isOpen = true;

        targetPosition =
            sheet.anchoredPosition;

        targetPosition.y = expandedY;
    }


    public void Close()
    {
        isOpen = false;

        targetPosition =
            sheet.anchoredPosition;

        targetPosition.y = collapsedY;
    }
}