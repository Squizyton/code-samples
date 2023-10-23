using System;
using UnityEngine;

namespace Player.Movement.States
{
    [Obsolete]
    public class PlayerControllerMoveState : PlayerBaseState
    {
        #region I don't think we honestly need

        /// <summary>
        /// We really don't need these since we already have a reference to movement. We are just...taking up more memory then we need
        /// </summary>
/*
        private Vector3 _move;
        private float _velocityY;
        private float moveSpeed;
        private float acceleration;

        private float gravity;
        private float jumpHeight;
        private float _velocityX, _velocityZ;
        private Vector3 velocity;
        
        private CharacterController controller;

        private bool isGrounded;

        private Transform playerTransform;
        private Rigidbody rb;

        private CapsuleCollider collider;
        */

        #endregion

        private Vector3 _velocity;

        private Vector3 _move;

        private float _velocityY, _velocityX, _velocityZ;

        private const float ReturnHookThreshold = 23f;

        public override void EnterState()
        {

        }

        public override void UpdateState()
        {
            if (_movement.grappled)
            {
                var difference = _movement.transform.position - _movement.oRb.position;
               

               
                if (difference.sqrMagnitude > ReturnHookThreshold)
                {
                    //_movement.ReturnHook();
                }

                if (_velocity.y <= -4f)
                {
                    _movement.capCollider.enabled = true;
                    _movement.controller.enabled = false;
                    _movement.rb.velocity = new Vector3(_move.x, _velocityY, _move.z);
                    //_movement.SwitchState(new GrappleState(_movement));
                }

                if (!_movement.isGrounded)
                {
                    _movement.rb.isKinematic = false;
                    _movement.controller.enabled = false;
                    _movement.capCollider.enabled = true;

                    _movement.rb.AddForce(Vector3.up * 12f + _movement.transform.forward * 6f, ForceMode.VelocityChange);

                   // _movement.ReturnHook();
                    //_movement.SwitchState(_movement.FlyState);
                }
            }

            //_movement.isGrounded = _movement.isGrounded;

            if ( _movement.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            //Moving
            var x = Input.GetAxisRaw("Horizontal");
            var z = Input.GetAxisRaw("Vertical");

            var movementVector = new Vector2(x, z).normalized;
            
            if (x == 0f)
            {
                _velocityX *= 0.92f;
            }

            if (z == 0f) 
            {
                _velocityZ *= 0.92f;
            }
            
            _velocityX += movementVector.x *  _movement.acceleration;
            _velocityZ += movementVector.y *  _movement.acceleration;

            _velocityX = Mathf.Clamp(_velocityX, - _movement.moveSpeed,  _movement.moveSpeed);
            _velocityZ = Mathf.Clamp(_velocityZ, - _movement.moveSpeed,  _movement.moveSpeed);

            var transform = _movement.transform;
            _move =  transform.right * _velocityX + transform.forward * _velocityZ;
            _movement.move = _move;

            _movement.controller.Move(_move * Time.deltaTime);

            //Jumping
           //if (Input.GetButtonDown("Jump") && _movement.isGrounded)
           //{
           //    _velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
           //}

            //Gravity
            _velocity.y += _movement.gravity * Time.deltaTime;
            _movement.controller.Move(_velocity * Time.deltaTime);
        }

        public PlayerControllerMoveState(OldPlayerMovement oldPlayerMovement) : base(oldPlayerMovement)
        {
        }
    }
}