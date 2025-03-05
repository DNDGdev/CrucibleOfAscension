using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 lastMousePosition;
    private Vector2 releaseVelocity;
    private bool isMoving = false;
    public float forceMultiplier = 10f; // Adjust to control force effect
    public float friction = 0.98f;      // Simulated friction for smooth stopping

    public Action onSwipe;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastMousePosition = eventData.position;
        isMoving = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastMousePosition;
        //rectTransform.anchoredPosition += delta;
        lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Calculate release velocity
        releaseVelocity = (eventData.position - lastMousePosition) * forceMultiplier;
        isMoving = true;
        onSwipe?.Invoke();
    }

    private void Update()
    {
        if (isMoving)
        {
            rectTransform.anchoredPosition += releaseVelocity * Time.deltaTime;
            releaseVelocity *= friction; // Simulate friction for gradual stopping

            // Stop when the velocity is very low
            if (releaseVelocity.magnitude < 0.1f)
                isMoving = false;
        }
    }
}
