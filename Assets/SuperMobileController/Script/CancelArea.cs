using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuperMobileController
{
    public class CancelArea : MonoBehaviour
    {
        public Sprite areaImage;
        public Sprite activeAreaImage;

        private Image cancelImage;
        private void Start()
        {
            cancelImage = GetComponent<Image>();
            if (areaImage != null)
            {
                cancelImage.sprite = areaImage;
            } else
            {
                areaImage = cancelImage.sprite;
            }
        }

        //Set variables methods
        public void SetAreaImage(Sprite image)
        {
            areaImage = image;
            cancelImage.sprite = image;
        }

        public void SetActiveAreaImage(Sprite image)
        {
            activeAreaImage = image;
        }

        public void Update()
        {
            //If we drag over the cancel area and it is active, we change the image to active image
            if (cancelImage.IsActive() && activeAreaImage != null)
            {
                var pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, raycastResults);
                var result = raycastResults.Where(c => c.gameObject == this.gameObject);
                if (result.Count() > 0)
                {
                    cancelImage.sprite = activeAreaImage;
                } else if (areaImage != null)
                {
                    cancelImage.sprite = areaImage;
                }
            }
        }
    }
}