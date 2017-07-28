using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

namespace gwyfhelper
{
    public class GoFast
    {
        // null these out on onDestroy
        static BallMovement ballMovement;
        static Transform ballMovementTransform;
        static Rigidbody rb;
        static GameObject playerCamPivot;
        static GameObject _Script;

        static int prevHole;
        static bool cursorEnabled;

        // shitty button logic
        static bool shouldShoot;
        static bool guiResetHoleClicked;
        static bool shouldGoToPreviousHole;
        static bool shouldGoToNextHole;
        static bool guiSkipIntermissions = true;
        static bool guiLockHole;
        static bool guiResetShotClicked;
        static bool guiRetryShotClicked;
        static bool guiUseHitForceSlider;
        static bool guiUseDegreesSlider;
        static float guiHitForceSliderInput;
        static float guiDegreesSliderInput;
        static string hitForceInput = "";
        static string rotationInput = "";

        static bool enableShootTimer;
        static float timeUntilShouldShoot;

        static bool isTrackingBallMovementTime;
        static float ballMovementTime;

        static Transform obstacleTransform;
        static float obstacleRotationDegrees;
        static bool guiObstacleRotationShootToggle;
        static string guiObstacleRotationInput = "";

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
            //     Debug.Log(obj+" is an active object");
            // }

            // GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            // foreach(GameObject obj in allObjects)
            // {
            //     Debug.Log(obj+" is an active object with tag: " + obj.tag);
            // }
        }

        public static void SetCursorLock(bool enabled) {
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
                float dist = Vector3.Distance(obj.transform.position, ballMovementTransform.position);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    closest = obj;
                }
            }
            return closest;
        }

        static T GetInstanceField<T>(Type type, object instance, string fieldName)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)type.GetField(fieldName, flags).GetValue(instance);
        }
        static void SetInstanceField<T>(Type type, object instance, string fieldName, T value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            type.GetField(fieldName, flags).SetValue(instance, value);
        }

        static int GetHole()
        {
            return GetInstanceField<int>(typeof(BallMovement), ballMovement, "hole");
        }
        static void SetHole(int val)
        {
            SetInstanceField<int>(typeof(BallMovement), ballMovement, "hole", val);
            var playerCustomProps = GetInstanceField<Hashtable>(typeof(BallMovement), ballMovement, "playerCustomProps");
            playerCustomProps["holeNumber"] = val;
            ballMovement.currentHoleNumber = val;
        }

        public static void PreUpdate()
        {
            // from SteamInvites class
            if (_Script == null && SceneManager.GetActiveScene().name != "MenuV2")
            {
                GameObject[] currentSceneGOArray = SceneManager.GetActiveScene().GetRootGameObjects();
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

            playerCamPivot = _Script.GetComponent<Menu>().playerCamPivot;
            GameObject playerBall = _Script.GetComponent<Menu>().playerBall;
            rb = playerBall.GetComponent<Rigidbody>();
            ballMovement = playerBall.GetComponent<BallMovement>();
            ballMovementTransform = ballMovement.transform;

            if (guiLockHole)
            {
                if (prevHole != GetHole()) {
                    SetHole(prevHole);
                    ResetToSpawn();
                }
            }

            int hole = GetHole();

            if (SceneManager.GetActiveScene().name == "ForestLevel") {
                if (hole == 4)
                {
                    obstacleTransform = GetClosestRotator("Large_Spinner").transform;
                    obstacleRotationDegrees = obstacleTransform.eulerAngles.z % 180;
                }
                else if (hole == 8)
                {
                    obstacleTransform = GetClosestRotator("Misc_smallSpinner").transform;
                    // a hack to make the euler angles get calculated correctly
                    obstacleTransform.eulerAngles = new Vector3(270, obstacleTransform.eulerAngles.y, obstacleTransform.eulerAngles.z);
                    obstacleRotationDegrees = obstacleTransform.eulerAngles.y % 180;
                }
                else if (hole == 9)
                {
                    obstacleTransform = GetClosestRotator("Windmill_Blades").transform;
                    // idk
                    float pitchAngle = Vector3.Angle(new Vector3(obstacleTransform.forward.x, 0, obstacleTransform.forward.z), obstacleTransform.forward);
                    obstacleRotationDegrees = pitchAngle;
                }
                else if (hole == 10)
                {
                    obstacleTransform = GetClosestRotator("Large_Spinner").transform;
                    obstacleRotationDegrees = obstacleTransform.eulerAngles.y % 180;
                }
                else
                {
                    obstacleTransform = null;
                }

                if (obstacleTransform != null && guiObstacleRotationShootToggle) {
                    float obstacleRotInput;
                    bool parsedRot = float.TryParse(guiObstacleRotationInput, out obstacleRotInput);
                    if (!String.IsNullOrEmpty(guiObstacleRotationInput) && parsedRot)
                    {
                        if (Math.Abs(obstacleRotationDegrees - obstacleRotInput) < 2f)
                        {
                            shouldShoot = true;
                            guiObstacleRotationShootToggle = false;
                        }
                    }
                }
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

            if (guiSkipIntermissions)
            {
                if (ballMovement.intermissionStarted && !ballMovement.startIntermission)
                {
                    ballMovement.intermissionStarted = false;
                    ballMovement.startIntermission = true;
                }
            }

            if (enableShootTimer)
            {
                timeUntilShouldShoot -= Time.deltaTime;
                if (timeUntilShouldShoot < 0f)
                {
                    shouldShoot = true;
                    enableShootTimer = false;
                }
            }

            if (guiResetShotClicked)
            {
                ResetToPosition(ballMovement.preHitLocation);
                guiResetShotClicked = false;
            }

            if (guiRetryShotClicked)
            {
                ResetToPosition(ballMovement.preHitLocation);
                enableShootTimer = true;
                timeUntilShouldShoot = 0.4f;
                guiRetryShotClicked = false;
            }

            // if (Input.GetKeyUp("j"))
            if (shouldGoToPreviousHole)
            {
                if (hole > 1)
                {
                    SetHole(hole - 1);
                }
                ResetToSpawn();
                shouldGoToPreviousHole = false;
            }

            // if (Input.GetKeyUp("k"))
            if (shouldGoToNextHole)
            {
                if (hole < 18)
                {
                    SetHole(hole + 1);

                }
                ResetToSpawn();
                shouldGoToNextHole = false;
            }

            if (guiResetHoleClicked)
            {
                ResetToSpawn();
                guiResetHoleClicked = false;
            }

            if (Input.GetKeyUp("left shift") || Input.GetKeyUp("right shift"))
            {
                cursorEnabled = !cursorEnabled;
            }
            SetCursorLock(cursorEnabled);


            if (Input.GetKeyUp("u"))
            {
                ballMovement.hitForce = ballMovement.hitForce - 1000f;
            }
            if (Input.GetKeyUp("i"))
            {
                ballMovement.hitForce = ballMovement.hitForce + 1000f;
            }
            if (Input.GetKeyUp("o"))
            {
                shouldShoot = true;
            }

            if (shouldShoot) {
                Shoot();
                shouldShoot = false;
            }

            if (guiUseDegreesSlider)
            {
                var angles = playerCamPivot.transform.rotation.eulerAngles;
                playerCamPivot.transform.eulerAngles = new Vector3(angles.x, guiDegreesSliderInput, angles.z);
            }
            else
            {
                float newYRotation;
                bool parsedRot = float.TryParse(rotationInput, out newYRotation);
                if (!String.IsNullOrEmpty(rotationInput) && parsedRot)
                {
                    var angles = playerCamPivot.transform.rotation.eulerAngles;
                    playerCamPivot.transform.eulerAngles = new Vector3(angles.x, newYRotation, angles.z);
                }
            }

            if (guiUseHitForceSlider)
            {
                ballMovement.hitForce = guiHitForceSliderInput;
            }
            else
            {
                float newHitForce;
                bool parsedHitForce = float.TryParse(hitForceInput, out newHitForce);
                if (!String.IsNullOrEmpty(hitForceInput) && parsedHitForce && !guiUseHitForceSlider)
                {
                    ballMovement.hitForce = newHitForce;
                }
            }

            prevHole = GetHole();
        }

        public static void ResetToSpawn()
        {
            var holeObject = GameObject.Find("SpawnHole" + GetHole());
            ResetToPosition(holeObject.transform.position);

            // ballMovementTransform.rotation = holeObject.transform.rotation;
            // playerCamPivot.transform.rotation = holeObject.gameObject.transform.rotation;
            // playerCamPivot.transform.Rotate(20f, 0f, 0f);

            ballMovement.preHitLocation = holeObject.transform.position;
        }

        public static void ResetToPosition(Vector3 pos)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            ballMovementTransform.position = pos;
            rb.isKinematic = false;
        }

        // copy pasted from BallMovement#Update
        public static void Shoot()
        {
            bool outOfBounds = GetInstanceField<bool>(typeof(BallMovement), ballMovement, "outOfBounds");
            bool onTrolley = GetInstanceField<bool>(typeof(BallMovement), ballMovement, "onTrolley");
            float initialDrag = GetInstanceField<float>(typeof(BallMovement), ballMovement, "initialDrag");
            GameObject hitPoint = GameObject.Find("HitPoint");
            Vector3 playerPosition = new Vector3(
                hitPoint.transform.position.x,
                ballMovementTransform.position.y,
                hitPoint.transform.position.z
            );
            rb.angularDrag = 1f;
            rb.drag = initialDrag + ballMovement.sandDragToApply + ballMovement.waterDragToApply;
            if (ballMovement.hitForce > 0f)
            {
                rb.AddForce(-(playerPosition - ballMovementTransform.position).normalized * (ballMovement.hitForce + 1f), ForceMode.Force);
                if (!onTrolley) {
                    ballMovement.hitCounter++;
                }
                ballMovementTime = 0f;
                isTrackingBallMovementTime = true;
            }
            if (!outOfBounds && !onTrolley)
            {
                ballMovement.preHitLocation = ballMovementTransform.position;
            }
            ballMovement.hitForce = 0f;
        }

        public static void OnGUI()
        {
            if (ballMovement == null)
            {
                return;
            }

            // If you press esc, defocus the text fields
            Event e = Event.current;
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(null);
            }

            const float w = 250;
            const float h = 500;
            const float textFieldW = 65;
            const float halfButtonW = ((w-10)/2);

            GUILayout.BeginArea(new Rect (5, 5, w, h), GUI.skin.box);

            GUILayout.Label("Current Hole: " + GetHole());
            GUILayout.Label("Ball Movement Time: " + ballMovementTime);
            GUILayout.Label("Ball Position: " + ballMovementTransform.position);
            if (obstacleTransform != null)
            {
                GUILayout.Label("Obstacle Rotation: " + obstacleTransform.rotation.eulerAngles);
                GUILayout.Label("Symmetric Rotation: " + obstacleRotationDegrees);
                guiObstacleRotationShootToggle = GUILayout.Toggle(guiObstacleRotationShootToggle, "Shoot when rotation is:");
                guiObstacleRotationInput = GUILayout.TextField(guiObstacleRotationInput, 10, GUILayout.Width(textFieldW));
            }

            GUILayout.BeginHorizontal();
            float hitForceSource = ballMovement.hitForce;
            // since it resets to 0 after shooting
            if (hitForceSource == 0f)
            {
                hitForceSource = guiHitForceSliderInput;
            }
            hitForceInput = GUILayout.TextField(hitForceInput, 10, GUILayout.Width(textFieldW));
            GUILayout.Label("hitForce: " + ballMovement.hitForce);
            GUILayout.EndHorizontal();
            guiUseHitForceSlider = GUILayout.Toggle(guiUseHitForceSlider, "Use Slider");
            if (guiUseHitForceSlider)
            {
                guiHitForceSliderInput = GUILayout.HorizontalSlider(hitForceSource, 0f, 10500f);
                hitForceInput = guiHitForceSliderInput.ToString();
            }

            GUILayout.BeginHorizontal();
            float yDegrees = playerCamPivot.transform.rotation.eulerAngles.y;
            string yDegreesString = yDegrees.ToString();
            rotationInput = GUILayout.TextField(rotationInput, 10, GUILayout.Width(textFieldW));
            GUILayout.Label("degrees: " + yDegreesString);
            GUILayout.EndHorizontal();
            guiUseDegreesSlider = GUILayout.Toggle(guiUseDegreesSlider, "Use Slider");
            if (guiUseDegreesSlider)
            {
                guiDegreesSliderInput = GUILayout.HorizontalSlider(yDegrees, 0f, 360f);
                rotationInput = yDegreesString;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retry Shot", GUILayout.Width(halfButtonW)))
            {
                guiRetryShotClicked = true;
            }
            if (GUILayout.Button("Shoot", GUILayout.Width(halfButtonW)))
            {
                shouldShoot = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset shot", GUILayout.Width(halfButtonW)))
            {
                guiResetShotClicked = true;
            }
            if (GUILayout.Button("Reset hole", GUILayout.Width(halfButtonW)))
            {
                guiResetHoleClicked = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous hole", GUILayout.Width(halfButtonW)))
            {
                shouldGoToPreviousHole = true;
            }
            if (GUILayout.Button("Next hole", GUILayout.Width(halfButtonW)))
            {
                shouldGoToNextHole = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            guiSkipIntermissions = GUILayout.Toggle(guiSkipIntermissions, "Skip Intermissions");
            GUILayout.EndHorizontal();
            guiLockHole = GUILayout.Toggle(guiLockHole, "Lock Current Hole");

            GUILayout.EndArea();
        }

        // don't currently need these
        public static void PreStart()
        {
            Debug.Log("PreStart called");
            prevHole = 1;
            cursorEnabled = false;
            return;
        }
        public static void PreFixedUpdate()
        {
            return;
        }
        public static void OnDestroy()
        {
            Debug.Log("OnDestroy called");
            ballMovement = null;
            ballMovementTransform = null;
            rb = null;
            playerCamPivot = null;
            _Script = null;

            return;
        }
        public static void LateUpdate()
        {
            return;
        }
    }
}
