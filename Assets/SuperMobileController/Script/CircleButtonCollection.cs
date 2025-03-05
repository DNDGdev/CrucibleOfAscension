using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

namespace SuperMobileController
{
    public class buttonCollection
    {
        public SimpleTouchButton button;
        public Vector3 position;
        public RectTransform rect;
    }

    public class CircleButtonCollection : MonoBehaviour
    {
        public float radius = 50;
        public float degreeInBetween = 30;
        public float degreeAbjust = 0;

        private List<buttonCollection> buttons;
        private Vector2 initialPostion;
        // Start is called before the first frame update
        void Start()
        {
            buttons = transform.GetComponentsInChildren<SimpleTouchButton>().Select((btn, i) => new buttonCollection
            {
                button = btn,
                position = getCirclePosition(i),
                rect = btn.gameObject.GetComponent<RectTransform>()
            }).ToList();
            Hide();
        }

        public void Show(string inputName, Vector2 position)
        {
            gameObject.SetActive(true);
            transform.position = position;
            initialPostion = position;

            if (buttons != null)
            {
                buttons.ForEach(c => {
                    c.button.transform.localPosition = Vector2.zero;
                });
                transform.localScale = Vector3.one;
            }
        }

        public void FireButtonAtPosition(string inputName, Vector2 position)
        {
            var pointerEventData = new PointerEventData(EventSystem.current) { position = position };
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            var result = raycastResults.Where(c => c.gameObject.name == "ButtonInner" && c.gameObject.GetComponentInParent<SimpleTouchButton>() != null)
                .Select(c => c.gameObject.GetComponentInParent<SimpleTouchButton>()).FirstOrDefault();
            if (result != null)
            {
                result.OnPointerUp(null);
            }
            Hide();
        }

        public void Hide()
        {
            //Hide the control by setting scale to 0 and move it outside the screen.  We can't make it inactive as it will stop the cooldown counter
            transform.position = new Vector2(10000,10000);
            transform.localScale = Vector3.zero;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            buttons.ForEach(c =>
            {
                if ((c.button.transform.localPosition - c.position).magnitude != 0)
                {
                    c.button.transform.localPosition = Vector3.Lerp(c.button.transform.localPosition, c.position, 0.6f);
                }
            });
        }

        private Vector3 getCirclePosition(int index) {
            float angle = (degreeInBetween * index) + degreeAbjust;
            Vector3 pos = new Vector3();
            pos.x = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.y = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            return pos;
        }
    }
}