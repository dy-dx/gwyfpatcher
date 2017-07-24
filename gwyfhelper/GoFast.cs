﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace gwyfhelper
{
    public class GoFast
    {
        static int hole;
        static float hitForce;
        static Vector3 preHitLocation;
        static Transform ballMovementTransform;
        static GameObject playerCamPivot;

        static bool shouldResetToLastShot;
        static bool _shouldResetToLastShot;

        // shitty button logic
        static bool shouldShoot;
        static bool shouldTeleport;
        static bool shouldGoToPreviousHole;
        static bool shouldGoToNextHole;

        static bool cursorEnabled;
        static bool helperInitialized;
        static bool menuUp;

        static GameObject IngameMenu;

        static string hitForceInput = "";
        static string rotationInput = "";

        public GoFast()
        {
        }

        // this doesn't work yet
        public static void LogToScreen(string msg)
        {
            // this.menuSystem.transform.Find("Spectating Text").gameObject.SetActive(true);
            // this.menuSystem.transform.Find("Spectating Text").gameObject.GetComponent<Text>().text = "asdf";

            // this.menuSystem.transform.Find("StrokeText").gameObject.SetActive(false);
            // this.menuSystem.transform.Find("PowerOverlay").gameObject.SetActive(false);
            // this.menuSystem.transform.Find("TimeText").gameObject.SetActive(false);
            // this.menuSystem.transform.Find("EndGameTimer").gameObject.SetActive(true);
            // this.menuSystem.transform.Find("EndGameTimer").GetComponent<Text>().text = "Returning to lobby in " + this.levelSelectIntermittion + " seconds";
            MonoBehaviour.print(msg);
        }

        public static void MenuLateUpdate()
        {
            if (cursorEnabled)
            {
                HideMenu();
            }
        }

        public static void HideMenu()
        {
            if (IngameMenu == null)
            {
                IngameMenu = GameObject.Find("IngameMenu");
            }
            if (IngameMenu != null)
            {
                IngameMenu.SetActive(false);
            }
        }

        public static void ToggleCursor()
        {
            // MonoBehaviour[] allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            // foreach(MonoBehaviour obj in allObjects)
            // {
            //     // Debug.LogError(obj+" is an active object");
            //     Debug.Log(obj+" is an active object");
            // }

            // GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            // foreach(GameObject obj in allObjects)
            // {
            //     Debug.Log(obj+" is an active object with tag: " + obj.tag);
            // }

            cursorEnabled = !cursorEnabled;
        }

        public static void EnableCursor()
        {
            GameObject hitPoint = GameObject.Find("HitPoint");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (hitPoint != null)
            {
                hitPoint.SetActive(false);
            }
            // if (this.playerCamPivot != null)
            // {
            //     this.playerCamPivot.GetComponent<MouseAim>().enabled = false;
            // }
            // if (this.playerBall != null)
            // {
            //     this.playerBall.GetComponent<BallMovement>().menuUp = true;
            // }
        }

        public static void DisableCursor()
        {
            GameObject hitPoint = GameObject.Find("HitPoint");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (hitPoint != null)
            {
                hitPoint.SetActive(true);
            }
            // if (this.playerCamPivot != null)
            // {
            //     this.playerCamPivot.GetComponent<MouseAim>().enabled = true;
            // }
            // if (this.playerBall != null)
            // {
            //     this.playerBall.GetComponent<BallMovement>().menuUp = false;
            // }
        }

        public static void Update(
            int _hole,
            float _hitForce,
            Vector3 _preHitLocation,
            Transform _ballMovementTransform,
            Rigidbody rb,
            GameObject _playerCamPivot,
            float currentVelocity,
            float minVelToHit,
            bool outOfBounds,
            bool onTrolley,
            float initialDrag,
            float sandDragToApply,
            float waterDragToApply
        )
        {
            helperInitialized = true;
            
            shouldResetToLastShot = false;
            hole = _hole;
            hitForce = _hitForce;
            preHitLocation = _preHitLocation;
            ballMovementTransform = _ballMovementTransform;
            playerCamPivot = _playerCamPivot;

            // if (Input.GetKeyUp("h"))
            // hopefully this still makes sense in the morning
            if (_shouldResetToLastShot)
            {
                shouldResetToLastShot = true;
            }
            _shouldResetToLastShot = false;

            // if (Input.GetKeyUp("j"))
            if (shouldGoToPreviousHole)
            {
                if (hole > 1)
                {
                    hole--;
                }
                shouldTeleport = true;
            }
            shouldGoToPreviousHole = false;

            // if (Input.GetKeyUp("k"))
            if (shouldGoToNextHole)
            {
                if (hole < 18)
                {
                    hole++;

                }
                shouldTeleport = true;
            }
            shouldGoToNextHole = false;

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
                rotationInput = "";
            }
            shouldTeleport = false;

            if (Input.GetKeyUp("escape"))
            {
                ToggleCursor();
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
                shouldShoot = true;
            }

            if (shouldShoot) {
                Shoot(
                    rb,
                    ballMovementTransform,
                    outOfBounds,
                    onTrolley,
                    initialDrag,
                    sandDragToApply,
                    waterDragToApply
                );
                shouldShoot = false;
            }

            float newYRotation;
            bool parsedRot = float.TryParse(rotationInput, out newYRotation);
            if (!String.IsNullOrEmpty(rotationInput) && parsedRot)
            {
                var angles = playerCamPivot.transform.rotation.eulerAngles;
                playerCamPivot.transform.eulerAngles = new Vector3(
                    angles.x, newYRotation, angles.z);
            }

            float newHitForce;
            bool parsedHitForce = float.TryParse(hitForceInput, out newHitForce);
            if (!String.IsNullOrEmpty(hitForceInput) && parsedHitForce)
            {
                hitForce = newHitForce;
            }
        }

        public static void LateUpdate()
        {
            // don't need this anymore
            // if (cursorEnabled)
            // {
            //     Cursor.lockState = CursorLockMode.None;
            // }
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
            if (hitForce > 0f)
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

        public static void MenuPreUpdate(bool _menuUp)
        {
            menuUp = _menuUp;

            if (cursorEnabled) {
                menuUp = true;
            } 
        }

        public static bool GetNewMenuUp()
        {
            return menuUp;
        }

        public static void OnGUI()
        {
            if (!helperInitialized) return;

            const float w = 200;
            const float h = 200;
            // const float x = 10;
            const float textFieldW = 50;
            // const float textFieldH = 20;

            GUILayout.BeginArea(new Rect (5, 5, w, h), GUI.skin.box);
            GUILayout.Label("t1gerw00dz 1337 hax menu v0.1");

            GUILayout.BeginHorizontal();
            hitForceInput = GUILayout.TextField(hitForceInput, 10, GUILayout.Width(textFieldW));
            GUILayout.Label("hitForce: " + hitForce);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rotationInput = GUILayout.TextField(rotationInput, 10, GUILayout.Width(textFieldW));
            GUILayout.Label("degrees: " + playerCamPivot.transform.rotation.eulerAngles.y);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Shoot"))
            {
                shouldShoot = true;
            }
            if (GUILayout.Button("Undo shot"))
            {
                _shouldResetToLastShot = true;
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Reset hole"))
            {
                shouldTeleport = true;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous hole"))
            {
                shouldGoToPreviousHole = true;
            }
            if (GUILayout.Button("Next hole"))
            {
                shouldGoToNextHole = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            // GUI.Box(new Rect(5, 5, w, h), "t1gerw00dz 1337 hax menu v0.1");

            // GUI.Label(new Rect(x, 30, w, h), "hitForce: " + hitForce.ToString());
            // hitForceInput = GUI.TextField(new Rect(x, 50, textFieldW, textFieldH), hitForceInput);

            // GUI.Label(new Rect(x, 70, w, h), "degrees: " + playerCamPivot.transform.rotation.eulerAngles.y.ToString());
            // rotationInput = GUI.TextField(new Rect(x, 90, textFieldW, textFieldH), rotationInput);

            // if (GUI.Button(new Rect(x, 110, 80, 20), "Shoot"))
            // {
            //     shouldShoot = true;
            // }
        }
    }
}
