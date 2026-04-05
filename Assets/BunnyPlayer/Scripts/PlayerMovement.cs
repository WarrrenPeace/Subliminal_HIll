using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    Rigidbody RB;
    Animator AM;
    Camera camRef;
    [SerializeField] InputActionReference move;
    private bool IsWalking = false;
    Vector3 inputDirection;
    [SerializeField] Transform model;
    [SerializeField] float speed;

    void Start()
    {
        RB = GetComponent<Rigidbody>();
        AM = GetComponent<Animator>();

        camRef = Camera.main;

        if(!IsOwner) //If not the owner of this object it is not needed at all on that client
        {Destroy(this);}
    }
    void PlayerInput()
    {
        if(move.action.triggered) //initial click
        {
            AM.SetBool("isWalking",true);
            IsWalking = true;
        }
        if(move.action.IsPressed()) // bool for holding down spacebar
        {
            //Debug.Log("Pressed");
        }
        else
        {
            if(IsWalking)
            {
                IsWalking = false;
                AM.SetBool("isWalking",false);
            }
            
        }
    }
    public void GetMovementDirection()
    {
        inputDirection.x = move.action.ReadValue<Vector2>().x;
        inputDirection.z = move.action.ReadValue<Vector2>().y;
        
        if(move.action.ReadValue<Vector2>() != Vector2.zero)
        {
            Vector3 camForward = camRef.transform.forward.normalized;
            Vector3 camRight = camRef.transform.right.normalized;
            camForward.y = 0;
            camRight.y = 0;
            inputDirection = camRight * inputDirection.x + camForward * inputDirection.z;

            Rotate();
            //Quaternion targetRotation = Quaternion.LookRotation(inputDirection.normalized);
            //model.transform.rotation = Quaternion.RotateTowards(model.transform.rotation, targetRotation, 720 * Time.deltaTime);
        }

        
    }

    void Update()
    {
        if(IsOwner)
        {
            PlayerInput();
            GetMovementDirection();
        }
    }
    void FixedUpdate()
    {
        if(IsOwner) 
        {
            Move();
        }
    }
    void Move()
    {
        RB.AddForce(inputDirection.normalized * speed);
    }
    void Rotate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(inputDirection.normalized);
        //model.transform.rotation = Quaternion.RotateTowards(model.transform.rotation, targetRotation, 720 * Time.deltaTime);
        RB.MoveRotation(targetRotation);
    }
}
