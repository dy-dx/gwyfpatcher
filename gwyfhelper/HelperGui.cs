using System;
using UnityEngine;
namespace gwyfhelper
{
    public class HelperGui
    {
        public static bool skipIntermissions;
        public static bool lockHole;
        public static bool resetHoleClicked;
        public static bool resetShotClicked;
        public static bool retryShotClicked;
        public static bool skipToPreviousHoleClicked;
        public static bool skipToNextHoleClicked;
        public static bool useHitForceSlider;
        public static bool useDegreesSlider;
        public static float hitForceSliderInput;
        public static float degreesSliderInput;
        public static string hitForceInput = "";
        public static string rotationInput = "";

        public static bool obstacleRotationShootToggle;
        public static string obstacleRotationInput = "";

        public static void OnGUI()
        {
            // If you press esc, defocus the text fields
            Event e = Event.current;
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(null);
            }

            const float w = 250;
            const float h = 500;
            const float textFieldW = 65;
            const float halfButtonW = ((w - 10) / 2);

            GUILayout.BeginArea(new Rect(5, 5, w, h), GUI.skin.box);

            GUILayout.Label("Current Hole: " + GoFast.GetHole());
            GUILayout.Label("Ball Movement Time: " + GoFast.ballMovementTime);
            GUILayout.Label("Ball Position: " + GoFast.ballMovement.transform.position);
            if (GoFast.obstacleTransform != null)
            {
                GUILayout.Label("Obstacle Rotation: " + GoFast.obstacleTransform.rotation.eulerAngles);
                GUILayout.Label("Symmetric Rotation: " + GoFast.obstacleRotationDegrees);
                obstacleRotationShootToggle = GUILayout.Toggle(obstacleRotationShootToggle, "Shoot when rotation is:");
                obstacleRotationInput = GUILayout.TextField(obstacleRotationInput, 10, GUILayout.Width(textFieldW));
            }

            GUILayout.BeginHorizontal();
            float hitForceSource = GoFast.ballMovement.hitForce;
            // since it resets to 0 after shooting
            if (hitForceSource == 0f)
            {
                hitForceSource = hitForceSliderInput;
            }
            hitForceInput = GUILayout.TextField(hitForceInput, 10, GUILayout.Width(textFieldW));
            GUILayout.Label("hitForce: " + GoFast.ballMovement.hitForce);
            GUILayout.EndHorizontal();
            useHitForceSlider = GUILayout.Toggle(useHitForceSlider, "Use Slider");
            if (useHitForceSlider)
            {
                hitForceSliderInput = GUILayout.HorizontalSlider(hitForceSource, 0f, 10500f);
                hitForceInput = hitForceSliderInput.ToString();
            }

            GUILayout.BeginHorizontal();
            float yDegrees = GoFast.playerCamPivot.transform.rotation.eulerAngles.y;
            string yDegreesString = yDegrees.ToString();
            rotationInput = GUILayout.TextField(rotationInput, 10, GUILayout.Width(textFieldW));
            GUILayout.Label("degrees: " + yDegreesString);
            GUILayout.EndHorizontal();
            useDegreesSlider = GUILayout.Toggle(useDegreesSlider, "Use Slider");
            if (useDegreesSlider)
            {
                degreesSliderInput = GUILayout.HorizontalSlider(yDegrees, 0f, 360f);
                rotationInput = yDegreesString;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retry Shot", GUILayout.Width(halfButtonW)))
            {
                retryShotClicked = true;
            }
            if (GUILayout.Button("Shoot", GUILayout.Width(halfButtonW)))
            {
                GoFast.Shoot();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset shot", GUILayout.Width(halfButtonW)))
            {
                resetShotClicked = true;
            }
            if (GUILayout.Button("Reset hole", GUILayout.Width(halfButtonW)))
            {
                resetHoleClicked = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous hole", GUILayout.Width(halfButtonW)))
            {
                skipToPreviousHoleClicked = true;
            }
            if (GUILayout.Button("Next hole", GUILayout.Width(halfButtonW)))
            {
                skipToNextHoleClicked = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            skipIntermissions = GUILayout.Toggle(skipIntermissions, "Skip Intermissions");
            GUILayout.EndHorizontal();
            lockHole = GUILayout.Toggle(lockHole, "Lock Current Hole");

            GUILayout.EndArea();
        }
    }
}
