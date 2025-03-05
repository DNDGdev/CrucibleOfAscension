using UnityEngine;
using UnityEngine.Events;

namespace SuperMobileController
{
    //Define the event 
    [System.Serializable]
    public class OnResultEvent : UnityEvent<string, Vector2>
    {
    }
    [System.Serializable]
    public class OnTargetResultEvent : UnityEvent<string, Vector3>
    {
    }
    [System.Serializable]
    public class OnDragBeginEvent : UnityEvent<string>
    {
    }
    [System.Serializable]
    public class OnTouchResultEvent : UnityEvent<string, Vector2, float>
    {
    }
    [System.Serializable]
    public class OnTouchSelectEvent : UnityEvent<TouchObject, Vector3>
    {
    }
    [System.Serializable]
    public class OnTouchAreaSelectEvent : UnityEvent<TouchObject>
    {
    }
    [System.Serializable]
    public class OnDeselectEvent : UnityEvent<TouchObject>
    {
    }
    [System.Serializable]
    public class OnDragButtonEvent : UnityEvent<string, Vector3?, GameObject, GameObject>
    {
    }
}