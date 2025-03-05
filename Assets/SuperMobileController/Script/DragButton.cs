using SuperMobileController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragButton : MonoBehaviour
{
    [Header("General Settings")]
    public string dropGroup;

    [Header("Events Settings")]
    public OnDragBeginEvent beginDragEvent;  //CallBack when Drag Begin - Function(string inputName)
    public OnDragButtonEvent dragEvent;  //Callback on dragging - Function(string SkillName, Vector2 TargetVector, float DragSpeed)
    public OnDragButtonEvent endDragEvent;  //Callback on Drag Ended - Function(string SkillName, Vector2 TargetVector, float DragSpeed)

    [Header("Drag Button Settings")]
    public Sprite buttonImage;
    public bool showDraggingObject = true;
    public bool showDraggingObjWhenUndropable = true;
    public GameObject draggingObject;
    public float dragYOffSet = 0;
    public Vector3 draggingObjectRotation;
    public bool createDropObject = true;
    public float dropYOffSet = 0;
    public Vector3 dropObjectRotation;
    public GameObject dropObject;

    private GameObject draggingObjectInstance;
    private List<TouchInput> touchInputs;
    private List<Color> oriColor = new List<Color>();
    // Start is called before the first frame update
    void Start()
    {
        //Setup the event handler
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
        trigger.triggers.Add(beginDragEntry);

        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        trigger.triggers.Add(dragEntry);

        EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        endDragEntry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        trigger.triggers.Add(endDragEntry);

        //Get all touch inputs, we need to disable them when using a drag button
        touchInputs = GameObject.FindObjectsOfType<TouchInput>().ToList();
        
        if (buttonImage != null)
        {
            GetComponent<Image>().sprite = buttonImage;
        }
    }

    //Pointer on begin drag callback, fire when drag begin
    private void OnBeginDrag(PointerEventData eventArgs)
    {
        //disable touch input
        touchInputs.ForEach(c => { c.enabled = false; });

        //begin drag call back
        beginDragEvent.Invoke(dropGroup);

        //Set color of the materials of drop target
        GameObject.FindObjectsOfType<DropTarget>().ToList().ForEach(c =>
        {
            var renderer = c.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                oriColor.Clear();
                foreach (var mat in renderer.materials)
                {
                    oriColor.Add(mat.color);
                    if (c.allowedDropGroup.Contains(dropGroup) && (c.allowedDropTint.r != 0 || c.allowedDropTint.g != 0 || c.allowedDropTint.b != 0))
                    {
                        mat.color = c.allowedDropTint;
                    } else if (!c.allowedDropGroup.Contains(dropGroup) && (c.disallowedDropTint.r != 0 || c.disallowedDropTint.g != 0 || c.disallowedDropTint.b != 0))
                    {
                        mat.color = c.disallowedDropTint;
                    }
                }
            }
        });
    }
        
    //Pointer on drag callback, fire when dragging
    private void OnDrag(PointerEventData eventArgs)
    {
        //Get hits
        Ray ray = Camera.main.ScreenPointToRay(eventArgs.position);
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);

        //Select the drop target with lowest priority
        var result = hits.Select(c => new
        {
            dropTarget = c.collider == null ? (DropTarget)null : c.collider.gameObject.GetComponents<DropTarget>().OrderBy(d => d.dropPriority).FirstOrDefault(),
            point = c.point
        });

        var obj = result.OrderBy(c => c.dropTarget.dropPriority).FirstOrDefault(c => c.dropTarget != null && c.dropTarget.allowedDropGroup.Contains(dropGroup));

        if (obj != null) {
            //if drop at fixed position, we get the world space position from the local transform
            var dropPoint = obj.point;
            if (obj.dropTarget.dropAtFixedPoint)
            {
                dropPoint = obj.dropTarget.gameObject.transform.TransformPoint(obj.dropTarget.fixedPoint);
            }
            
            //create dragging object
            if (showDraggingObject)
            {
                if (draggingObjectInstance == null)
                {
                    draggingObjectInstance = GameObject.Instantiate(draggingObject, new Vector3(dropPoint.x, dropPoint.y + dragYOffSet, dropPoint.z), Quaternion.Euler(draggingObjectRotation));
                } else
                {
                    draggingObjectInstance.transform.position = new Vector3(dropPoint.x, dropPoint.y + dragYOffSet, dropPoint.z);
                }
            }

            //drag call back
            dragEvent.Invoke(dropGroup, dropPoint, draggingObjectInstance, obj.dropTarget.gameObject);
        } else
        {
            //if no hit, we find the nearest hit of any object to show the dragging object 
            if (showDraggingObject && showDraggingObjWhenUndropable)
            {
                if (hits.Count() > 0)
                {
                    var hit = hits.FirstOrDefault(c => c.collider.gameObject != draggingObjectInstance);
                    
                    if (draggingObjectInstance == null)
                    {
                        draggingObjectInstance = GameObject.Instantiate(draggingObject, new Vector3(hit.point.x, hit.point.y + dragYOffSet, hit.point.z), Quaternion.Euler(dropObjectRotation));
                    }
                    else
                    {
                        draggingObjectInstance.transform.position = new Vector3(hit.point.x, hit.point.y + dragYOffSet, hit.point.z);
                    }
                }
            }

            dragEvent.Invoke(dropGroup, null, draggingObjectInstance, null);
        }
    }

    //Pointer on end drag callback, fire when drag eneded
    private void OnEndDrag(PointerEventData eventArgs)
    {
        //desory the dragging object
        if (draggingObjectInstance != null)
        {
            GameObject.Destroy(draggingObjectInstance);
        }

        //reset the materials' color
        GameObject.FindObjectsOfType<DropTarget>().ToList().ForEach(c =>
        {
            var renderer = c.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                for (int i = 0; i < renderer.materials.Count(); i++)
                {
                    renderer.materials[i].color = oriColor[i];
                }
            }
        });

        //check if use fixed point drop
        Ray ray = Camera.main.ScreenPointToRay(eventArgs.position);
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);

        var result = hits.Select(c => new
        {
            dropTarget = c.collider == null ? (DropTarget)null : c.collider.gameObject.GetComponents<DropTarget>().OrderBy(d => d.dropPriority).FirstOrDefault(),
            point = c.point
        });

        //Select the drop target with lowest priority
        var obj = result.OrderBy(c => c.dropTarget.dropPriority).FirstOrDefault(c => c.dropTarget != null && c.dropTarget.allowedDropGroup.Contains(dropGroup));

        if (obj != null)
        {
            //create drop object and call drop callback
            var dropPoint = obj.point;
            if (obj.dropTarget.dropAtFixedPoint)
            {
                dropPoint = obj.dropTarget.gameObject.transform.TransformPoint(obj.dropTarget.fixedPoint);
            }

            if (createDropObject && dropObject != null)
            {
                var dropObjectInstance =  GameObject.Instantiate(dropObject, new Vector3(dropPoint.x, dropPoint.y + dropYOffSet, dropPoint.z), new Quaternion());
                endDragEvent.Invoke(dropGroup, new Vector3(dropPoint.x, dropPoint.y + dropYOffSet, dropPoint.z), dropObjectInstance, obj.dropTarget.gameObject);
            }
            else
            {
                endDragEvent.Invoke(dropGroup, new Vector3(dropPoint.x, dropPoint.y + dropYOffSet, dropPoint.z), dropObject, obj.dropTarget.gameObject);
            }
        }
        //enable touch input
        touchInputs.ForEach(c => { c.enabled = true; });
    }
}
