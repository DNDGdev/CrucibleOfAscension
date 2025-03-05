using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuperMobileController
{
    public class TouchObject : MonoBehaviour
    {
        [Header("Events Settings")]
        public OnTouchSelectEvent tapSelectEvent;  //CallBack when tap on an unselected object - Function(Vector2 InteractPoint)
        public OnTouchSelectEvent tapReselectEvent;  //CallBack when tap on a selected object - Function(Vector2 InteractPoint)
        public OnTouchAreaSelectEvent areaSelectEvent;  //Callback when select by area - Function()
        public OnDeselectEvent deselectEvent;  //Callback when deselect - Function()

        [Header("General Settings")]
        public string selectionType;
        public bool areaSelectable = true;
        public bool tapSelectable = true;
        public bool deselectWhenTapSelect = false;
        public bool deselectWhenAreaSelect = true;
        public bool deselectWhenReselect = false;
        public bool uniqueSelectByTypeWhenTap = true;
        public TouchSelectStatus touchSelectStatus = TouchSelectStatus.Unselected;
        public Texture selectedMarkerTex;
        public float selectedMarkerSize = 1;   //The size of the selected marker (3 means 3 times image width)
        public LayerMask MarkerIgnoreLayers;
        public Color selectedMarkerColour = Color.green;
        public bool blockLowerPrioritytap = true;
        public int tapPriority = 0;

        public static List<TouchObject> selectedObjects = new List<TouchObject>();
        private GameObject selectedProjector;    //The projector object
        private TouchSelectStatus OriStatus = TouchSelectStatus.Unselected;

        private void Start()
        {
            if (selectedMarkerTex != null)
            {
                selectedProjector = new GameObject("UnitProjector", new Type[] { typeof(Projector) });
                Projector projectObj = selectedProjector.GetComponent<Projector>();
                projectObj.nearClipPlane = 0.01f;
                projectObj.farClipPlane = 1000;
                projectObj.fieldOfView = 50;
                projectObj.aspectRatio = 1;
                projectObj.orthographic = true;
                projectObj.orthographicSize = selectedMarkerSize;
                projectObj.ignoreLayers = MarkerIgnoreLayers;
                projectObj.material = new Material(Shader.Find("Projector/NoTransparentSolidProjector"));
                projectObj.material.SetTexture("_ShadowTex", selectedMarkerTex);
                projectObj.material.SetColor("_Color", selectedMarkerColour);
                selectedProjector.SetActive(false);
                selectedProjector.transform.SetParent(transform);
                selectedProjector.transform.rotation = Quaternion.Euler(90, 0, 0);
                selectedProjector.transform.localPosition = new Vector3(0, 100, 0);
            }
        }
        public void TapSelect(Vector3 InteractPoint)
        {
            if (tapSelectable && touchSelectStatus != TouchSelectStatus.UnSelectable)
            {
                if (uniqueSelectByTypeWhenTap)
                {
                    selectedObjects.Where(c => c.selectionType == selectionType && c != this).ToList().ForEach(c => c.Deselect());
                }
                if (touchSelectStatus == TouchSelectStatus.Selected)
                {
                    if (deselectWhenReselect)
                    {
                        Deselect();
                    } else
                    {
                        if (tapReselectEvent != null)
                            tapReselectEvent.Invoke(this, InteractPoint);
                    }
                } else
                {
                    touchSelectStatus = TouchSelectStatus.Selecting;
                    if (selectedProjector) 
                        selectedProjector.SetActive(true);
                    if (tapSelectEvent != null)
                        tapSelectEvent.Invoke(this, InteractPoint);
                    if (!selectedObjects.Contains(this))
                    {
                        selectedObjects.Add(this);
                    }
                }
            }
        }
        public void AreaSelecting()
        {
            if (areaSelectable && touchSelectStatus != TouchSelectStatus.UnSelectable)
            {
                if (touchSelectStatus == TouchSelectStatus.Selected)
                {
                    OriStatus = touchSelectStatus;
                    if (deselectWhenReselect)
                    {
                        touchSelectStatus = TouchSelectStatus.Deselecting;
                        if (selectedProjector)
                            selectedProjector.SetActive(false);
                    } else
                    {
                        touchSelectStatus = TouchSelectStatus.Selecting;
                    }
                }
                else if (touchSelectStatus == TouchSelectStatus.Unselected)
                {
                    OriStatus = touchSelectStatus;
                    touchSelectStatus = TouchSelectStatus.Selecting;
                    if (selectedProjector)
                        selectedProjector.SetActive(true);
                }
            }
        }

        public void UndoAreaSelecting()
        {
            if (areaSelectable)
            {
                touchSelectStatus = OriStatus;
                if (selectedProjector)
                    selectedProjector.SetActive(OriStatus == TouchSelectStatus.Selected);
            }
        }

        public void AreaSelect()
        {
            if (areaSelectable)
                areaSelectEvent.Invoke(this);
        }

        public void Deselect()
        {
            touchSelectStatus = TouchSelectStatus.Unselected;
            if (selectedProjector)
                selectedProjector.SetActive(false);
            if (selectedObjects.Contains(this))
            {
                selectedObjects.Remove(this);
            }
            deselectEvent.Invoke(this);
        }
    }
}
