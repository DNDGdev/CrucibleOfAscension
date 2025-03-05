using SuperMobileController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacterStatus
{
    Idle,
    Attacking,
    drinking,
    Moving,
    Channelling,
    Dashing
}

public class CharacterController : MonoBehaviour
{
    [Header("General Settings")]
    public Animator animator;
    public GameObject skill1;
    public GameObject skill2;
    public GameObject skill3;
    public GameObject skill4;
    public float speed = 0.2f;
    public float dashSpeed = 0.8f;
    public CharacterStatus[] movableStatus = new CharacterStatus[] { CharacterStatus.Idle, CharacterStatus.Moving };
    public CharacterStatus[] attackableStatus = new CharacterStatus[] { CharacterStatus.Idle, CharacterStatus.Moving };
    public CharacterStatus[] dashableStatus = new CharacterStatus[] { CharacterStatus.Idle, CharacterStatus.Moving };
    public SuperMobileController.CircleButtonCollection circleButtonCollection;
    public GameObject attackTarget = null;

    private CharacterStatus status = CharacterStatus.Idle;
    private Dictionary<string, GameObject> activeObjects = new Dictionary<string, GameObject>();

    private Vector3 movementInput = new Vector3();
    private UnityEngine.CharacterController characterController;
    private GameObject Camera;
    private Vector3? moveTarget;
    private Vector3 lastPosition;
    private float cantMoveCount = 0;
    private float attackCount = 0.5f;
    private Quaternion turnAngle = Quaternion.identity;
    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
        characterController = gameObject.GetComponent<UnityEngine.CharacterController>();
        Camera = GameObject.Find("Main Camera");
    }

    void FixedUpdate()
    {
        if (moveTarget != null)
        {
            var Target = (moveTarget.Value - transform.position).normalized * 10 * speed;
            if ((moveTarget.Value - new Vector3(transform.position.x, moveTarget.Value.y, transform.position.z)).magnitude <= 0.02f)
            {
                moveTarget = null;
                cantMoveCount = 0;
                movementInput = Vector3.zero;
                turnAngle = Quaternion.identity;
            }
            else
            {
                if (cantMoveCount > 0)
                {
                    if (lastPosition != transform.position)
                    {
                        cantMoveCount -= Time.deltaTime;
                    }
                }
                else
                {
                    turnAngle = Quaternion.identity;
                }

                movementInput = turnAngle * new Vector3(Target.x, 0, Target.z);
            }

            //if the position is not updated for 2 frames, that mean the character is blocked, so we stop it.
            if (lastPosition == transform.position)
            {
                cantMoveCount += Time.deltaTime;
            }
            else
            {
                lastPosition = transform.position;
            }
            if (!movableStatus.Contains(status))
            {
                cantMoveCount = 0;
            }
            if (cantMoveCount >= 0.5f)
            {
                if (attackTarget != null)
                {
                    //If can't move for too long, we turn 90 degree, not a good way to do path finding, it is just for demo.
                    turnAngle = Quaternion.AngleAxis(90, Vector3.up);
                    movementInput = turnAngle * new Vector3(Target.x, 0, Target.z);
                }
                else
                {
                    //If can't move for too long, we stop moving
                    moveTarget = transform.position;
                }
            }
        }

        if (attackTarget != null)
        {
            var targetDirection = attackTarget.transform.position - transform.position;
            if (targetDirection.magnitude <= 1f)
            {
                moveTarget = null;
                movementInput = Vector3.zero;
                if (attackCount >= 0.5f)
                {
                    transform.forward = targetDirection;
                    SetInput("attack", new Vector2(targetDirection.x, targetDirection.z));
                    attackCount = 0;
                }
            }
            else
            {
                moveTarget = attackTarget.transform.position;
            }
        }

        if (attackCount < 0.5f)
        {
            attackCount += Time.deltaTime;
        }

        if (status == CharacterStatus.Dashing)
        {
            characterController.Move(transform.forward * dashSpeed);
            movementInput = Vector3.zero;
        }

        if (movementInput.magnitude > 0 && movableStatus.Contains(status))
        {
            characterController.Move(movementInput * speed);
            transform.forward = Vector3.Lerp(transform.forward, movementInput.normalized, 0.5f);
            animator.SetBool("Run", true);
            status = CharacterStatus.Moving;
        }
        else
        {
            animator.SetBool("Run", false);
            movementInput = Vector3.zero;

            if (status == CharacterStatus.Moving)
                status = CharacterStatus.Idle;
        }
    }

    //For Touch Controller
    public void EndTouch(string inputName, Vector2 input, float s)
    {
        if (input.magnitude < 0.05)
        {
            if (attackableStatus.Contains(status))
            {
                status = CharacterStatus.Attacking;
                animator.SetTrigger("Attack");
                StartCoroutine(SetStatus(0.5f, CharacterStatus.Idle));
            }
        }
        else
        {
            //If dash speed is fast, we perform a dash
            if (s > 1 && dashableStatus.Contains(status))
            {
                status = CharacterStatus.Dashing;
                transform.forward = new Vector3(input.x, 0, input.y);
                animator.SetBool("Run", false);
                animator.SetTrigger("Dash");
                StartCoroutine(SetStatus(0.2f, CharacterStatus.Idle));
            }
        }
        //reset the movement input on drag ended
        movementInput = Vector3.zero;
    }

    public void MovementTouch(string inputName, Vector2 input, float s)
    {
        if (movableStatus.Contains(status))
        {
            status = CharacterStatus.Moving;
            movementInput = new Vector3(input.x, 0, input.y).normalized;
        }
        else
        {
            movementInput = Vector3.zero;
        }
    }

    public void MoveToTarget(TouchObject selectObject, Vector3 target)
    {
        attackTarget = null;
        moveTarget = target;
    }

    //For Gamepad
    public void SetInput(string inputName, Vector2 input)
    {
        if (inputName == "Movement")
        {
            movementInput = new Vector3(input.x, 0, input.y);
        }
        else if (attackableStatus.Contains(status))
        {
            switch (inputName.ToLower())
            {
                case "skill1":
                    animator.SetTrigger("Cast");
                    if (input.magnitude > 0)
                        transform.forward = new Vector3(input.x, 0, input.y);

                    if (skill1 != null)
                    {
                        status = CharacterStatus.Attacking;
                        if (input.magnitude == 0)
                        {
                            input = new Vector2(transform.forward.x, transform.forward.z);
                        }
                        StartCoroutine(CreateSkillObjectDelay(skill1, 0.5f, new Vector3(0, transform.position.y + 0.5f, 0), input, inputName));
                    }

                    break;
                case "attack":
                    if (attackableStatus.Contains(status))
                    {
                        status = CharacterStatus.Attacking;
                        animator.SetTrigger("Attack");
                        StartCoroutine(SetStatus(0.8f, CharacterStatus.Idle));
                    }
                    break;
                case "item1":
                case "item2":
                case "item3":
                    status = CharacterStatus.drinking;
                    animator.SetTrigger("Drink");
                    StartCoroutine(SetStatus(0.9f, CharacterStatus.Idle));
                    break;
            }
        }
    }

    public void SetTargetInput(string inputName, Vector3 input)
    {
        if (attackableStatus.Contains(status))
        {
            switch (inputName.ToLower())
            {
                case "skill2":
                    animator.SetTrigger("Cast");
                    if (input.magnitude > 0)
                        transform.forward = new Vector3(input.x, 0, input.z);
                    if (skill2 != null)
                    {
                        status = CharacterStatus.Attacking;
                        if (input.magnitude == 0)
                        {
                            input = new Vector2(transform.forward.normalized.x, transform.forward.normalized.z);
                        }
                        StartCoroutine(CreateSkillObjectDelay(skill2, 0.5f, input, Vector2.zero, inputName));
                    }
                    break;
                case "skill4":
                    animator.SetTrigger("Cast");
                    if (input.magnitude > 0 && movementInput.magnitude == 0)
                        transform.forward = new Vector3(input.x, 0, input.z);

                    if (input.magnitude == 0)
                    {
                        input = new Vector2(transform.forward.normalized.x, transform.forward.normalized.z);
                    }

                    if (skill4 != null)
                    {
                        status = CharacterStatus.Channelling;
                        StartCoroutine(CreateSkillObjectDelay(skill4, 0, input, Vector2.zero, inputName));
                    }

                    break;
            }
        }
    }

    public void SetFacing(string inputName, Vector2 input)
    {
        switch (inputName.ToLower())
        {
            case "skill3":
                transform.forward = new Vector3(input.x, 0, input.y);
                status = CharacterStatus.Channelling;
                break;
        }
    }
    public void SetBeginInput(string inputName)
    {
        switch (inputName.ToLower())
        {
            case "skill3":
                animator.SetBool("Casting", true);
                if (skill3 != null)
                {
                    StartCoroutine(CreateSkillObjectDelay(skill3, 0.2f, Vector3.zero, Vector2.zero, inputName, true));
                }

                break;
        }
    }

    public void SetEndInput(string inputName, Vector2 input)
    {
        switch (inputName.ToLower())
        {
            case "skill3":
                animator.SetBool("Casting", false);
                if (activeObjects.ContainsKey(inputName) && activeObjects[inputName] != null)
                    Destroy(activeObjects[inputName]);
                else
                    StartCoroutine(DelayDestroy(inputName, 0.6f));

                status = CharacterStatus.Idle;
                break;
        }
    }

    IEnumerator SetStatus(float delay, CharacterStatus value)
    {
        yield return new WaitForSeconds(delay);
        status = value;
    }

    IEnumerator DelayDestroy(string inputName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeObjects.ContainsKey(inputName) && activeObjects[inputName] != null)
            Destroy(activeObjects[inputName]);
    }

    IEnumerator CreateSkillObjectDelay(GameObject obj, float delay, Vector3 position, Vector2 forward, string skillName, bool parentPlayer = false)
    {
        yield return new WaitForSeconds(delay);
        GameObject newObj;
        if (!parentPlayer)
        {
            if (forward.magnitude > 0)
            {
                var forwardOffset = forward.normalized * 0.5f;
                position += new Vector3(forwardOffset.x, 0, forwardOffset.y);
            }
            newObj = Instantiate(obj, new Vector3(transform.position.x, 0, transform.position.z) + position, new Quaternion());
            if (movementInput.magnitude == 0 || forward.magnitude > 0)
            {
                if (new Vector3(forward.x, 0, forward.y).magnitude > 0)
                {
                    newObj.transform.forward = new Vector3(forward.x, 0, forward.y);
                }
            }
        }
        else
        {
            newObj = Instantiate(obj, transform);
        }

        if (activeObjects.ContainsKey(skillName))
        {
            activeObjects[skillName] = newObj;
        }
        else
        {
            activeObjects.Add(skillName, newObj);
        }
        status = CharacterStatus.Idle;
    }
}
