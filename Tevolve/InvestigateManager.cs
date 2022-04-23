using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Sirenix.OdinInspector;
using Tevolve.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
public enum ControlScheme
{
    Buttons,
    FreeMode
}

//Commit with the Button Movement!

//Item manipulation should be done in it's own script.
//Everything else should be handled by a manager

public class InvestigateManager : MonoBehaviourPun
{
    [Header("Is Investigating")] public bool isInvestigating;

    [Header("Investigate Renderer")] public Camera investigateCamera;
    public GameObject investigateCanvas;

    [Header("Layer Mask")] public LayerMask layer;

    ControlScheme controlScheme;

    public static InvestigateManager instance;

    [Header("Object Spawn Point")] public Transform spawnPoint;

    [Header("misc variables")] public Animator anim;
    public GameObject canvas;

    [Header("Zoom Slider ")] public Slider slider;
    // Start is called before the first frame update

    [Header("Unity Events")] public UnityEvent OnOpen;
    public UnityEvent OnClose;

    [Title("Reading Variables")]
    public TextMeshProUGUI readText;
    public GameObject readUI;
    public bool isReading;
    [SerializeField]private GameObject readButton;

    [Title("Pickup Button")] public GameObject pickupButton;
    
    
    [Title("Private Variables")]
    [SerializeField]private InvestigateableObject obj;
    private Vector3 resetCamera;
    private bool drag;
    private Vector3 origin;
    private Vector3 difference; 
    private float previousAngle;
    private Vector3 maxRotation;
    private bool limitRotation;
    private float rotSpeed = 2;
    //A duct tape way that needs to be re-done
    private bool onFirstClick;

    [SerializeField] private bool canInteract = false;

    void Awake()
    {
       
        instance = this;
        controlScheme = ControlScheme.FreeMode;
    }


    [SerializeField] private GameObject currentlyInvestigating;

    private Vector3 posLastFrame;
    private Transform resetPos;
    private GameObject caller;

    // Update is called once per frame
    private void Update()
    {
        
        if (!isInvestigating) return;

        //TODO This is a big big nono change this Joel when you think of something better. Just reference the circle slider or something
        if (InteractManager.IsPointerOverUIObject(slider.transform.GetChild(2).transform.GetChild(0).gameObject)) return;

        if (!onFirstClick)
        {
            var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(investigateCamera);
            onFirstClick = true;
        }

        Ray ray = investigateCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        var deltaPos = Input.mousePosition - posLastFrame;

        slider.onValueChanged.AddListener(delegate { UpdateScale(); });



        if (InteractManager.IsPointerOverUIObject(slider.gameObject))
        {
            Debug.Log("uh");
        }

        //Scroll wheel
        var mouseAxis = Input.GetAxis("Mouse ScrollWheel");
        if (mouseAxis > 0 || mouseAxis < 0)
        {
            slider.value += mouseAxis * .2f;
        }


        //Rotation
        if (Input.GetMouseButton(0) && !InteractManager.IsPointerOverUIObject(slider.gameObject))
        {
            origin = Input.mousePosition;
                
            float x = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;
                
            if (Math.Abs(x) > Math.Abs(rotY))
            {
                if (limitRotation)
                {
                    if (maxRotation.x != 0 && Mathf.Abs(currentlyInvestigating.transform.rotation.x) >= maxRotation.x)
                    {
                        currentlyInvestigating.transform.rotation = new Quaternion(maxRotation.x,
                            currentlyInvestigating.transform.rotation.y, 0f,0f);
                        return;
                    }
                }
                currentlyInvestigating.transform.RotateAround(Vector3.up, -x);
            } else if (Math.Abs(x) < Math.Abs(rotY))
            {
                if (limitRotation)
                {
                    if (maxRotation.y != 0 &&
                        Mathf.Abs(currentlyInvestigating.transform.rotation.y) >= maxRotation.y)
                    {
                        return;
                    }
                }

                currentlyInvestigating.transform.RotateAround(Vector3.right, rotY);
            }
        }
        posLastFrame = Input.mousePosition;
    }
    
    #region Stays In This Script

    public void StartInvestigation(InvestigateableObject obj, GameObject caller)
    {
        this.obj = obj;
        
        
        OnOpen?.Invoke();
        GameUIManager.Instance.HideUISubviewButtons = true;
        canvas.SetActive(true);
        investigateCanvas.SetActive(true);
        resetPos = spawnPoint.transform;
        currentlyInvestigating = Instantiate(obj.prefab, resetPos.position, obj.prefab.transform.rotation,
            resetPos);

        currentlyInvestigating.transform.tag = "Rotateable";
        currentlyInvestigating.layer = 22;

        if (obj.limitRotation)
        {
            limitRotation = obj.limitRotation;
            maxRotation = obj.maxRotation;
        }

        if (obj.hasANote)
        {
            readButton.SetActive(true);
        }

        if (obj.canPickup)
        {
            this.caller = caller;
            pickupButton.SetActive(true);
        }

        anim.SetTrigger("appear");
        currentlyInvestigating.transform.localPosition = new Vector3(0, 0, 0);
        isInvestigating = true;
    }

    public void Exit()
    {
        pickupButton.SetActive(false);
        readButton.SetActive(false);
        canvas.SetActive(false);
        investigateCanvas.SetActive(false);
        OnClose?.Invoke();
        GameUIManager.Instance.HideUISubviewButtons = false;
        isInvestigating = false;
        Reset();
        Destroy(currentlyInvestigating);
        currentlyInvestigating = null;
    }

    public void Reset()
    {
        if (currentlyInvestigating == null) return;
        
        currentlyInvestigating.transform.rotation = resetPos.rotation;
        currentlyInvestigating.transform.localPosition = new Vector3(0, 0, 0);
    }

    void UpdateScale()
    {
        if (currentlyInvestigating != null)
            currentlyInvestigating.transform.localScale = resetPos.transform.localScale * slider.value;
    }

    public void PickUpItem()
    {
        try
        {
            if (obj.callFunctionOnPickup)
            {
                //Sends a message to caller
                caller.SendMessage("Action");
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"There was an error....Ask a developer. They might know\n Error: {e} ");
        }
    }

    //TODO Make this get called automatically or something I don't know
    /// <summary>
    /// You must call this in the gameObject you are sending caller.SendMessage to you if you want to continue the pickup sequence.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="receiver"></param>
    public void ContinuePickup(bool value, GameObject receiver)
    {
        try
        {
            if (!value) return;
            
            var pickup = receiver.GetComponent<PickupObject>();

            pickup.Pickup();

            obj.item.Interact(currentlyInvestigating.transform.position, true);

            if (pickup.id != 0)
            {
                photonView.RPC("PunPickup", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.ActorNumber,
                    pickup.id);
            }

            Exit();
        }
        catch (Exception e)
        {
            Debug.LogError($"There was an error....Ask a developer. They might know\n Error: {e} ");
            throw;
        }
    }


    [PunRPC]
    public void PunPickup(int owner, int id)
    {
        var listofPU =Resources.FindObjectsOfTypeAll<PickupObject>();
        var correct = listofPU.FirstOrDefault(s => s.id == id);
        correct.LocalPickup(owner);
        
    }

    private void OnPointerEnter(PointerEventData eventData)
    {
        canInteract = true;
    }

    public void ReadNote()
    {
        isReading = true;
        readUI.SetActive(true);
        readText.SetText(obj.text);
    }

    public void ExitReading()
    {
        isReading = false;
        readText.SetText("");
        readUI.SetActive(false);
    }

    private void OnMouseExit()
    {
        canInteract = false;
    }

    //-----------------------------------

    #endregion
}