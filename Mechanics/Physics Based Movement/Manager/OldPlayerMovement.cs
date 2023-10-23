using System;
using Player.Movement.States;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Movement
{
    public class OldPlayerMovement : MonoBehaviour
    {
        public CharacterController controller;

        private Vector3 _move;
        private Transform _lineOrigin;
        private SpringJoint _joint;


       public CapsuleCollider capCollider;
        
        [Title("Components")]
        public Rigidbody rb;
        public Rigidbody oRb;
        

        [Title("Planet Variables")] public float gravity = -9.81f;
        
        [Title("Grapple")] public bool grappled = false;

        [Title("Movement")] public Vector3 move;
        public float moveSpeed = 8f;
        public float acceleration = 8f;
        
        [Title("Jumping")] [SerializeField] private float jumpHeight;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;
       public bool isGrounded;
        private float _velX, _velY;
    
        
        //Todo move to own script
        [Title("Hook")] 
        [SerializeField] private Transform hook;
        [SerializeField] private Vector3 intialHookPosition;
        [SerializeField] private Quaternion hookIntialRotation;
        [SerializeField] private Transform hand;

        private PlayerBaseState currentState;


        private void Start()
        {
            //TODO: Cache states during Game Jam
            currentState = new PlayerControllerMoveState(this);
            
            //Enter Movement state
            currentState.EnterState();

           // hookIntialRotation = hook.rotation;
           // intialHookPosition = hook.position;
        }


        private void Update()
        {
            isGrounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down),groundMask);
        }

        private void FixedUpdate()
        {
            //Since we are working with Physics based systems. You want all Movement done in FixedUpdate so everything can process properly
            currentState.UpdateState();
        }

        public void SwitchState(PlayerBaseState newState)
        {
            currentState = newState;
            currentState.EnterState();
        }
    }
}