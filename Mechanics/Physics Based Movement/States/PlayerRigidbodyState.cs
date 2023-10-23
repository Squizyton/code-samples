using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using Managers;
using Player.Movement.Manager;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using static Player.Movement.Manager.PlayerMovement;

namespace Player.Movement.States
{
    public class PlayerRigidbodyState
    {
        private PlayerMovementStats _stats;
        private PlayerControls _controls;
        private PlayerMovement _movementManager;
        private Rigidbody rb;
        private Transform orientation;

        private bool _grounded;
        private bool _sprinting;
        private bool _jumping;
        private bool _pressedToMove;

        private bool _hadjumped;
        private bool _hadStartedWalking;

        private bool _crouching;

        //private variables
        private bool _readyToJump = true;

        //Input TODO::Replace this with actual Input from Unity input System
        private float x, y;

        private Vector3 normalVector = Vector3.up;
        private WaitForSeconds _jumpCooldown;




        public PlayerRigidbodyState(PlayerMovement movement,PlayerMovementStats stats, Rigidbody rigidbody, Transform orientation,PlayerControls controls)
        {
            _stats = stats;
            _movementManager = movement;
            _controls = controls;
            this.orientation = orientation;
            rb = rigidbody;
        }

        public void EnterState()
        {
            _jumpCooldown = new WaitForSeconds(_stats.jumpCooldown);
            _controls.Player.Movement.performed += GetMovement;
            _controls.Player.Movement.canceled += GetMovement;
            _controls.Player.Movement.performed += ctx => _pressedToMove = true;
            _controls.Player.Movement.canceled += ctx => _pressedToMove = false;
            _controls.Player.Jump.performed += ctx => Jump();
        }

        public void FixedUpdateState(Vector3 movementInput)
        {
            Movement();

            if(_pressedToMove)
                _movementManager.PlayerMoveSFX();
            else 
                _movementManager.PlayerStopMoveSFX();
        }
    

        public void LeaveState()
        {
            
        }

        public void NormalUpdate()
        {
            

            if (!_controls.Player.Movement.IsPressed()) return;

            x = _controls.Player.Movement.ReadValue<Vector2>().x;
            y = _controls.Player.Movement.ReadValue<Vector2>().y;
        }


        void GetMovement(InputAction.CallbackContext context)
        {
            //TODO:: Change this to Vector2
            x = context.ReadValue<Vector2>().x;
            y = context.ReadValue<Vector2>().y;
        }

        private void Movement()
        {
            //Extra Gravity to make sure player isn't flying all over the place
            rb.AddForce(Vector3.down * (Time.deltaTime * _stats.extraGravity));

            //Find the actual velocity relative to where player is currently looking
            Vector3 mag = FindVelRelativeToLook();
           
            
            
            float xMag = mag.x, yMag = mag.y;

            //Counteract sliding and sloppy movement
            CounterMovement(x, y, mag);

            //If Holding jump down && ready to jump, then jump again
            if (_readyToJump && _movementManager.jumping) Jump();

            //set the max speed a player can go 
            float maxSpeed = _stats.maxSpeed;


            //if speed is larger then maxspeed, cancel out the input so you don't go over max speed
            //If x > 0, and the magnitude of x is greater then the max speed, then set x to 0 so player can't surpass the max speed
            if (x > 0 && xMag > (maxSpeed + PlayerContainer.Instance.stats.extraSpeed)) x = 0;
            //Sif x < 0, and is less then the max speed, just...stop?
            if (x < 0 && xMag < (-maxSpeed- PlayerContainer.Instance.stats.extraSpeed)) x = 0;

            // Same as x, but...with Y
            if (y > 0 && yMag > (maxSpeed+ PlayerContainer.Instance.stats.extraSpeed)) y = 0;
            if (y < 0 && yMag < (-maxSpeed- PlayerContainer.Instance.stats.extraSpeed)) y = 0;


            //Multipliers to slow down movement/increase movement controler
            float multiplier = 1f, multiplierV = 1f;
            
            if (!_movementManager.IsGrounded())
            {
                multiplier = .5f;
                multiplierV = 0.5f;
            }

            //Movement while sliding

            if (_movementManager.IsGrounded() && _crouching) multiplierV = 0f;

            //Apply all the forces generated to move player
            rb.AddForce(orientation.transform.forward * y * (_stats.moveSpeed * _stats.sprintScale) * Time.deltaTime * multiplier * multiplierV);
            rb.AddForce(orientation.transform.right * x * (_stats.moveSpeed * _stats.sprintScale) * Time.deltaTime * multiplier);
        }

        #region Jumping

        private void Jump()
        {
         
            if (_movementManager.IsGrounded() && _readyToJump)
            {
                
                _readyToJump = false;
                _movementManager.PlayerJumpSFX();
                _movementManager.PlayerStopMoveSFX();
                _hadStartedWalking = false;
                _hadjumped = true;

                rb.AddForce(Vector2.up * (_stats.jumpForce * .5f));
                rb.AddForce(_movementManager.normalVector * (_stats.jumpForce * 0.5f));

                //If jumping while falling, reset y velocity
                var velocity = rb.velocity;
                var vel = velocity;

                velocity = velocity.y switch
                {
                    < 0.5f => new Vector3(vel.x, 0, vel.z),
                    > 0 => new Vector3(vel.x, vel.y / x, vel.z),
                    _ => velocity
                };
                rb.velocity = velocity;
            }

            //Invoke ResetJump with the jumpCooldown
            GameManager.instance.AddCoroutine(ResetJump());
        }

        IEnumerator ResetJump()
        {
            yield return _jumpCooldown;
            _readyToJump = true;
        }

        #endregion


        private void CounterMovement(float x, float y, Vector2 mag)
        {
            //If we AREN'T ON THE GROUND, IT DOESN"T MATTER
            if (!_movementManager.IsGrounded() || _movementManager.jumping) return;


            //Counter movement
            if (Math.Abs(mag.x) > _stats.threshold && Math.Abs(x) < 0.05f || (mag.x < -_stats.threshold && x > 0) ||
                (mag.x > _stats.threshold && x < 0))
            {
                rb.AddForce(orientation.transform.right * (_stats.moveSpeed * Time.deltaTime * -mag.x * _stats.counterMovement));
            }  if (Math.Abs(mag.y) > _stats.threshold && Math.Abs(y) < 0.05f || (mag.y < -_stats.threshold && y > 0) || (mag.y > _stats.threshold && y < 0)) {
                rb.AddForce(orientation.transform.forward * (_stats.moveSpeed * Time.deltaTime * -mag.y * _stats.counterMovement));
            }
            //Limit diagonal running
            //By using Pythagorean Thearom, we are getting the maginitude of the x and z combined together (diagonal) and if we are going faster then the max speed 
            if (!(MathF.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > _stats.maxSpeed)) return;
            
            var velocity = rb.velocity;
            var fallSpeed = velocity.y;

            //we cut the velocity
            Vector3 n = velocity.normalized * _stats.maxSpeed;

            //And set the velocity to what it should be
            velocity = new Vector3(n.x, fallSpeed, n.z);
            rb.velocity = velocity;
        }

        /// <summary>
        /// Find the velocity relative to where the player is looking
        /// Useful for vectors calculations regarding movement an dlmiting movement
        /// </summary>
        /// <returns></returns>
        private Vector2 FindVelRelativeToLook()
        {
            var lookAngle = orientation.transform.transform.eulerAngles.y;
            //Get the angle that we are moving in
            var moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

            //Get the angle that we are looking in vs the angle we are actually going
            var u = Mathf.DeltaAngle(lookAngle, moveAngle);
            var v = 98 - u;
            
            
            var magnitude = rb.velocity.magnitude;

            var yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
            var xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);
            return new Vector2(xMag, yMag);
        }
        public void SetHadJumped(bool x)
        {
            _hadjumped = x;
            //_movementManager.currentMovementMode = MovementState.inAir;
        }

    }
}