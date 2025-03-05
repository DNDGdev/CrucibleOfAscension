using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuperMobileController
{
    public class SimpleTouchButton : ControlBase
    {
        [Header("General Settings")]
        public string inputName;    // Skill name which pass to skill handler

        [Header("Result Callback Settings")]
        public OnResultEvent resultEvent;  //Callback for end drag / end touch - Function(string SkillName, Vector2 TargetVector)
        public OnResultEvent beginTouchEvent;  //Callback on touch begin - Function(string SkillName, Vector2 TargetVector, float DragSpeed)

        [Header("Skill Button Settings")]
        public float cooldownSecond = 2f;   //Cooldown of the button
        public bool canCancel = true; //Is cancel available for this button
        public int quantityLeft = -1; //Quantity left for this button to be used (-1 = unlimited)
        public Sprite buttonImage;

        //private variables
        private GameObject[] allCancelAreas;
        private Image cooldownImage; //The image for reloading 
        private Image disableImage; //Image to be enabled when button is disabled 
        private Text quantityText;
        private Text cooldownText;
        private bool isEnabled = true; //Is the button Enabled
        private float currentCoolDown = 0;
        private Image buttonInnerImage;

        // Start is called before the first frame update
        void Start()
        {
            //Setup the event handler
            EventTrigger trigger = GetComponent<EventTrigger>();

            //set data (we cache the data at start as GetComponent is slow)
            cooldownImage = transform.Find("Cooldown").GetComponent<Image>();
            disableImage = transform.Find("Disable").GetComponent<Image>();
            quantityText = transform.Find("QuantityText").GetComponent<Text>();
            cooldownText = cooldownImage.transform.Find("Text").GetComponent<Text>();

            if (buttonImage != null)
            {
                buttonInnerImage = transform.Find("ButtonInner/Image").GetComponent<Image>();
                buttonInnerImage.sprite = buttonImage;
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

            //Set quantity text
            if (quantityLeft > -1 && quantityText != null)
            {
                quantityText.text = quantityLeft.ToString();
            }
        }

        private void Update()
        {
            //handle cooldown
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

            //Cancel area set to visible 
            if (canCancel)
                SetCancelAreasEnabled(true);

            if (beginTouchEvent != null)
            {
                beginTouchEvent.Invoke(inputName, new Vector2(transform.position.x, transform.position.y));
            }
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

            //If End on Cancel Area, We do not send the result
            if (eventArgs != null && eventArgs.pointerEnter != null)
            {
                CancelArea cancelButton = eventArgs.pointerEnter.GetComponent<CancelArea>();
                if (cancelButton != null)
                {
                    return;
                }
            }

            //Handle cooldown
            if (cooldownSecond > 0 && cooldownImage != null)
            {
                currentCoolDown = cooldownSecond;
            }

            ReturnResult(eventArgs == null ? Vector2.zero : eventArgs.position);
        }

        //Enable or disable all cancel areas
        private void SetCancelAreasEnabled(bool isEnabled)
        {
            foreach (var ca in allCancelAreas)
            {
                ca.GetComponent<Image>().enabled = isEnabled;
            }
        }

        //private functions

        //Result callback
        private void ReturnResult(Vector2 relativePosition)
        {
            if (resultEvent != null)
            {
                //callback function of the control
                resultEvent.Invoke(inputName, relativePosition);
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

        public void SetButtonImage(Sprite image)
        {
            buttonImage = image;
            buttonInnerImage.sprite = image;
        }
    }
}