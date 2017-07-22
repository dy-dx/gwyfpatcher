using System;
using UnityEngine;

namespace gwyfhelper
{
    public class GoFast
    {
        static volatile int hole;
        static volatile float hitForce;
        static volatile bool shouldResetToLastShot;
        static Vector3 preHitLocation;

        public GoFast()
        {
        }

        public static void Update(
            int _hole,
            float _hitForce,
            Vector3 _preHitLocation,
            Transform ballMovementTransform,
            Rigidbody rb,
            GameObject playerCamPivot,
            float currentVelocity,
            float minVelToHit,
            bool outOfBounds,
            bool onTrolley,
            float initialDrag,
            float sandDragToApply,
            float waterDragToApply
        )
        {
            bool shouldTeleport = false;
            shouldResetToLastShot = false;
            hole = _hole;
            hitForce = _hitForce;
            preHitLocation = _preHitLocation;

            if (Input.GetKeyUp("h"))
            {
                shouldResetToLastShot = true;
            }
            if (Input.GetKeyUp("j"))
            {
                if (hole > 1)
                {
                    hole--;
                    shouldTeleport = true;
                }
            }
            if (Input.GetKeyUp("k"))
            {
                if (hole < 18)
                {
                    hole++;
                    shouldTeleport = true;
                }
            }

            if (shouldTeleport)
            {
                var holeObject = GameObject.Find("SpawnHole" + hole);
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                ballMovementTransform.position = holeObject.transform.position;
                rb.isKinematic = false;

                ballMovementTransform.rotation = holeObject.transform.rotation;
                playerCamPivot.transform.rotation = holeObject.gameObject.transform.rotation;
                playerCamPivot.transform.Rotate(20f, 0f, 0f);
            }

            if (Input.GetKeyUp("m"))
            {
                playerCamPivot.transform.Rotate(0f, 10f, 0f);
            }
            if (Input.GetKeyUp("n"))
            {
                playerCamPivot.transform.Rotate(0f, -10f, 0f);
            }


            if (Input.GetKeyUp("u"))
            {
                hitForce = hitForce - 1000f;
            }
            if (Input.GetKeyUp("i"))
            {
                hitForce = hitForce + 1000f;
            }
            if (Input.GetKeyUp("o"))
            {
                Shoot(
                    rb,
                    ballMovementTransform,
                    outOfBounds,
                    onTrolley,
                    initialDrag,
                    sandDragToApply,
                    waterDragToApply
                );
            }
        }

        // copy pasted from BallMovement#Update
        public static void Shoot(
            Rigidbody rb,
            Transform ballMovementTransform,
            bool outOfBounds,
            bool onTrolley,
            float initialDrag,
            float sandDragToApply,
            float waterDragToApply
        )
        {
            GameObject hitPoint = GameObject.Find("HitPoint");
            Vector3 playerPosition = new Vector3(
                hitPoint.transform.position.x,
                ballMovementTransform.position.y,
                hitPoint.transform.position.z
            );
            rb.angularDrag = 1f;
            rb.drag = initialDrag + sandDragToApply + waterDragToApply;
            if (hitForce != 0f)
            {
                rb.AddForce(-(playerPosition - ballMovementTransform.position).normalized * (hitForce + 1f), ForceMode.Force);
            }
            if (!outOfBounds && !onTrolley)
            {
                preHitLocation = ballMovementTransform.position;
            }
            hitForce = 0f;
        }

        public static int GetNewHole()
        {
            return hole;
        }

        public static float GetNewHitForce()
        {
            return hitForce;
        }

        public static Vector3 GetNewPreHitLocation()
        {
            return preHitLocation;
        }

        public static bool ShouldResetToLastShot()
        {
            return shouldResetToLastShot;
        }
    }
}
