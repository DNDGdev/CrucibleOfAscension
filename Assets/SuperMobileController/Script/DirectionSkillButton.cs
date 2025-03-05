using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace SuperMobileController
{
    public class DirectionSkillButton : ControlBase
    {
        [Header("General Settings")]
        public string inputName;    // Skill name which pass to skill handler

        [Header("General Settings")]
        public float draggableRadiusModifier = 0;   //The default draggable area of joystick / Skill button is the image width of the outer circle.  This is used to adjust the default area 
        public Transform relativeTransform; //Set only if you want the result relative to the target object (e.g.The camera)

        [Header("Result Callback Settings")]
        public OnResultEvent inputEvent;  //Callback for end drag / end touch - Function(string SkillName, Vector2 TargetVector)
        public OnDragBeginEvent beginDragEvent;  //CallBack when Drag Begin - Function(string SkillName)
        public float draggingEventCallInterval = 1; //The interval for calling drag event
        public OnResultEvent draggingEvent; //Callback when dragging - unction(string SkillName, Vector2 TargetVector)

        [Header("Skill Button Settings")]
        public float cooldownSecond = 2f;   //Cooldown of the button
        public bool canCancel = true; //Is cancel available for this button
        public int quantityLeft = -1; //Quantity left for this button to be used (-1 = unlimited)
        public GameObject player;   //The object which this controller is controlling 
        public Texture skillTargetMarkerTexture;
        public float skillMarkerAspectRatio = 5;
        public float skillMarkerSize = 0.5f;
        public Color skillMarkerColour = Color.green;
        public LayerMask markerIgnoreLayers;
        public Sprite buttonImage;
        public Sprite analogImage;
        public Sprite analogAreaImage;

        //private variables
        private Image skillAreaImage;    //The ground area image when dragging
        private Image analogTargetImage; //The image for drag target
        private GameObject skillAreaMarker;
        private GameObject skillTargetMarker;
        private GameObject[] allCancelAreas;
        private float currentCoolDown = 0;
        private Image cooldownImage; //The image for reloading 
        private Image disableImage; //Image to be enabled when button is disabled 
        private Text quantityText;
        private Text cooldownText;
        private Vector2 currentTarget;
        private bool isEnabled = true; //Is the button Enabled
        private Vector2 cachedDraggingInput = Vector2.zero;
        [SerializeField]private GameObject targetMarkerProjector; //The projector for drag target
        public DecalProjector decalProjector;
        private Image buttonInnerImage;
        private RectTransform rect;
        private Vector2 parentPosition;
        private Vector3 relativeForward;
        private float maxDisplacement = 0;
        private bool IsEndDrag = false;
        private Vector2 draggingTarget;

        // Start is called before the first frame update
        void Start()
        {
            //Setup the event handler
            EventTrigger trigger = GetComponent<EventTrigger>();

            //set data (we cache the data at start as GetComponent is slow)
            rect = gameObject.GetComponent<RectTransform>();

            skillAreaImage = transform.Find("AreaImage").GetComponent<Image>();
            analogTargetImage = transform.Find("TargetMarker").GetComponent<Image>();
            cooldownImage = transform.Find("Cooldown").GetComponent<Image>();
            disableImage = transform.Find("Disable").GetComponent<Image>();
            quantityText = transform.Find("QuantityText").GetComponent<Text>();
            cooldownText = cooldownImage.transform.Find("Text").GetComponent<Text>();
            if (skillTargetMarkerTexture != null && skillMarkerSize > 0)
            {
                if(targetMarkerProjector == null)
                {
                    //var skillDirectionProjectorObj = new GameObject("SkillDirectionMarker", new Type[] { typeof(Projector) });
                    //targetMarkerProjector = skillDirectionProjectorObj.GetComponent<Projector>();
                    //targetMarkerProjector.nearClipPlane = 0.01f;
                    //targetMarkerProjector.farClipPlane = 100;
                    //targetMarkerProjector.fieldOfView = 50;
                    //targetMarkerProjector.aspectRatio = 1;
                    //targetMarkerProjector.orthographic = true;
                    //targetMarkerProjector.orthographicSize = skillMarkerSize;
                    //targetMarkerProjector.ignoreLayers = markerIgnoreLayers;
                    //targetMarkerProjector.aspectRatio = skillMarkerAspectRatio;
                    //targetMarkerProjector.material = new Material(Shader.Find("Projector/NoTransparentSolidProjector"));
                    //targetMarkerProjector.material.SetTexture("_ShadowTex", skillTargetMarkerTexture);
                    //targetMarkerProjector.material.SetColor("_Color", skillMarkerColour);
                  
                }
                decalProjector = targetMarkerProjector.GetComponent<DecalProjector>();
                targetMarkerProjector.SetActive(false);
                targetMarkerProjector.transform.SetParent(transform);
                decalProjector.transform.position = Vector3.zero;
                targetMarkerProjector.transform.position = Vector3.zero;
            }
            else
            {
                targetMarkerProjector = null;
            }
            if (buttonImage != null)
            {
                buttonInnerImage = transform.Find("ButtonInner/Image").GetComponent<Image>();
                buttonInnerImage.sprite = buttonImage;
            }
            if (analogImage != null && analogTargetImage != null)
            {
                analogTargetImage.sprite = analogImage;
            }
            if (analogAreaImage != null && skillAreaImage != null)
            {
                skillAreaImage.sprite = analogAreaImage;
            }

            allCancelAreas = GameObject.FindObjectsOfType<CancelArea>().Select(c => c.gameObject).ToArray();
            SetCancelAreasEnabled(false);

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

            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
            trigger.triggers.Add(beginDragEntry);


            //hide the area when not dragging, set max dragging displacement
            if (skillAreaImage != null)
            {
                skillAreaImage.enabled = false;
            }
            if (analogTargetImage != null)
            {
                analogTargetImage.enabled = false;
            }
            if (targetMarkerProjector != null && player != null)
            {
                skillTargetMarker = GameObject.Instantiate(targetMarkerProjector.gameObject, player.transform);
                skillTargetMarker.gameObject.SetActive(false);
            }
            maxDisplacement = (skillAreaImage.rectTransform.rect.width / 2) + draggableRadiusModifier;

            //Set quantity text
            if (quantityLeft > -1 && quantityText != null)
            {
                quantityText.text = quantityLeft.ToString();
            }
        }

        private void Update()
        {
            if (currentCoolDown > 0)
            {
                currentCoolDown -= Time.deltaTime;
                cooldownImage.fillAmount = currentCoolDown / cooldownSecond;

                if (cooldownText != null)
                {
                    if (currentCoolDown > 0)
                    {
                        cooldownText.text = currentCoolDown.ToString("0.0");
                    }
                    else
                    {
                        cooldownText.text = "";
                    }
                }
            }
        }

        //Pointer down event callback, fire when touch down
        public override void OnPointerDown(PointerEventData eventArgs)
        {
            //Exit function when it's not a valid action
            if (!isEnabled || currentCoolDown > 0)
            {
                return;
            }

            //make the dragging button on top of others
            if (skillAreaImage != null)
            {
                skillAreaImage.enabled = true;
                transform.SetAsLastSibling();
            }

            //set postion of the dragging control
            parentPosition = new Vector2(transform.position.x, transform.position.y);

            //Call begin drag event
            if (beginDragEvent != null)
            {
                beginDragEvent.Invoke(inputName);
            }

            //Cancel area set to visible 
            if (canCancel)
                SetCancelAreasEnabled(true);

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
            //Exit function when it's not a valid action
            if (!isEnabled || currentCoolDown > 0)
            {
                return;
            }

            if (canCancel)
                SetCancelAreasEnabled(false);

            //Handle a single tap (not dragged)
            OnEndDrag(eventArgs);
        }

        //Pointer begin drag callback, fire when start to drag
        public override void OnBeginDrag(PointerEventData eventArgs)
        {
            //Exit function when it's not a valid action
            if (!isEnabled || currentCoolDown > 0)
            {
                return;
            }

            //cancel area should always on top
            foreach (var ca in allCancelAreas)
            {
                ca.transform.SetAsLastSibling();
            }

            //Start calling Dragging callback conotinuously
            if (draggingEvent != null && draggingEventCallInterval > 0)
            {
                IsEndDrag = false;
                StartCoroutine(DraggingCallBack(draggingEventCallInterval));
            }
        }

        //Pointer on drag callback, fire when dragging
        public override void OnDrag(PointerEventData eventArgs)
        {
            //Exit function when it's not a valid action
            if (!isEnabled || currentCoolDown > 0)
            {
                return;
            }

            //Calculate the position
            Vector2 relativePosition = eventArgs.position - parentPosition;
            Vector2 analogTargetImagePosition;


            //if the displacement is larger then the radius, we set the magnitude of the relative position to max displacement
            if (relativePosition.magnitude > maxDisplacement)
            {
                relativePosition = relativePosition.normalized * maxDisplacement;
            }

            analogTargetImagePosition = relativePosition;
            relativePosition = CommonFunctions.GetRelativeTransformedPosition(relativePosition, relativeForward);
            currentTarget = relativePosition.normalized;
            cachedDraggingInput = relativePosition;

            if (analogTargetImage != null)
            {
                //show the marker when dragging
                analogTargetImage.transform.localPosition = analogTargetImagePosition;
                analogTargetImage.enabled = true;
            }
            if (skillAreaMarker != null && player != null)
            {
                //Show skill area when dragging
                skillAreaMarker.transform.localPosition = new Vector3(0, 0, 0);
                skillAreaMarker.gameObject.SetActive(true);
            }
            if (skillTargetMarker != null && player != null)
            {
                skillTargetMarker.gameObject.SetActive(true);
            }
        }

        public override void OnEndDrag(PointerEventData eventArgs)
        {
            //Exit function when it's not a valid action
            if (!isEnabled || currentCoolDown > 0)
            {
                return;
            }


            if (skillAreaImage != null)
            {
                //hide the area when end dragging
                skillAreaImage.enabled = false;
            }
            if (analogTargetImage != null)
            {
                //hide the marker when end dragging
                analogTargetImage.enabled = false;
            }
            if (skillAreaMarker != null)
            {
                //hide the area when end dragging
                skillAreaMarker.SetActive(false);
            }
            if (skillTargetMarker != null)
            {
                //hide the marker when end dragging
                skillTargetMarker.SetActive(false);
            }

            Vector2 relativePosition = eventArgs.position - parentPosition;

            //if the displacement is larger then the radius, we set the magnitude of the relative position to max displacement
            if (relativePosition.magnitude > maxDisplacement)
            {
                relativePosition = relativePosition.normalized * maxDisplacement;
            }

            relativePosition = CommonFunctions.GetRelativeTransformedPosition(relativePosition, relativeForward);

            //If End on Cancel Area, We do not send the result
            if (eventArgs.pointerEnter != null)
            {
                CancelArea cancelButton = eventArgs.pointerEnter.GetComponent<CancelArea>();
                if (cancelButton != null)
                {
                    return;
                }
            }

            //handle cooldown
            if (cooldownSecond > 0 && cooldownImage != null)
            {
                currentCoolDown = cooldownSecond;
            }
            //result callback
            ReturnResult(relativePosition);
        }

        //Result callback
        private void ReturnResult(Vector2 relativePosition)
        {
            if (inputEvent != null)
            {
                //callback function of the control
                inputEvent.Invoke(inputName, relativePosition.normalized);
            }

            //handle quantity
            if (quantityLeft > 0)
            {
                quantityLeft -= 1;
                quantityText.text = quantityLeft.ToString();
                if (quantityLeft == 0)
                {
                    SetEnabled(false);
                }
            }
            //stop the dragging callback
            IsEndDrag = true;
            cachedDraggingInput = Vector2.zero;
        }
        //Keep calling the dragging callback until drag ended
        private IEnumerator DraggingCallBack(float interval)
        {
            if (!IsEndDrag)
            {
                draggingEvent.Invoke(inputName, currentTarget);
                yield return new WaitForSeconds(interval);
                StartCoroutine(DraggingCallBack(interval));
            }
            else
            {
                IsEndDrag = false;
            }
        }
        //Enable or disable all cancel areas
        private void SetCancelAreasEnabled(bool isEnabled)
        {
            foreach (var ca in allCancelAreas)
            {
                ca.GetComponent<Image>().enabled = isEnabled;
            }
        }

        private void LateUpdate()
        {
            if (player != null && skillTargetMarker != null && cachedDraggingInput.magnitude > 0)
            {
                //Calculate the position and display target marker
                Vector2 relativeMarkerPosition = new Vector2((cachedDraggingInput.x / maxDisplacement), (cachedDraggingInput.y / maxDisplacement));
                var direction = new Vector3(cachedDraggingInput.x, 0, cachedDraggingInput.y);
                skillTargetMarker.transform.rotation = Quaternion.LookRotation(direction, Vector3.forward) * Quaternion.Euler(0, 90, 0);
                if (skillTargetMarker.transform.rotation.eulerAngles.x == 0)
                {
                    skillTargetMarker.transform.rotation *= Quaternion.Euler(90, 0, 0);
                }
                else if (cachedDraggingInput.x > 0)
                {
                    skillTargetMarker.transform.rotation *= Quaternion.Euler(180, 0, 0);
                }
                Vector3 directionNormalized = direction.normalized;
                Vector2 decalSize = decalProjector.size; // Get the Decal Projector size

                // Use the X component for width
                float decalWidth = decalSize.x * 0.5f; // Assuming size.x is the width (half-size for centering)

                skillTargetMarker.transform.position = player.transform.position + (directionNormalized * decalWidth) + new Vector3(0, 0, 0);



            }
        }

        //Set variables methods
        //method to enable / disable the control
        public void SetEnabled(bool isEnabled)
        {
            this.isEnabled = isEnabled;
            if (disableImage != null)
            {
                if (isEnabled)
                {
                    disableImage.enabled = false;
                }
                else
                {
                    disableImage.enabled = true;
                }
            }
        }

        public void SetQuantity(int quantity)
        {
            quantityLeft = quantity;
            quantityText.text = quantity.ToString();
            if (quantity != 0)
            {
                SetEnabled(true);
            }
            else
            {
                SetEnabled(false);
            }
        }

        public void SetCoolDown(float seconds)
        {
            cooldownSecond = seconds;
        }

        public void SetskillMarkerSize(float size)
        {
            skillMarkerSize = size;
            if (skillTargetMarkerTexture != null)
            {
                //targetMarkerProjector.orthographicSize = skillMarkerSize;
            }
        }

        public void SetButtonImage(Sprite image)
        {
            buttonImage = image;
            buttonInnerImage.sprite = image;
        }
    }
}