using SuperMobileController;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private GameObject activeObj;

    private void Start()
    {
        //FrameRate Testing
        //Application.targetFrameRate = 20;
    }

    public void MoveSelectedToTarget(TouchObject selectObject, Vector3 target)
    {
        TouchObject.selectedObjects.Where(c => c.selectionType == "Player").ToList().ForEach(c =>
        {
            var cc = c.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.MoveToTarget(selectObject, target);
            }
        });
    }

    public void SetAttackTarget(TouchObject selectObject, Vector3 target)
    {
        TouchObject.selectedObjects.Where(c => c.selectionType == "Player").ToList().ForEach(c =>
        {
            var cc = c.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.attackTarget = selectObject.gameObject;
            }
        });
    }

    public void DisablePlayer(string dropGroup, Vector3? dropPos, GameObject obj, GameObject target)
    {
        //Disable Character Controller of the dragging object
        obj.GetComponent<UnityEngine.CharacterController>().enabled = false;

        //Make the object transparent
        var mat = obj.transform.Find("yuji_body").GetComponent<SkinnedMeshRenderer>().material;
        mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 0.3f));
        mat = obj.transform.Find("yuji_eye").GetComponent<SkinnedMeshRenderer>().material;
        mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 0.3f));
        mat = obj.transform.Find("yuji_face").GetComponent<SkinnedMeshRenderer>().material;
        mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 0.3f));
        mat = obj.transform.Find("yuji_head").GetComponent<SkinnedMeshRenderer>().material;
        mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 0.3f));
    }

    public void AssignEvent(string dropGroup, Vector3? dropPos, GameObject obj, GameObject target)
    {
        obj.GetComponent<TouchObject>().tapSelectEvent.AddListener(SetAttackTarget);
        obj.GetComponent<TouchObject>().tapReselectEvent.AddListener(SetAttackTarget);
    }

    public void DisableDrop(string dropGroup, Vector3? dropPos, GameObject obj, GameObject target)
    {
        target.GetComponent<DropTarget>().allowedDropGroup = new string[] { };
        target.GetComponent<TowerPosition>().tower = obj;
    }

    public void TowerPostionTouch(TouchObject selectObject, Vector3 target)
    {
        if (selectObject.GetComponent<TowerPosition>().tower != null)
        {
            GameObject.FindObjectOfType<CircleButtonCollection>().Show("", Camera.main.WorldToScreenPoint(selectObject.transform.position));
            activeObj = selectObject.gameObject;
        }
    }

    public void TowerAction(string actionName, Vector2 pos)
    {
        var tp = activeObj.GetComponent<TowerPosition>();
        switch (actionName)
        {
            case "sell":
                activeObj.GetComponent<DropTarget>().allowedDropGroup = new string[] { "tower" };
                GameObject.Destroy(tp.tower);
                tp.tower = null;
                break;
            case "upgrade":
                tp.tower.transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case "cancel":
                break;
        }
        GameObject.FindObjectOfType<CircleButtonCollection>().Hide();
    }

    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
