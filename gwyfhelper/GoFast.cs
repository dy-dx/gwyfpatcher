using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

namespace gwyfhelper
{
    public class GoFast
    {
        // null these out on onDestroy
        public static BallMovement ballMovement;
        public static Rigidbody rb;
        public static GameObject playerCamPivot;
        public static GameObject hitPoint;
        public static GameObject _Script;
        public static GameObject objectMouseIsOver;

        public static Vector3 prevHitPointPosition;
        public static float hitPointDistanceMoved;

        public static int prevHole;
        public static bool cursorEnabled;
        public static float timeSinceNewHole;

        public static bool _shouldShoot;
        public static bool _shouldShootOnNextFrame;

        public static bool enableShootDelayTimer;
        public static float shootDelayTimeout;

        public static bool isTrackingBallMovementTime;
        public static float ballMovementTime;

        public static Transform obstacleTransform;
        public static float obstacleMovementAmount;
        public static Transform secondObstacleTransform;
        public static float secondObstacleMovementAmount;

        public static void Shoot()
        {
            if (enableShootDelayTimer)
            {
                return;
            }
            _shouldShoot = true; // handle on current/upcoming update
        }
        public static void ShootOnNextFrame()
        {
            _shouldShootOnNextFrame = true; // handle on next update
        }
        public static void ShootWithDelay(float s)
        {
            if (enableShootDelayTimer)
            {
                return;
            }
            enableShootDelayTimer = true;
            shootDelayTimeout = s;
        }

        private static void _Shoot()
        {
            if (ballMovement.hitForce <= 0f)
            {
                return;
            }
            // assume we successfully shot and we can start tracking movement time
            // even though that may not be true
            ballMovementTime = 0f;
            isTrackingBallMovementTime = true;


            hitPoint.SetActive(true);
            ballMovement.menuUp = false;

            // make it so cInput.GetKeyUp("Shoot") returns true
            int hash = "Shoot".GetHashCode();
            var inputNameHash = Util.GetStaticField<Dictionary<int, int>>(typeof(cInput), "_inputNameHash");
            int num = inputNameHash[hash];
            var getKeyUpArray = Util.GetStaticField<bool[]>(typeof(cInput), "_getKeyUpArray");
            getKeyUpArray[num] = true;
        }

        public static void ResetToSpawn()
        {
            var holeObject = GameObject.Find("SpawnHole" + GetHole());
            ResetToPosition(holeObject.transform.position);

            // ballMovement.transform.rotation = holeObject.transform.rotation;
            // playerCamPivot.transform.rotation = holeObject.gameObject.transform.rotation;
            // playerCamPivot.transform.Rotate(20f, 0f, 0f);

            ballMovement.preHitLocation = holeObject.transform.position;
            ballMovement.hitCounter = 0;
            ballMovement.startIntermission = false;
        }

        public static void ResetToPosition(Vector3 pos)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            ballMovement.transform.position = pos;
            rb.isKinematic = false;
        }

        public static void SetCursorLock(bool enabled)
        {
            // Menu.steamInviteUp = false seems to unlock the mouse cursor lol
            if (_Script != null)
            {
                _Script.GetComponent<Menu>().steamInviteUp = enabled;
            }
        }

        public static StaticRotator GetClosestRotator(string name)
        {
            StaticRotator[] allRotators = UnityEngine.Object.FindObjectsOfType<StaticRotator>();
            StaticRotator[] rotators = Array.FindAll(allRotators, r => r.name.StartsWith(name));
            float lowestDist = Single.PositiveInfinity;
            StaticRotator closest = null;
            foreach(var obj in rotators)
            {
                float dist = Vector3.Distance(obj.transform.position, ballMovement.transform.position);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    closest = obj;
                }
            }
            return closest;
        }

        public static PingPongMotion GetClosestPingPongMotion(string name)
        {
            PingPongMotion[] allObjs = UnityEngine.Object.FindObjectsOfType<PingPongMotion>();
            PingPongMotion[] objs = Array.FindAll(allObjs, o => o.name.StartsWith(name));
            float lowestDist = Single.PositiveInfinity;
            PingPongMotion closest = null;
            foreach(var obj in objs)
            {
                float dist = Vector3.Distance(obj.transform.position, ballMovement.transform.position);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    closest = obj;
                }
            }
            return closest;
        }

        public static GameObject GetNewestLog()
        {
            DeleteAfterSeconds[] allObjs = UnityEngine.Object.FindObjectsOfType<DeleteAfterSeconds>();
            DeleteAfterSeconds[] objs = Array.FindAll(allObjs, r => r.name.StartsWith("Animated_Log"));
            float lowestZ = Single.PositiveInfinity;
            GameObject newestObj = null;
            foreach(var obj in objs)
            {
                float z = obj.transform.position.z;
                if (z < lowestZ)
                {
                    lowestZ = z;
                    newestObj = obj.gameObject;
                }
            }
            return newestObj;
        }

        public static int GetHole()
        {
            // this doesn't work anymore, since private field names got obfuscated
            // return GetInstanceField<int>(typeof(BallMovement), ballMovement, "hole");
            Hashtable playerCustomProps = PhotonNetwork.player.CustomProperties;
            return (int)playerCustomProps["holeNumber"];
        }
        public static void SetHole(int val)
        {
            // this doesn't work anymore, since private field names got obfuscated
            // SetInstanceField<int>(typeof(BallMovement), ballMovement, "hole", val);
            Hashtable playerCustomProps = PhotonNetwork.player.CustomProperties;
            playerCustomProps["holeNumber"] = val;
            ballMovement.currentHoleNumber = val;
        }
        public static bool OnNewHole()
        {
            return GetHole() != prevHole;
        }
        public static void SetHitForce(float hitForce)
        {
            ballMovement.hitForce = hitForce;
        }
        public static void SetDegrees(float degrees)
        {
            var angles = playerCamPivot.transform.rotation.eulerAngles;
            playerCamPivot.transform.eulerAngles = new Vector3(angles.x, degrees, angles.z);
        }
        public static bool CanShoot()
        {
            return ballMovement != null &&
                !ballMovement.inHole &&
                !ballMovement.intermissionStarted &&
                !ballMovement.startIntermission &&
                (ballMovement.currentVelocity < ballMovement.minVelToHit);
        }
        public static string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public static void PreUpdate()
        {
            // from SteamInvites class
            if (_Script == null && GetActiveSceneName() != "MenuV2")
            {
                GameObject[] currentSceneGOArray = SceneManager.GetActiveScene().GetRootGameObjects();
                for (int l = 0; l < (int)currentSceneGOArray.Length; l++)
                {
                    if (currentSceneGOArray[l].name == "_Scripts")
                    {
                        _Script = currentSceneGOArray[l];
                    }
                }
                playerCamPivot = _Script.GetComponent<Menu>().playerCamPivot;
                GameObject playerBall = _Script.GetComponent<Menu>().playerBall;
                rb = playerBall.GetComponent<Rigidbody>();
                ballMovement = playerBall.GetComponent<BallMovement>();
                hitPoint = GameObject.Find("HitPoint");
            }
            if (_Script == null)
            {
                return;
            }

            if (prevHitPointPosition != null)
            {
                hitPointDistanceMoved = Vector3.Distance(prevHitPointPosition, hitPoint.transform.position);
            }
            prevHitPointPosition = hitPoint.transform.position;


            if (OnNewHole()) {
                timeSinceNewHole = 0f;
            } else {
                timeSinceNewHole += Time.deltaTime;
            }

            if (HelperGui.lockHole && prevHole != GetHole())
            {
                SetHole(prevHole);
                ResetToSpawn();
            }

            int hole = GetHole();

            if (GetActiveSceneName() == "ForestLevel") {
                if (hole == 4)
                {
                    if (obstacleTransform == null || obstacleTransform.gameObject.name != "Large_Spinner_1")
                    {
                        obstacleTransform = GetClosestRotator("Large_Spinner").transform;
                    }
                    obstacleMovementAmount = obstacleTransform.eulerAngles.z % 180;
                }
                else if (hole == 8)
                {
                    if (obstacleTransform == null || obstacleTransform.gameObject.name != "Misc_smallSpinner")
                    {
                        obstacleTransform = GetClosestRotator("Misc_smallSpinner").transform;
                    }
                    // a hack to make the euler angles get calculated correctly
                    obstacleTransform.eulerAngles = new Vector3(270, obstacleTransform.eulerAngles.y, obstacleTransform.eulerAngles.z);
                    obstacleMovementAmount = obstacleTransform.eulerAngles.y % 180;
                }
                else if (hole == 9)
                {
                    if (obstacleTransform == null || obstacleTransform.gameObject.name != "Windmill_Blades")
                    {
                        obstacleTransform = GetClosestRotator("Windmill_Blades").transform;
                    }
                    // idk
                    float pitchAngle = Vector3.Angle(new Vector3(obstacleTransform.forward.x, 0, obstacleTransform.forward.z), obstacleTransform.forward);
                    obstacleMovementAmount = pitchAngle;
                }
                else if (hole == 10)
                {
                    if (obstacleTransform == null || obstacleTransform.gameObject.name != "Large_Spinner_4")
                    {
                        obstacleTransform = GameObject.Find("Large_Spinner_4").transform;
                    }
                    if (secondObstacleTransform == null || secondObstacleTransform.gameObject.name != "Large_Spinner_3")
                    {
                        secondObstacleTransform = GameObject.Find("Large_Spinner_3").transform;
                    }
                    obstacleMovementAmount = obstacleTransform.eulerAngles.y % 180;
                    secondObstacleMovementAmount = secondObstacleTransform.eulerAngles.y % 180;
                }
                else if (hole == 16)
                {
                    if (obstacleTransform == null || !obstacleTransform.gameObject.name.StartsWith("Misc_Divider"))
                    {
                        obstacleTransform = GetClosestPingPongMotion("Misc_Divider").transform;
                    }
                    obstacleMovementAmount = obstacleTransform.position.z;
                }
                else if (hole == 18)
                {
                    obstacleTransform = GetNewestLog().transform;
                    obstacleMovementAmount = obstacleTransform.position.z;
                }
                else
                {
                    obstacleTransform = null;
                }

                if (hole != 10)
                {
                    secondObstacleTransform = null;
                }

                if (obstacleTransform != null && Input.GetKeyDown("o"))
                {
                    var pos = obstacleTransform.position;
                    if (pos.y < 5f)
                    {
                        pos.y += 5f;
                    }
                    else
                    {
                        pos.y -= 5f;
                    }
                    obstacleTransform.position = pos;
                }

                if (obstacleTransform != null && HelperGui.obstacleMovementShootToggle) {
                    float obstacleMovementMin;
                    float obstacleMovementMax;
                    bool isParsed = float.TryParse(HelperGui.obstacleMovementInput, out obstacleMovementMin);
                    bool isParsed2 = float.TryParse(HelperGui.obstacleMovementInput2, out obstacleMovementMax);

                    bool isShotClear = (
                        isParsed &&
                        isParsed2 &&
                        !String.IsNullOrEmpty(HelperGui.obstacleMovementInput) &&
                        !String.IsNullOrEmpty(HelperGui.obstacleMovementInput2) &&
                        obstacleMovementAmount >= obstacleMovementMin &&
                        obstacleMovementAmount <= obstacleMovementMax
                    );

                    if (isShotClear && secondObstacleTransform != null) {
                        float secondObstacleMovementMin;
                        float secondObstacleMovementMax;
                        bool secondIsParsed = float.TryParse(HelperGui.secondObstacleMovementInput, out secondObstacleMovementMin);
                        bool secondIsParsed2 = float.TryParse(HelperGui.secondObstacleMovementInput2, out secondObstacleMovementMax);
                        isShotClear = (
                            secondIsParsed &&
                            secondIsParsed2 &&
                            !String.IsNullOrEmpty(HelperGui.secondObstacleMovementInput) &&
                            !String.IsNullOrEmpty(HelperGui.secondObstacleMovementInput2) &&
                            secondObstacleMovementAmount >= secondObstacleMovementMin &&
                            secondObstacleMovementAmount <= secondObstacleMovementMax
                        );
                    }

                    if (isShotClear)
                    {
                        Shoot();
                        HelperGui.obstacleMovementShootToggle = false;
                    }
                }
            }

            if (HelperGui.toggleShowExtraInfo)
            {
                objectMouseIsOver = Util.GetGameObjectCursorIsOver();
            }

            JS.Update();


            if (Input.GetKeyDown("h")) {
              HelperGui.enabled = !HelperGui.enabled;
            }

            if (isTrackingBallMovementTime)
            {
                if (ballMovement.currentVelocity > ballMovement.minVelToHit || ballMovementTime < 0.5f)
                {
                    ballMovementTime += Time.deltaTime;
                }

                if (ballMovement.inHole)
                {
                    isTrackingBallMovementTime = false;
                }
            }

            if (HelperGui.skipIntermissions)
            {
                if (ballMovement.intermissionStarted && !ballMovement.startIntermission)
                {
                    ballMovement.intermissionStarted = false;
                    ballMovement.startIntermission = true;
                }
            }

            if (enableShootDelayTimer)
            {
                shootDelayTimeout -= Time.deltaTime;
                if (shootDelayTimeout <= 0f)
                {
                    enableShootDelayTimer = false;
                    Shoot();
                }
            }

            if (HelperGui.resetShotClicked)
            {
                ResetToPosition(ballMovement.preHitLocation);
                HelperGui.resetShotClicked = false;
            }

            if (HelperGui.retryShotClicked)
            {
                ResetToPosition(ballMovement.preHitLocation);
                ShootWithDelay(0.4f);
                HelperGui.retryShotClicked = false;
            }

            if (HelperGui.skipToPreviousHoleClicked)
            {
                if (hole > 1)
                {
                    SetHole(hole - 1);
                }
                ResetToSpawn();
                HelperGui.skipToPreviousHoleClicked = false;
            }

            if (HelperGui.skipToNextHoleClicked)
            {
                if (hole < 18)
                {
                    SetHole(hole + 1);
                }
                ResetToSpawn();
                HelperGui.skipToNextHoleClicked = false;
            }

            if (HelperGui.resetHoleClicked)
            {
                ResetToSpawn();
                HelperGui.resetHoleClicked = false;
            }

            if (Input.GetKeyUp("left shift") || Input.GetKeyUp("right shift"))
            {
                cursorEnabled = !cursorEnabled;
            }
            if (!HelperGui.enabled)
            {
                cursorEnabled = false;
            }
            SetCursorLock(cursorEnabled);


            if (HelperGui.useDegreesSlider)
            {
                SetDegrees(HelperGui.degreesSliderInput);
            }
            else
            {
                float newYRotation;
                bool parsedRot = float.TryParse(HelperGui.rotationInput, out newYRotation);
                if (!String.IsNullOrEmpty(HelperGui.rotationInput) && parsedRot)
                {
                    SetDegrees(newYRotation);
                }
            }

            if (HelperGui.useHitForceSlider)
            {
                SetHitForce(HelperGui.hitForceSliderInput);
            }
            else
            {
                float newHitForce;
                bool parsedHitForce = float.TryParse(HelperGui.hitForceInput, out newHitForce);
                if (!String.IsNullOrEmpty(HelperGui.hitForceInput) && parsedHitForce && !HelperGui.useHitForceSlider)
                {
                    SetHitForce(newHitForce);
                }
            }


            if (_shouldShoot)
            {
                _Shoot();
                _shouldShoot = false;
            }
            if (_shouldShootOnNextFrame)
            {
                _shouldShoot = true;
                _shouldShootOnNextFrame = false;
            }

            prevHole = GetHole();
        }

        public static void OnGUI()
        {
            if (ballMovement != null)
            {
                HelperGui.OnGUI();
            }
        }

        public static void PreStart()
        {
            Console.WriteLine("PreStart called");
            cursorEnabled = false;
            prevHole = 1;
            timeSinceNewHole = 0f;
            isTrackingBallMovementTime = false;
            ballMovementTime = 0f;
            enableShootDelayTimer = false;
            shootDelayTimeout = 0f;

            return;
        }
        public static void OnDestroy()
        {
            Console.WriteLine("OnDestroy called");
            ballMovement = null;
            rb = null;
            playerCamPivot = null;
            hitPoint = null;
            _Script = null;
            objectMouseIsOver = null;

            return;
        }
        // don't currently need these
        public static void PreFixedUpdate()
        {
            return;
        }
        public static void LateUpdate()
        {
            return;
        }
        // just using this for testing
        public static void Entry()
        {
            JS.Initialize();
            return;
        }
    }
}
