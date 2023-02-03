using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Animations;

public class PlayerBehaviour : NetworkBehaviour
{
    // Start is called before the first frame update
    public CharacterController nccp;
    public float speed;
    [Header("Character Controller Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse = 8.0f;
    public float acceleration = 10.0f;
    public float braking = 10.0f;
    public float maxSpeed = 2.0f;
    public float rotationSpeed = 15.0f;
    public Transform Body;
    

    [Networked]
    public bool IsGrounded { get; set; }

    [Networked]
    [HideInInspector]
    public Vector3 Velocity { get; set; }
    [Networked]
    public bool isMoving { get; set; }
    [Networked]
    public bool isJumping { get; set; }

    public bool AllowMovement = true;
    private void Awake()
    {
        Body = transform.GetChild(0);

    }
    void Start()
    {
        nccp = GetComponent<CharacterController>();
        Body = transform.GetChild(0);

        ConstraintSource cs = new ConstraintSource();
        cs.sourceTransform = Camera.main.transform;
        cs.weight = 1;
        LookAtConstraint lookAt = GetComponentInChildren<LookAtConstraint>();
        lookAt.constraintActive = true;
        lookAt.AddSource(cs);

    }

    public override void Spawned()
    {
        base.Spawned();
        FindObjectOfType<DataManager>().RPC_UpdateChar();
    }



    public void UpdateChar(int avatarIndex)
    {
        for(int i = 0; i < 3; i++)
        {
            if(i == avatarIndex) 
            {
                Body.GetChild(i).gameObject.SetActive(true);
                Avatar playerAvatar = Body.GetChild(avatarIndex).GetComponent<Animator>().avatar;
                GetComponent<Animator>().avatar = playerAvatar;
            }
            else
            {
                Body.GetChild(i).gameObject.SetActive(false);
            }
        }
        

    }
    // Update is called once per frame

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object.InputAuthority == Runner.LocalPlayer && AllowMovement)
        {
            Vector3 direction = Vector3.zero;
            direction += Input.GetKey(KeyCode.W) ? Vector3.forward : Vector3.zero;
            direction += Input.GetKey(KeyCode.A) ? Vector3.left : Vector3.zero;
            direction += Input.GetKey(KeyCode.S) ? Vector3.back : Vector3.zero;
            direction += Input.GetKey(KeyCode.D) ? Vector3.right : Vector3.zero;
            Move(direction);
            isMoving = direction != Vector3.zero;
            isJumping = Input.GetKey(KeyCode.Space);
          
        }
        GetComponent<Animator>().SetBool("IsGrounded", IsGrounded);

        if (isJumping)
        {
            Jump();
            GetComponent<Animator>().SetTrigger("Jumping");
        }


        GetComponent<Animator>().SetBool("Moving", isMoving);

        
    }
    public virtual void Move(Vector3 direction)
    {
        var deltaTime = Runner.DeltaTime;
        var previousPos = transform.position;
        var moveVelocity = Velocity;

        direction = direction.normalized;

        if (IsGrounded && moveVelocity.y < 0)
        {
            moveVelocity.y = 0f;
        }

        moveVelocity.y += gravity * Runner.DeltaTime;

        var horizontalVel = default(Vector3);
        horizontalVel.x = moveVelocity.x;
        horizontalVel.z = moveVelocity.z;

        if (direction == default)
        {
            horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
        }
        else
        {
            horizontalVel = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Runner.DeltaTime);
        }

        moveVelocity.x = horizontalVel.x;
        moveVelocity.z = horizontalVel.z;

        nccp.Move(moveVelocity * deltaTime);

        Velocity = (transform.position - previousPos) * Runner.Simulation.Config.TickRate;
        IsGrounded = nccp.isGrounded;
    }
    public virtual void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
    {
        if (IsGrounded || ignoreGrounded)
        {
            var newVel = Velocity;
            newVel.y += overrideImpulse ?? jumpImpulse;
            Velocity = newVel;
        }
    }
}

