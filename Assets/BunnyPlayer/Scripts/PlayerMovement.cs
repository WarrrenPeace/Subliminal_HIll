using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    Rigidbody rb;
    Camera camRef;
    [SerializeField] InputActionReference move;
    Vector3 inputDirection;
    [SerializeField] float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camRef = Camera.main;

        if(!IsOwner) //If not the owner of this object it is not needed at all on that client
        {Destroy(this);}
    }
    public void MoveInput()
    {
        inputDirection.x = move.action.ReadValue<Vector2>().x;
        inputDirection.z = move.action.ReadValue<Vector2>().y;
        
        if(move.action.ReadValue<Vector2>() != Vector2.zero)
        {
            Debug.Log("MOVING");
            RotatePlayerTowardsCameraDirection();
            
            inputDirection = camRef.transform.forward.normalized * inputDirection.x + camRef.transform.forward.normalized * inputDirection.z;
        }

        
    }
    void RotatePlayerTowardsCameraDirection()
    {
        transform.forward = camRef.transform.forward;
    }

    void Update()
    {
        if(IsOwner)
        {
            MoveInput();
        }
    }
    void FixedUpdate()
    {
        if(IsOwner) {Move();}
    }
    void Move()
    {
        rb.AddForce(inputDirection.normalized * speed);
    }
}
