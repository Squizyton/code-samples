using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Quick_Time_Events
{
    public class QTEManager : MonoBehaviour
    {
        private PlayerControls _controls;
        public static QTEManager instance;


        [SerializeField] private float requiredFillAmount;
        [SerializeField]private float currentFillAmount;

        private float _fillPerClick;
        private Action _onCompletion;
        private Action _onFailed;
        
        [SerializeField] private InputControl requiredKey;
       [SerializeField]private bool qteEnabled;
       [SerializeField]private float timer;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            instance = this;
        }


        private void Start()
        {
            _controls = new PlayerControls();

            _controls.QuickTimeEvent.Buttons.performed += CheckIfKeyPressWasCorrectKey;
            var controlsArray = _controls.QuickTimeEvent.Buttons.controls;
        }

        public void StartAQuickTimeEvent(Action completeAction, Action failedAction, float fillPerClick, float timerAmount)
        {
            _controls.Enable();
            _onCompletion = completeAction;
            _onFailed = failedAction;
            
            var controlsArray = _controls.QuickTimeEvent.Buttons.controls;
            requiredKey = controlsArray[Random.Range(0, controlsArray.Count)];
            timer = timerAmount;
            _fillPerClick = fillPerClick;

            qteEnabled = true;
            
            UIManager.instance.QuickTimeButton(requiredKey.name);
            UIManager.instance.QuickTimeUIStatus(true);
        }

        private void Update()
        {
            if (!qteEnabled) return;

            if (currentFillAmount >= requiredFillAmount)
            {
                OnComplete();
            }
            
            if (timer >= 0f)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                OnFail();
            }


            
        }

        public void CheckIfKeyPressWasCorrectKey(InputAction.CallbackContext ctx)
        {
            if (ctx.control.name == requiredKey.name)
            {
                currentFillAmount += _fillPerClick;
            }
        }

        void OnComplete()
        {
            qteEnabled = false;

            _onCompletion?.Invoke();

            CleanupCode();
        }


        void OnFail()
        {
            qteEnabled = false;

            _onFailed?.Invoke();
            CleanupCode();
        }


        void CleanupCode()
        {
            UIManager.instance.QuickTimeUIStatus(false);
            _onFailed = null;
            _onCompletion = null;
            currentFillAmount = 0;
           _controls.Disable();
        }
    }
}