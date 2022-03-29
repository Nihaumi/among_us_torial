using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AU_PlayerController : MonoBehaviour
{
    //to simulate multiplayer
    [SerializeField] bool hasControl;
    public static AU_PlayerController localPlayer; //singleton

    //player object
    Rigidbody myRB;
    Transform myAvatar;
    Animator myAnim;

    // player movement
    [SerializeField] InputAction WASD;
    Vector2 movementInput;
    [SerializeField] float movementSpeed;

    //player color
    static Color myColor;
    SpriteRenderer myAvatarSprite;

    //Role
    [SerializeField] bool isImposter;
    [SerializeField] InputAction KILL;
    List<AU_PlayerController> targets;
    [SerializeField] Collider myCollider;
    bool isDead;
    [SerializeField] GameObject bodyPrefab; //dead part

    //report
    public static List<Transform> allBodies;

    List<Transform> bodiesFound;

    [SerializeField] InputAction Report;
    [SerializeField] LayerMask ignoreForBody;

    //Interaction
    [SerializeField] InputAction Mouse;
    Vector2 mousePositionInput;
    Camera myCamera;
    [SerializeField] InputAction Interaction;
    [SerializeField] LayerMask interactLayer;

    private void Awake()
    {
        KILL.performed += KILLtargets;
        Report.performed += ReportBody;
        Interaction.performed += Interact;
    }

    private void OnEnable()
    {
        WASD.Enable();
        KILL.Enable();
        Report.Enable();
        Mouse.Enable();
        Interaction.Enable();
    }

    private void OnDisable()
    {
        WASD.Disable();
        KILL.Disable();
        Report.Disable();
        Mouse.Disable();
        Interaction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //basics
        myRB = GetComponent<Rigidbody>();
        myAvatar = transform.GetChild(0);
        myAnim = GetComponent<Animator>();
        targets = new List<AU_PlayerController>();

        //to simulate multiplayer
        if (hasControl)
        {
            localPlayer = this;
        }

        //color
        myAvatarSprite = myAvatar.GetComponent<SpriteRenderer>();
        if (myColor == Color.clear)
        {
            myColor = Color.white;
        }
        if (!hasControl)
        {
            return;
        }
        myAvatarSprite.color = myColor;

        //report
        allBodies = new List<Transform>();
        bodiesFound = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasControl)
        {
            return;
        }

        //movement
        movementInput = WASD.ReadValue<Vector2>();

        if (movementInput.x != 0)
        {
            myAvatar.localScale = new Vector2(Mathf.Sign(movementInput.x), 1); //mathf.sign(positive) = 1; mathf.sign(negative) = -1
        }

        myAnim.SetFloat("Speed", movementInput.magnitude);// magnitude: macht x und y position zu 1 positiver variable

        //repot
        if (allBodies.Count > 0)
        {
            BodySearch();
        }

        mousePositionInput = Mouse.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        myRB.velocity = movementInput * movementSpeed;
        mousePositionInput = movementInput * movementSpeed;
    }


    //color
    public void SetColor(Color newColor)
    {
        myColor = newColor;
        if (myAvatarSprite != null)
        {
            myAvatarSprite.color = myColor;
        }
    }

    //Role
    public void SetRole(bool newRole)
    {
        isImposter = newRole;
    }

    private void OnTriggerEnter(Collider other) //adds touched player as killing option
    {
        if (other.tag == "Player")
        {
            AU_PlayerController temptarget = other.GetComponent<AU_PlayerController>();
            if (isImposter)
            {
                if (temptarget.isImposter)//checks if touched player is fellow imposter
                {
                    return;
                }
                else
                {
                    targets.Add(temptarget);
                    return;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) //if not touching other player, remove them as kill option
    {
        if (other.tag == "Player")
        {
            AU_PlayerController tempTarget = other.GetComponent<AU_PlayerController>();
            if (targets.Contains(tempTarget))
            {
                targets.Remove(tempTarget);
            }
        }
    }

    void KILLtargets(InputAction.CallbackContext context) // kills target
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (targets.Count == 0)
            {
                return;
            }
            else
            {
                if (targets[targets.Count - 1].isDead) // checks if last touched player is ghost???
                {
                    return;
                }

                transform.position = targets[targets.Count - 1].transform.position;
                targets[targets.Count - 1].Die();
                targets.RemoveAt(targets.Count - 1);
            }
        }
    }

    public void Die()
    {
        isDead = true;

        myAnim.SetBool("IsDead", isDead);
        myCollider.enabled = false;
        gameObject.layer = 6;

        AU_Body tempBody = Instantiate(bodyPrefab, transform.position, transform.rotation).GetComponent<AU_Body>();
        tempBody.SetColor(myAvatarSprite.color);
    }

    void BodySearch()
    {
        foreach (Transform body in allBodies)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, body.position - transform.position);
            Debug.DrawRay(transform.position, body.position - transform.position, Color.cyan);//visible ray

            if (Physics.Raycast(ray, out hit, 1000f, ~ignoreForBody))//true if ray hits object
            {
                if (hit.transform == body)
                {
                    Debug.Log(hit.transform.name);
                    Debug.Log(bodiesFound.Count);
                    if (bodiesFound.Contains(body.transform))//dont add same body again
                    {
                        return;
                    }

                    bodiesFound.Add(body.transform);
                }
                else
                {
                    bodiesFound.Remove(body.transform);
                }
            }
        }
    }

    //report
    void ReportBody(InputAction.CallbackContext obj)
    {
        if (bodiesFound == null)
        {
            return;
        }
        if (bodiesFound.Count == 0)
        {
            return;
        }
        Transform tempBody = bodiesFound[bodiesFound.Count - 1];
        allBodies.Remove(tempBody);
        bodiesFound.Remove(tempBody);
        tempBody.GetComponent<AU_Body>().Report();
    }

    //interaction minigame(task)
    void Interact(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Here");
            RaycastHit hit;
            Ray ray = myCamera.ScreenPointToRay(mousePositionInput);

            if(Physics.Raycast(ray, out hit, interactLayer)){
                if(hit.transform.tag == "Interactable")
                {
                    if (!hit.transform.GetChild(0).gameObject.activeInHierarchy)
                    {
                        return;
                    }
                    AU_Interactable temp = hit.transform.GetComponent<AU_Interactable>();
                    temp.PlayMiniGame();
                }
            }
        }
    }
}
