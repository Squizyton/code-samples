using System;
using System.Collections;
using FMOD.Studio;
using Managers;
using Player.Movement.States;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Movement.Manager
{
	public class PlayerMovement : MonoBehaviour
	{
		private PlayerControls _controls;

		[SerializeField] private Transform playerCam;
		[SerializeField] private Transform orientation;

		private PlayerRigidbodyState _currentState;


		[Title("Movement Stats"), Required, SerializeField]
		private PlayerMovementStats movementStats;

		[Title("Rigidbody")][SerializeField] private Rigidbody rb;

        public FMOD.Studio.EventInstance FMODPlayerWalk;
        public string fmodSurface;

        //bools 
        [ShowInInspector] private bool _grounded;
		[ShowInInspector] private bool _sprinting;
		[ShowInInspector] public bool jumping;
        [HideInInspector] public Vector3 normalVector = Vector3.up;
        [SerializeField] private float walkspeed;

        [SerializeField] private bool hadjumped;
		private float jumpdelay;

        private Coroutine groundInvoke;

		[SerializeField] private float raycastDistance;

		// Start is called before the first frame update
		void Awake()
		{
			_controls = new PlayerControls();
			_controls.Enable();


			rb = GetComponent<Rigidbody>();
		}


		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			_currentState = new PlayerRigidbodyState(this, movementStats, rb, orientation, _controls);
			_currentState.EnterState();

            FMODPlayerWalk = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Player/Movement/Player_Footsteps");
        }




		private void FixedUpdate()
		{
			if (GameManager.instance.AmIInUI()) return;
			
			_currentState.FixedUpdateState(Vector3.zero);

            if (hadjumped)
			{
				if (jumpdelay > 0)
					jumpdelay--;
				else if (_grounded)
				{
                    PlayerLandSFX();
					hadjumped= false;
                }
            }
            walkspeed = rb.velocity.magnitude;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Speed", walkspeed);
        }


		private bool _cancellingGrounded;

		private bool IsFloor(Vector3 v)
		{
			var angle = Vector3.Angle(Vector3.up, v);
			//Are we at an angle or not?
			return angle < movementStats.maxSlopeAngle;
		}

		public void Update()
		{

			_currentState.NormalUpdate();


			//Checks if we are on ground or not
			if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out var hit,
					raycastDistance,
					movementStats.whatIsGround))
			{
				var normal = hit.normal;

				//Floor
				if (IsFloor(normal))
				{
					_grounded = true;
					_cancellingGrounded = false;
					normalVector = normal;
				}
			}
			else _grounded = false;
		}


		private IEnumerator StopGrounded(float delay)
		{
			yield return new WaitForSeconds(delay);
			_grounded = false;
		}


		#region Getters

		public bool IsGrounded()
		{
			return _grounded;
		}


		public bool IsSprinting()
		{
			return _sprinting;
		}


		private void OnDrawGizmos()
		{
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down * raycastDistance), Color.red);
		}

        #endregion
        public void PlayerMoveSFX()
        {
            PLAYBACK_STATE playbackState;
            FMODPlayerWalk.getPlaybackState(out playbackState);
			if (!_grounded && playbackState == PLAYBACK_STATE.PLAYING)
				PlayerStopMoveSFX();
            if (_grounded && !hadjumped && playbackState != PLAYBACK_STATE.PLAYING)
				FMODPlayerWalk.start();
            return;
        }

        public void PlayerStopMoveSFX()
        {
            //stop
            FMODPlayerWalk.stop(STOP_MODE.ALLOWFADEOUT);
            return;
        }
        public void PlayerJumpSFX()
        {
			PlayerStopMoveSFX();
            hadjumped = true;
			
			if (IsGrounded())
				FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Movement/Player_Jump");
            jumpdelay = 15f;
            return;
        }

        public void PlayerLandSFX()
        {
			_currentState.SetHadJumped(false);

            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Movement/Player_Land");
            return;
        }
    }
}