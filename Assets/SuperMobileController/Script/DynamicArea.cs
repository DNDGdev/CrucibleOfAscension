using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperMobileController
{
    public class DynamicArea : MonoBehaviour
    {
        public bool hideElementOnRelease = false;

        private ControlBase dynamicControl;
        private Vector2 originalPosition;
        // Start is called before the first frame update
        void Start()
        {
            EventTrigger trigger = GetComponent<EventTrigger>();

            dynamicControl = transform.GetComponentInChildren<ControlBase>(true);
            originalPosition = dynamicControl.transform.position;

            if (hideElementOnRelease)
            {
                dynamicControl.gameObject.SetActive(false);
            }

            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
            trigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
            trigger.triggers.Add(pointerUpEntry);

            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
            trigger.triggers.Add(dragEntry);

            EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
            endDragEntry.eventID = EventTriggerType.EndDrag;
            endDragEntry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
            trigger.triggers.Add(endDragEntry);

            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
            trigger.triggers.Add(beginDragEntry);
        }

        //Pointer down event callback, fire when touch down
        private void OnPointerDown(PointerEventData eventArgs)
        {
            dynamicControl.transform.position = eventArgs.position;
            dynamicControl.gameObject.SetActive(true);
            dynamicControl.OnPointerDown(eventArgs);
        }

        //Pointer down up callback, fire when touch up
        private void OnPointerUp(PointerEventData eventArgs)
        {
            dynamicControl.transform.position = originalPosition;
            dynamicControl.OnPointerUp(eventArgs);
            if (hideElementOnRelease)
            {
                dynamicControl.gameObject.SetActive(false);
            }
        }

        //Pointer begin drag callback, fire when start to drag
        private void OnBeginDrag(PointerEventData eventArgs)
        {
            dynamicControl.OnBeginDrag(eventArgs);
        }

        //Pointer on drag callback, fire when dragging
        private void OnDrag(PointerEventData eventArgs)
        {
            dynamicControl.OnDrag(eventArgs);
        }

        private void OnEndDrag(PointerEventData eventArgs)
        {
            dynamicControl.OnEndDrag(eventArgs);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}