using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperMobileController
{
    public class TouchInput : MonoBehaviour
    {
        [Header("General Settings")]
        public string inputName;    // Skill name which pass to skill handler
        public Transform relativeTransform; //Set only if you want the result relative to the target object (e.g.The camera)

        [Header("Events Settings")]
        public OnDragBeginEvent beginDragEvent;  //CallBack when Drag Begin - Function(string inputName)
        public bool DraggingResultRelativeToInitialPoint; //Set to true for using it as a movement input
        public OnTouchResultEvent dragEvent;  //Callback on dragging - Function(string SkillName, Vector2 TargetVector, float DragSpeed)
        public OnTouchResultEvent endDragEvent;  //Callback on Drag Ended - Function(string SkillName, Vector2 TargetVector, float DragSpeed)
        public float HoldSecond = 0.5f; //The time in second which we concern it as a hold action
        public bool StopDragEndWhenHold = true; //Do not fire the drag end / touch end event when it is a hold action
        public bool StopDragWhenHold = true; //Do not fire the drag event when it is a hold action
        public OnResultEvent HoldBeginEvent; //Callback on hold - function(string SkillName, Vector2 HoldPoint)
        public OnResultEvent HoldEndEvent; //Callback on hold - function(string SkillName, Vector2 HoldPoint)
        public float dragThreshold = 1;  //The threshold the determine if it is dragging 
        public bool enableDragSelectionBox = false;
        public Color dragSelectionBoxColour = Color.green;

        //private variables
        private RectTransform rect;
        private Vector2 parentPosition;
        private Vector3 relativeForward;
        private float maxDisplacement = 0;
        private Vector2 draggingTarget;
        private Vector2 beginDragPosition;
        private DateTime beginDragTime;
        private bool isHolding = false;
        private bool isDragged = false;
        private Coroutine hold;
        private Rect? drawSelectionBox;

        // Start is called before the first frame update
        void Start()
        {
            //Setup the event handler
            EventTrigger trigger = GetComponent<EventTrigger>();


            //set data
            rect = gameObject.GetComponent<RectTransform>();

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

            maxDisplacement = (rect.rect.width / 2);
        }

        public void Update()
        {
            if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) || Input.GetMouseButtonUp(0)) && !isDragged)
            {
                Vector2 point = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(point);
                RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);

                var result = hits.Select(c => new
                {
                    touchObject = c.collider == null ? (TouchObject)null : c.collider.gameObject.GetComponent<TouchObject>(),
                    point = c.point
                });

                var objs = result.Where(c => c.touchObject != null).OrderBy(c => c.touchObject.tapPriority);

                foreach (var obj in objs)
                {
                    obj.touchObject.TapSelect(obj.point);
                    if (obj.touchObject.blockLowerPrioritytap)
                    {
                        break;
                    }
                }
            }
        }

        void OnGUI()
        {
            if (enableDragSelectionBox && drawSelectionBox != null)
            {                
                CommonFunctions.DrawRect(drawSelectionBox.Value, new Color(dragSelectionBoxColour.r, dragSelectionBoxColour.g, dragSelectionBoxColour.b, 0.25f));
                CommonFunctions.DrawBorder(drawSelectionBox.Value, 2, new Color(dragSelectionBoxColour.r, dragSelectionBoxColour.g, dragSelectionBoxColour.b));
            }
        }

        //Pointer down event callback, fire when touch down
        private void OnPointerDown(PointerEventData eventArgs)
        {
            //Reset variables
            isDragged = false;
            isHolding = false;

            //Call delayed hold method
            hold = StartCoroutine(SetHold(eventArgs.position));

            //set postion of the initial drag
            parentPosition = eventArgs.position;
            beginDragPosition = eventArgs.position;

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
            
            beginDragTime = DateTime.Now;
            OnDrag(eventArgs);
        }

        //Pointer up event callback, fire when touch up
        private void OnPointerUp(PointerEventData eventArgs)
        {
            if (!isDragged)
            {
                OnEndDrag(eventArgs);
            }
        }
            
        //Pointer on drag callback, fire when dragging
        private void OnDrag(PointerEventData eventArgs)
        {
            Vector2 resultPoint = GetResultPosition(eventArgs);
            //We don't want it too be too sensitive
            if ((eventArgs.position - beginDragPosition).magnitude > dragThreshold)
            {
                if (enableDragSelectionBox && isDragged)
                {
                    drawSelectionBox = CommonFunctions.GetRect(beginDragPosition, eventArgs.position);
                    var viewportBounds = CommonFunctions.GetViewportBounds(beginDragPosition, eventArgs.position);
                    GameObject.FindObjectsOfType<TouchObject>().ToList().ForEach(c =>
                    {
                        if (viewportBounds.Contains(Camera.main.WorldToViewportPoint(c.transform.position)))
                        {
                            c.GetComponent<TouchObject>().AreaSelecting();
                        }
                        else
                        {
                            c.GetComponent<TouchObject>().UndoAreaSelecting();
                        }
                    });
                }

                isDragged = true;


                if (!StopDragWhenHold || !isHolding)
                {
                    DateTime now = DateTime.Now;
                    float speed = (resultPoint - beginDragPosition).magnitude / (float)(now - beginDragTime).TotalMilliseconds;
                    //Result callback
                    ReturnResult(resultPoint, dragEvent, speed);
                }
            }
        }

        //Pointer on end drag callback, fire when drag eneded
        private void OnEndDrag(PointerEventData eventArgs)
        {
            if (isDragged)
            {
                TouchObject.selectedObjects.Where(c => c.touchSelectStatus != TouchSelectStatus.Selecting).ToList().ForEach(c =>
                {
                    if (c.deselectWhenAreaSelect)
                    {
                        c.Deselect();
                    }
                });
            } else
            {
                TouchObject.selectedObjects.Where(c => c.touchSelectStatus != TouchSelectStatus.Selecting).ToList().ForEach(c =>
                {
                    if (c.deselectWhenTapSelect)
                    {
                        c.Deselect();
                    }
                });
            }

            var objectsToConfirmSelection = GameObject.FindObjectsOfType<TouchObject>().Where(c => c.touchSelectStatus == TouchSelectStatus.Selecting || c.touchSelectStatus == TouchSelectStatus.Deselecting).ToList();
            for (int i = 0; i < objectsToConfirmSelection.Count; i++)
            {
                if (objectsToConfirmSelection[i].touchSelectStatus == TouchSelectStatus.Selecting)
                {
                    objectsToConfirmSelection[i].touchSelectStatus = TouchSelectStatus.Selected;
                    if (objectsToConfirmSelection[i].areaSelectEvent != null)
                    {
                        objectsToConfirmSelection[i].AreaSelect();
                    }
                    if (!TouchObject.selectedObjects.Contains(objectsToConfirmSelection[i]))
                    {
                        TouchObject.selectedObjects.Add(objectsToConfirmSelection[i]);
                    }
                }
                else if (objectsToConfirmSelection[i].touchSelectStatus == TouchSelectStatus.Deselecting)
                {
                    objectsToConfirmSelection[i].Deselect();
                }
            }
            drawSelectionBox = null;

            //Avoid hold function begin called after we done the action
            if (!StopDragEndWhenHold || !isHolding)
            {
                Vector2 resultPoint = GetResultPosition(eventArgs);
                DateTime now = DateTime.Now;
                float speed = (resultPoint - beginDragPosition).magnitude / (float)(now - beginDragTime).TotalMilliseconds;
                //Result callback
                ReturnResult(resultPoint, endDragEvent, speed);
            } 

            if (isHolding && HoldEndEvent != null)
            {
                HoldEndEvent.Invoke(inputName, eventArgs.position);
            }
            
            //Cancel the hold method call
            StopCoroutine(hold);
        }

        //Result callback
        private void ReturnResult(Vector2 relativePosition, OnTouchResultEvent resultEvent, float speed)
        {
            if (resultEvent != null)
            {
                //callback function of the control
                resultEvent.Invoke(inputName, GetFinalResult(relativePosition), speed);
            }
        }

        private Vector2 GetResultPosition(PointerEventData eventArgs)
        {
            //Calculate the position
            Vector2 relativePosition = eventArgs.position - parentPosition;

            if (!DraggingResultRelativeToInitialPoint)
                parentPosition = eventArgs.position;

            //if the displacement is larger then the radius, we set the magnitude of the relative position to max displacement
            if (relativePosition.magnitude > maxDisplacement)
            {
                relativePosition = relativePosition.normalized * maxDisplacement;
            }

            if (relativeTransform != null)
            {
                return CommonFunctions.GetRelativeTransformedPosition(relativePosition, relativeForward);
            } else
            {
                return relativePosition;
            }
        }

        private Vector2 GetFinalResult(Vector2 relativePosition)
        {
            //We need to untransform the vector to the relative transform so we get the point relative to the player

            //set magnitude to 0-1
            Vector2 result = relativePosition;
            result = relativePosition / 2;
            return result;
        }

        //Call When it's a hold action
        private IEnumerator SetHold(Vector2 holdPoint)
        {
            yield return new WaitForSeconds(HoldSecond);
            
            if (!isDragged)
            {
                isHolding = true;
                HoldBeginEvent.Invoke(inputName, holdPoint);
            }
        }
    }
}