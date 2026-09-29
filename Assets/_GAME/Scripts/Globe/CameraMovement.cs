using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.005f;
    [SerializeField] private float smoothing = 0.1f;
    [SerializeField] private float minPitch = -Mathf.PI / 2f + 0.01f;
    [SerializeField] private float maxPitch = Mathf.PI / 2f - 0.01f;

    private RawImage MapImage => MapInteractionRaycaster.Instance.MapImage;
    private float distance;
    private Vector2 mouse;
    private Vector2 mouseOnDown;
    private Vector2 rotation;
    private Vector2 target;
    private Vector2 targetOnDown;
    private bool dragging;

    private void Start()
    {
        SyncFromTransform();
    }

    private void Update()
    {
        if (FlyToLocation.Instance != null && FlyToLocation.Instance.IsFlying)
        {
            dragging = false;
            return;
        }

        Vector3 center = GetCenter();
        if (!dragging && NeedsSync(center))
            SyncFromTransform();

        if (Input.GetMouseButtonDown(0) && IsPointerOverMap(Input.mousePosition))
        {
            dragging = true;
            mouseOnDown = new Vector2(Input.mousePosition.x, -Input.mousePosition.y);
            targetOnDown = target;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            mouse = new Vector2(Input.mousePosition.x, -Input.mousePosition.y);
            target.x = targetOnDown.x + (mouse.x - mouseOnDown.x) * rotationSpeed;
            target.y = targetOnDown.y + (mouse.y - mouseOnDown.y) * rotationSpeed;
            target.y = Mathf.Clamp(target.y, minPitch, maxPitch);
        }

        rotation.x += (target.x - rotation.x) * smoothing;
        rotation.y += (target.y - rotation.y) * smoothing;

        Vector3 position = CalculatePosition(rotation, distance);
        transform.position = center + position;
        transform.LookAt(center);
    }

    private Vector3 GetCenter()
    {
        return transform.parent != null ? transform.parent.position : Vector3.zero;
    }

    private Vector3 CalculatePosition(Vector2 rotationAngles, float radius)
    {
        return new Vector3(
            radius * Mathf.Sin(rotationAngles.x) * Mathf.Cos(rotationAngles.y),
            radius * Mathf.Sin(rotationAngles.y),
            radius * Mathf.Cos(rotationAngles.x) * Mathf.Cos(rotationAngles.y));
    }

    private bool NeedsSync(Vector3 center)
    {
        Vector3 expectedPosition = center + CalculatePosition(rotation, distance);
        return (transform.position - expectedPosition).sqrMagnitude > 0.0001f;
    }

    private void SyncFromTransform()
    {
        Vector3 center = GetCenter();
        Vector3 offset = transform.position - center;
        distance = offset.magnitude;
        if (distance < 0.001f)
            distance = 1f;

        rotation.x = Mathf.Atan2(offset.x, offset.z);
        rotation.y = Mathf.Asin(offset.y / distance);
        target = rotation;
    }

    private bool IsPointerOverMap(Vector2 screenPosition)
    {
        if (MapImage == null)
            return false;

        RectTransform rectTransform = MapImage.rectTransform;
        if (rectTransform == null)
            return false;

        Camera uiCamera = MapImage.canvas != null ? MapImage.canvas.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, uiCamera, out Vector2 localPoint))
            return false;

        Rect rect = rectTransform.rect;
        return localPoint.x >= rect.xMin && localPoint.x <= rect.xMax && localPoint.y >= rect.yMin && localPoint.y <= rect.yMax;
    }
}
