using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Fishing
{
    public class FishAI : MonoBehaviour
    {
        private float aggressiveness;
        private bool _isMoving;
        private float changeDirectionTimer;
        private Vector2 movePos;

        [SerializeField] private float speed;
        [SerializeField] private CompositeCollider2D bounds;

        private FishData _iAmThisFish;


        private Image spriteR;
        private float objectWidth;
        private float objectHeight;
        

        private Vector2 posToGoTo;

        public float rotationSpeed;

        private void Start()
        {
            spriteR = GetComponent<Image>();
            objectHeight = spriteR.sprite.bounds.size.y / 2;
            objectWidth = spriteR.sprite.bounds.size.x / 2;
        }

        public void OnInitialize(FishData fishData)
        {
            aggressiveness = fishData.aggressionValue;
            _iAmThisFish = fishData;
            StartMoving();
        }

        void StartMoving()
        {
            GenerateTimer();
            _isMoving = true;
        }

        void GenerateTimer()
        {
            changeDirectionTimer = Mathf.Clamp(Random.Range(5, 15) / aggressiveness, 0, 10);
            GenerateDirection();
        }

        void GenerateDirection()
        {
            speed = Random.Range(_iAmThisFish.minMaxSpeed.x, _iAmThisFish.minMaxSpeed.y);
            posToGoTo = GetRandomPositionInCircle(bounds);
        }


        private void Update()
        {
            if (!_isMoving) return;

            transform.position = Vector3.MoveTowards(transform.position, posToGoTo, Time.deltaTime * speed * 1.5f);


            var direction = posToGoTo - (Vector2) transform.position;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;


            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward),
                Time.deltaTime * rotationSpeed);


            if (Vector2.Distance(posToGoTo, (Vector2) transform.position) < .1f)
                GenerateDirection();


            if (!bounds.bounds.Contains(transform.position))
                GenerateDirection();

            if (changeDirectionTimer > 0f)
            {
                changeDirectionTimer -= Time.deltaTime;
            }
            else
            {
                GenerateTimer();
            }
        }


        Vector2 GetRandomPositionInCircle(Collider2D theBounds)
        {
            var colliderBounds = theBounds.bounds;


            var newPoint = new Vector2(Random.Range(colliderBounds.min.x + objectWidth, colliderBounds.max.x - objectWidth -1f),
                Random.Range(colliderBounds.min.y + objectHeight , colliderBounds.max.y - objectHeight - 1f));


            if (!theBounds.bounds.Contains(newPoint)) return transform.position;

            Debug.Log("contains point");
            return newPoint;
        }


        private void OnCollisionEnter2D(Collision2D col)
        {
            // Debug.Log("exited");
            // 
            // Vector2 surfaceNormal = col.contacts[0].normal;

            // movePos = Vector2.Reflect(movePos, surfaceNormal);  
        }
    }
}