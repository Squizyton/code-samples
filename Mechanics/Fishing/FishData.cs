using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Dialogue;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[CreateAssetMenu(fileName = "New Fish", menuName = "New Fish Data")]
public class FishData : ScriptableObject
{
   
   [Header("Values")]
   public string fishName;
   public Sprite fishIcon;
   public GameObject fishPrefab;
   public int fishID;

   [Header("Other Things")] [TextArea] public string fishDescription;
   
   [Header("Fish Features")] 
   [MinMaxSlider(0, 50, true)] public Vector2 minMaxWeight;
   [MinMaxSlider(0, 100,true)] public Vector2 minMaxSpeed;
   [MinMaxSlider(0, 10, true)] public Vector2 minMaxSize;

   [Header("Aggressiveness")]
   [Tooltip("Higher the number, the more the fish moves")]public float aggressionValue;

   
   
   
   [Header("Spawning")] public float beginningTime;
   public float endTime;



   [Header("Dream stuff")] public bool isPartOfAThought;
   [ShowIf("@isPartOfAThought")] public DialogueData dialogue;


}
