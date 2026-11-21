using UnityEngine;
using UnityEngine.EventSystems;

public class AudienceRotateHandle :
    MonoBehaviour,
    IDragHandler
{
    public AudienceEditManager editManager;

    public float rotationSpeed = 0.4f;


    public void OnDrag(PointerEventData eventData)
    {
        float amount =
            -eventData.delta.x *
            rotationSpeed;

        editManager.Rotate(amount);
    }
}