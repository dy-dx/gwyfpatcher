﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        // shitty button logic
        static bool shouldShoot;
        static bool shouldTeleport;
        static bool shouldGoToPreviousHole;
        static bool shouldGoToNextHole;

        // static bool cursorEnabled = true;
        static bool cursorEnabled;
        static bool helperInitialized;
        // don't need this anymore
        static bool menuUp;

        static GameObject _Script;
        static GameObject[] currentSceneGOArray;

        static bool guiSkipIntermissions = true;
        static bool guiUndoShotClicked;
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

        public static void LogObjects()
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
        }

        public static void SetCursorLock(bool enabled) {
            // from SteamInvites class
            // Menu.steamInviteUp = false seems to unlock the mouse cursor lol
            if (_Script == null && SceneManager.GetActiveScene().name != "MenuV2")
            {
                currentSceneGOArray = SceneManager.GetActiveScene().GetRootGameObjects();
                for (int l = 0; l < (int)currentSceneGOArray.Length; l++)
                {
                    if (currentSceneGOArray[l].name == "_Scripts")
                    {
                        _Script = currentSceneGOArray[l];
                    }
                }
            }

            if (_Script != null)
            {
                _Script.GetComponent<Menu>().steamInviteUp = enabled;
            }
        }

        public static void Update(
            int _hole,
            float _hitForce, // don't need this anymore
            Vector3 _preHitLocation, // don't need this anymore
            Transform _ballMovementTransform,
            Rigidbody rb,
            GameObject _playerCamPivot, // don't need this anymore
            float currentVelocity, // don't need this anymore
            float minVelToHit, // don't need this anymore
            bool outOfBounds,
            bool onTrolley,
            float initialDrag,
            float sandDragToApply, // don't need this anymore
            float waterDragToApply // don't need this anymore
        )
        {
            if (_Script == null && SceneManager.GetActiveScene().name != "MenuV2")
            {
                currentSceneGOArray = SceneManager.GetActiveScene().GetRootGameObjects();
                for (int l = 0; l < (int)currentSceneGOArray.Length; l++)
                {
                    if (currentSceneGOArray[l].name == "_Scripts")
                    {
                        _Script = currentSceneGOArray[l];
                    }
                }
            }
            if (_Script == null)
            {
                return;
            }

            helperInitialized = true;

            shouldResetToLastShot = false;

            hole = _hole;
            ballMovementTransform = _ballMovementTransform;
            playerCamPivot = _Script.GetComponent<Menu>().playerCamPivot;
            GameObject playerBall = _Script.GetComponent<Menu>().playerBall;
            BallMovement ballMovement = playerBall.GetComponent<BallMovement>();
            preHitLocation = ballMovement.preHitLocation;
            hitForce = ballMovement.hitForce;


            if (guiSkipIntermissions)
            {
                if (ballMovement.intermissionStarted && !ballMovement.startIntermission)
                {
                    ballMovement.intermissionStarted = false;
                    ballMovement.startIntermission = true;
                }
            }


            // if (Input.GetKeyUp("h"))
            if (guiUndoShotClicked)
            {
                shouldResetToLastShot = true;
            }
            guiUndoShotClicked = false;

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
                // rotationInput = "";

                preHitLocation = holeObject.transform.position;
            }
            shouldTeleport = false;

            if (Input.GetKeyUp("left shift") || Input.GetKeyUp("right shift"))
            {
                cursorEnabled = !cursorEnabled;
            }
            SetCursorLock(cursorEnabled);


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
                    ballMovement,
                    rb,
                    ballMovementTransform,
                    outOfBounds,
                    onTrolley,
                    initialDrag
                );
                shouldShoot = false;
            }

            float newYRotation;
            bool parsedRot = float.TryParse(rotationInput, out newYRotation);
            if (!String.IsNullOrEmpty(rotationInput) && parsedRot)
            {
                var angles = playerCamPivot.transform.rotation.eulerAngles;
                playerCamPivot.transform.eulerAngles = new Vector3(angles.x, newYRotation, angles.z);
            }

            float newHitForce;
            bool parsedHitForce = float.TryParse(hitForceInput, out newHitForce);
            if (!String.IsNullOrEmpty(hitForceInput) && parsedHitForce)
            {
                hitForce = newHitForce;
            }
        }

        // copy pasted from BallMovement#Update
        public static void Shoot(
            BallMovement ballMovement,
            Rigidbody rb,
            Transform ballMovementTransform,
            bool outOfBounds,
            bool onTrolley,
            float initialDrag
        )
        {
            GameObject hitPoint = GameObject.Find("HitPoint");
            Vector3 playerPosition = new Vector3(
                hitPoint.transform.position.x,
                ballMovementTransform.position.y,
                hitPoint.transform.position.z
            );
            rb.angularDrag = 1f;
            rb.drag = initialDrag + ballMovement.sandDragToApply + ballMovement.waterDragToApply;
            if (hitForce > 0f)
            {
                rb.AddForce(-(playerPosition - ballMovementTransform.position).normalized * (hitForce + 1f), ForceMode.Force);
                if (!onTrolley) {
                    ballMovement.hitCounter++;
                }
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

        public static void OnGUI()
        {
            if (!helperInitialized) return;

            const float w = 200;
            const float h = 200;
            const float textFieldW = 50;

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
                guiUndoShotClicked = true;
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

            GUILayout.BeginHorizontal();
            guiSkipIntermissions = GUILayout.Toggle(guiSkipIntermissions, "Skip Intermissions");
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        // don't need these anymore
        public static void LateUpdate()
        {
            return;
        }
        public static void MenuLateUpdate()
        {
            return;
        }
        public static void MenuPreUpdate(bool _menuUp)
        {
            menuUp = _menuUp;
        }
        public static bool GetNewMenuUp()
        {
            return menuUp;
        }
    }
}
