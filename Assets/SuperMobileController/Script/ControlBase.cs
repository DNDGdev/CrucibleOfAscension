using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuperMobileController
{
    public abstract class ControlBase : MonoBehaviour
    {
        public virtual void OnPointerDown(PointerEventData eventArgs) { }
        public virtual void OnPointerUp(PointerEventData eventArgs) { }
        public virtual void OnBeginDrag(PointerEventData eventArgs) { }
        public virtual void OnDrag(PointerEventData eventArgs) { }
        public virtual void OnEndDrag(PointerEventData eventArgs) { }
    }
}