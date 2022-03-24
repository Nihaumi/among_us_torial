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

    private void Awake()
    {
        KILL.performed += KILLtargets;
    }

    private void OnEnable()
    {
        WASD.Enable();
        KILL.Enable();
    }

    private void OnDisable()
    {
        WASD.Disable();
        KILL.Disable();
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
        if(myColor == Color.clear)
        {
            myColor = Color.white;
        }
        if (!hasControl)
        {
            return;
        }
        myAvatarSprite.color = myColor;
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

        if(movementInput.x != 0)
        {
            myAvatar.localScale = new Vector2(Mathf.Sign(movementInput.x), 1); //mathf.sign(positive) = 1; mathf.sign(negative) = -1
        }

        myAnim.SetFloat("Speed", movementInput.magnitude);// magnitude: macht x und y position zu 1 positiver variable
    }

    private void FixedUpdate()
    {
        myRB.velocity = movementInput * movementSpeed;
    }


    //color
    public void SetColor(Color newColor)
    {
        myColor = newColor;
        if(myAvatarSprite != null)
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
        if(other.tag == "Player")
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
        if(context.phase == InputActionPhase.Performed)
        {
            if(targets.Count == 0)
            {
                return;
            }
            else
            {
                if (targets[targets.Count -1].isDead) // checks if last touched player is ghost???
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

        AU_Body tempBody = Instantiate(bodyPrefab, transform.position, transform.rotation).GetComponent<AU_Body>();
        tempBody.SetColor(myAvatarSprite.color);
    }
}
