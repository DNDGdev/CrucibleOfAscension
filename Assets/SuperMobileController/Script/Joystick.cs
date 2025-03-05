using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuperMobileController
{
    public class Joystick : ControlBase
    {
        [Header("General Settings")]
        public string inputName;    // Skill name which pass to skill handler
        public float draggableRadiusModifier = 0;   //The default draggable area of joystick / Skill button is the image width of the outer circle.  This is used to adjust the default area 
        public Transform relativeTransform; //Set only if you want the result relative to the target object (e.g.The camera)

        [Header("Events Settings")]
        public OnDragBeginEvent beginDragEvent;  //CallBack when Drag Begin - Function(string inputName)
        public OnResultEvent dragEvent;  //Callback for end drag / end touch - Function(string SkillName, Vector2 TargetVector)
        public Sprite analogImage;
        public Sprite analogAreaImage;

        //private variables
        private Image joystickImage; //Image of the Joystick
        private RectTransform rect;
        private Vector2 parentPosition;
        private Vector3 relativeForward;
        private float maxDisplacement = 0;
        private bool IsEndDrag = false;
        private Vector2 draggingTarget;

        // Start is called before the first frame update
        void Start()
        {
            joystickImage = transform.Find("Joystick").GetComponent<Image>();
            if (analogImage != null && joystickImage != null)
            {
                joystickImage.sprite = analogImage;
            }
            if (analogAreaImage != null && GetComponent<Image>() != null)
            {
                GetComponent<Image>().sprite = analogAreaImage;
            }

            //Setup the event handler
            EventTrigger trigger = GetComponent<EventTrigger>();

            //set data
            rect = gameObject.GetComponent<RectTransform>();

            //If we want the direction relative to target (character) transform, be add begin drag event listener
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

            maxDisplacement = (rect.rect.width / 2) + draggableRadiusModifier;
        }
        //Pointer down event callback, fire when touch down
        public override void OnPointerDown(PointerEventData eventArgs)
        {
            //set postion of the dragging control
            parentPosition = new Vector2(transform.position.x, transform.position.y);

            //Call begin drag event
            if (beginDragEvent != null)
            {
                beginDragEvent.Invoke(inputName);
            }

            //If we want the direction relative to target (character) transform, we store the initial forward direction as reference
            if (relativeTransform != null)
            {
                relativeForward = relativeTransform.forward;
            }
            OnDrag(eventArgs);
        }

        //Pointer down up callback, fire when touch up
        public override void OnPointerUp(PointerEventData eventArgs)
        {
            //Handle a single tap (not dragged)
            OnEndDrag(eventArgs);
        }

        //Pointer on drag callback, fire when dragging
        public override void OnDrag(PointerEventData eventArgs)
        {
            //Calculate the position
            Vector2 relativePosition = eventArgs.position - parentPosition;
            Vector2 targetMarkerImagePosition;

            //if the displacement is larger then the radius, we set the magnitude of the relative position to max displacement
            if (relativePosition.magnitude > maxDisplacement)
            {
                relativePosition = relativePosition.normalized * maxDisplacement;
            }

            targetMarkerImagePosition = relativePosition;
            relativePosition = CommonFunctions.GetRelativeTransformedPosition(relativePosition, relativeForward);
            draggingTarget = GetFinalResult(relativePosition);

            if (joystickImage != null)
            {
                //set joystick position
                joystickImage.transform.localPosition = targetMarkerImagePosition;
            }
            //Result callback
            ReturnResult(relativePosition);
        }

        public override void OnEndDrag(PointerEventData eventArgs)
        {
            if (joystickImage != null)
            {
                //Reset the joystick to the parent position
                joystickImage.transform.position = transform.position;
            }
            //Call event with zero vector
            ReturnResult(Vector2.zero);
        }

        //Result callback
        private void ReturnResult(Vector2 relativePosition)
        {
            if (dragEvent != null)
            {
                //callback function of the control
                dragEvent.Invoke(inputName, GetFinalResult(relativePosition));
            }

            //stop the dragging callback
            IsEndDrag = true;
        }

        private Vector2 GetFinalResult(Vector2 relativePosition)
        {
            //We need to untransform the vector to the relative transform so we get the point relative to the player

            //set magnitude to 0-1
            Vector2 result = relativePosition;

            result = relativePosition / maxDisplacement;
            return result;
        }

    }
}