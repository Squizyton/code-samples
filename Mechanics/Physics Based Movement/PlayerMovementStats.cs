using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Movement
{
    [CreateAssetMenu(fileName = "New Player Movement Values",menuName = "Player Movement Control")]
    public class PlayerMovementStats : ScriptableObject
    {
        [Title("Extra Gravity")] public float extraGravity;
        
        [Title("movement Variables")]
        public float moveSpeed;

        public float sprintScale = 1;

        public float maxSpeed = 20;
        
        public float counterMovement = 0.175f;

        public float threshold;
        
        [Title("Ground")] public LayerMask whatIsGround;

        [Title("Jumping")] 
        public float jumpCooldown = 0.25f;

        public float jumpForce;


        [Title("Grappling Hook")] 
        public float jointSpring = 4.5f;
        public float jointDamper = 7f;
        public float jointMassScale = 4.5f;

        [Title("Sliding")] public float maxSlopeAngle = 35f;
        
    }
}
