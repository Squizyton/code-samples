 using System;
using System.Collections;
using Dream;
using Player;
using Quick_Time_Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using static UnityEditor.Profiling.RawFrameDataView;

namespace Fishing
{
    public class FishingManager : MonoBehaviour
    {
        public static FishingManager instance;

        [SerializeField] private bool hasDoneTutorial;


        [SerializeField] private Transform theFish;
        [SerializeField] private Slider progressionBar;
        [SerializeField] private Rigidbody2D circleThing;
        [SerializeField] private EventReference reelSound;
        private EventInstance FE_ReelSound;
        private EventInstance FE_fishingSnapshot;
        [SerializeField] private EventReference successSound;
        [SerializeField] private EventReference failSound;
        [SerializeField] private EventReference rodReadyToReal;
        [SerializeField] private EventReference reelSoundMind;
        [SerializeField] private EventReference rodReadyToRealMind;
        private EventInstance FE_ReelSoundMind;

        [Title("References")] [SerializeField] private FishAI fishAI;
        [SerializeField] private PlayerInteraction interaction;

        [Title("Timer Settings")] private float biteTimer;

        [SerializeField, MinMaxSlider(0, 10, true)]
        private Vector2 minMaxTimeWait;

        private bool _failedToReelIn;
        private float _timeTillFail;
        private bool timerStarted;
        private bool _canReelIn;

        [Title("Circle Variables")] [SerializeField]
        private float circleSpeed;

        public bool isFishing;


        [Title("Bounds")] [SerializeField] private CircleCollider2D bounds;


        private PlayerControls _controls;
        private Vector2 movePos;
        private bool onFish;

        private FishData selectedFish;

        //Prevents from automatically failing at the beginning. If turned off failing can happen
        private bool safeNet;

        [Title("Tutorial Events")] [SerializeField]
        private UnityEvent onTutorialEnter;

        [SerializeField] private UnityEvent onTutorialLeave;
        private bool stopTrackingprogress;


        private void Awake()
        {
            //instance
            instance = this;
        }


        private void Start()
        {
            // Create new controls
            _controls = new PlayerControls();

            Debug.Log(_controls);

            //Input functions
            _controls.FishingMinigame.CircleMovement.performed += GetPlayerInput;
            _controls.FishingMinigame.CircleMovement.canceled += GetPlayerInput;
            _controls.FishingMinigame.ReelIn.performed += ctx => AttemptReelIn();
        }

        public void StartBiteTimer(FishData fish)
        {
            _controls.Enable();
            this.selectedFish = fish;

            //Give the biteTimer a random amount 
            biteTimer = Random.Range(minMaxTimeWait.x, minMaxTimeWait.y);

            //Set Player to occupied
            PlayerMovement.instance.SetState(State.Occupied);

            //FMOD
            FE_fishingSnapshot = RuntimeManager.CreateInstance("snapshot:/FishingMinigame");
            FE_fishingSnapshot.start();


            timerStarted = true;
        }


        #region Functions for the InputControls

        private void AttemptReelIn()
        {
            //Make it so if we are already fishing or 
            if (isFishing || DreamSequencer.instance.inDream) return;


            //If fish is attached to the bobble
            if (_canReelIn)
            {
                StartTheFishing();
                timerStarted = false;
                _canReelIn = false;

                FE_ReelSound = RuntimeManager.CreateInstance(reelSound);
                FE_ReelSound
                    .start(); //FMOD change - reel sound needs to be able to stop, so it cannot be a playoneshot function
            }
            //If we cancel at any point...just end the fishing
            else EndFishing();
        }

        void GetPlayerInput(InputAction.CallbackContext ctx)
        {
            movePos = ctx.ReadValue<Vector2>();
        }

        #endregion

        //TODO:: inject dream fishing


        #region Unity Functions

        private void Update()
        {
            //Before Fishing
            if (timerStarted)
            {
                BiteTimer();
            }

            if (_canReelIn)
            {
                CanReelInSequence();
            }

            if (!isFishing) return;

            Fishing();
        }

        private void FixedUpdate()
        {
            if (!isFishing) return;


            if (movePos == Vector2.zero) return;

            //Move the circle
            circleThing.MovePosition((circleThing.position + (movePos * (circleSpeed * Time.fixedDeltaTime))));
        }

        #endregion


        #region Fishing

        private void BiteTimer()
        {
            if (biteTimer > 0f)
            {
                biteTimer -= Time.deltaTime;
            }
            else
            {

                AudioManager.instance.PlayOneShot(rodReadyToReal);

                _timeTillFail = 1.5f;
                _canReelIn = true;
                timerStarted = false;
            }
        }

        private void CanReelInSequence()
        {
            interaction.IndicatorStatus(true);

            if (_timeTillFail > 0)
            {
                _timeTillFail -= Time.deltaTime;
            }
            else
            {
                //if we haven't completed the tutorial
                if (!hasDoneTutorial)
                    FailedTutorial();
                else
                {
                    interaction.IndicatorStatus(false);
                    _canReelIn = false;
                    _failedToReelIn = true;
                    RuntimeManager.PlayOneShot(failSound);
                    interaction.IndicatorStatus(false);
                    EndFishing();
                }
            }
        }

        void StartTheFishing()
        {
            interaction.IndicatorStatus(false);
            PlayerMovement.instance.SetAnimTrigger("reeling");
            UIManager.instance.OpenFishingMinigame();
            progressionBar.value = 0;
            safeNet = true;
            fishAI.OnInitialize(selectedFish);

            if (!hasDoneTutorial)
                BeginTutorial();

            isFishing = true;
        }

        void Fishing()
        {
            //Progression Bar

            //If we are on fish
            switch (onFish)
            {
                //Todo:: Remove hard coded values
                case true:
                 
                    progressionBar.value += 15f * Time.deltaTime;
                    break;
                case false:
                {
                    if (progressionBar.value != 0)
                    {
                        progressionBar.value -= 12f * Time.deltaTime;
                    }

                    break;
                }
            }

            //Safe Net
            if (progressionBar.value > 35f && safeNet)
            {
                safeNet = false;
            }

            //If we completed the Fishing Minigame
            if (progressionBar.value.Equals(progressionBar.maxValue))
            {
                //Close the game
                UIManager.instance.CloseFishingMinigame();

                //Stop sound
                StopSoundWithFadeout(FE_ReelSound);

                PlayerMovement.instance.SetAnimTrigger("wrestling");
                //If We haven't completed the tutorial
                if (!hasDoneTutorial)
                {
                    QTEManager.instance.StartAQuickTimeEvent(CompletedTutorial, FailedTutorial, .50f, 12f);
                    return;
                }

                //Else if we have
                QTEManager.instance.StartAQuickTimeEvent(() =>
                    {
                        if (!DreamSequencer.instance.inDream)
                            interaction.LaunchTheBobbleUp(selectedFish);
                        else
                        {
                            UIManager.instance.OpenDialogue();
                            UIManager.instance.dialogue.SetText("You", selectedFish.dialogue.dialog[0].text,
                                new Action(
                                    () => { DreamSequencer.instance.StopDreamSequence(); }));

                            EndFishing();
                            StopSoundWithFadeout(FE_ReelSoundMind);
                        }
                    },
                    () => { EndFishing(); },
                    .50f
                    , 12f);
            }

            //If we failed the fishing
            else if (!safeNet && progressionBar.value <= 0)
            {
                if (!hasDoneTutorial) FailedTutorial();
                else EndFishing();
            }
        }

        public void DisplayTheCaughtFish()
        {
            PlayerMovement.instance.SetAnimTrigger("doneFishing");

            _controls.Disable();
            var weight = Random.Range(selectedFish.minMaxWeight.x, selectedFish.minMaxWeight.y);

            Cursor.lockState = CursorLockMode.None;
            UIManager.instance.CaughtFishStatus(true);
            UIManager.instance.InjectTheFish(selectedFish, weight);
            interaction.ResetBobbleAndLine();

            switch (selectedFish.fishName)
            {
                case "Bass?":
                    PlayCollectibleSound("Bass");
                    break;
                case "Blahaj":
                    PlayCollectibleSound("Blahaj");
                    break;
                default:
                    AudioManager.instance.PlayOneShot(successSound);
                    break;
            }
        }

        public void EndFishing()
        {
            StopSoundWithFadeout(FE_ReelSound);
            StopSoundWithFadeout(FE_fishingSnapshot);
            _controls.Disable();
            isFishing = false;
            timerStarted = false;
            _canReelIn = false;
            _timeTillFail = 0;
            biteTimer = 0;
            Cursor.lockState = CursorLockMode.Locked;

            StartCoroutine(WaitForEndFrame());
        }

        #endregion


        #region Coroutines

        IEnumerator WaitForEndFrame()
        {
            yield return new WaitForSeconds(.2f);
            interaction.ResetBobbleAndLine();
            PlayerMovement.instance.SetState(State.CanWalk);
        }

        #endregion

        #region Helper Methods
        public void OnFish(bool value)
        {
            onFish = value;
        }
        #endregion
        
        
        #region Tutorial

        void BeginTutorial()
        {
            onTutorialEnter?.Invoke();
        }

        //If we failed the tutorial
        void FailedTutorial()
        {
            _controls.Disable();
            isFishing = false;
            timerStarted = false;
            _canReelIn = false;
            _timeTillFail = 0;
            biteTimer = 0;
            interaction.IndicatorStatus(false);
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(WaitForEndFrame());
        }

        void CompletedTutorial()
        {
            onTutorialLeave?.Invoke();
            hasDoneTutorial = true;
        }

        #endregion


        #region Sound

        private void PlayCollectibleSound(string collectibleName)
        {
            var eventInstance = RuntimeManager.CreateInstance("event:/Music/FE_Collectibles");
            eventInstance.setParameterByNameWithLabel("CollectibleType", collectibleName);
            eventInstance.start();
            eventInstance.release();
        }

        private void StopSoundWithFadeout(EventInstance instance)
        {
            PLAYBACK_STATE playbackState;
            instance.getPlaybackState(out playbackState);

            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
            }
        }

        #endregion
    }
}