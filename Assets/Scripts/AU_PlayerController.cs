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

    AU_PlayerController target;
    [SerializeField] Collider myCollider;

    bool isDead;

    private void Awake()
    {
        KILL.performed += KILLTarget;
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

    private void OnTriggerEnter(Collider other) //checks if touched player is fellow imposter
    {
        if(other.tag == "Player")
        {
            AU_PlayerController tempTarget = other.GetComponent<AU_PlayerController>();
            if (isImposter)
            {
                if (tempTarget.isImposter)
                {
                    return;
                }
                else
                {
                    target = tempTarget;
                    Debug.Log(target.name);
                }
            }
        }
    }

    void KILLTarget(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            if(target == null)
            {
                return;
            }
            else
            {
                if (target.isDead)
                {
                    return;
                }

                transform.position = target.transform.position; 
                target.Die();
                target = null;
            }
        }
    }

    public void Die()
    {
        isDead = true;

        myAnim.SetBool("IsDead", isDead);
        myCollider.enabled = false;
    }
}
