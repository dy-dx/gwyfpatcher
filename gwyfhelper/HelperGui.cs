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

        public static bool obstacleMovementShootToggle;
        public static string obstacleMovementInput = "";
        public static string obstacleMovementInput2 = "";
        public static string secondObstacleMovementInput = "";
        public static string secondObstacleMovementInput2 = "";

        public static void OnGUI()
        {
            // If you press esc, defocus the text fields
            Event e = Event.current;
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(null);
            }

            const float w = 250;
            const float h = 600;
            const float textFieldW = 65;
            const float halfButtonW = ((w - 10) / 2);

            GUILayout.BeginArea(new Rect(5, 5, w, h), GUI.skin.box);

            GUILayout.Label("Current Hole: " + GoFast.GetHole());
            GUILayout.Label("Ball Movement Time: " + GoFast.ballMovementTime);

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


            if (GoFast.obstacleTransform != null)
            {
                // GUILayout.Label("Obstacle Name: " + GoFast.obstacleTransform.gameObject.name);
                GUILayout.Label("Obstacle mvmt: " + GoFast.obstacleMovementAmount);
            }
            if (GoFast.secondObstacleTransform != null)
            {
                // GUILayout.Label("2nd Obstacle Name: " + GoFast.secondObstacleTransform.gameObject.name);
                GUILayout.Label("2nd Obstacle mvmt: " + GoFast.secondObstacleMovementAmount);
            }
            if (GoFast.obstacleTransform != null)
            {
                obstacleMovementShootToggle = GUILayout.Toggle(obstacleMovementShootToggle, "Shoot when movement is between:");
                GUILayout.BeginHorizontal();
                obstacleMovementInput = GUILayout.TextField(obstacleMovementInput, 10, GUILayout.Width(textFieldW));
                obstacleMovementInput2 = GUILayout.TextField(obstacleMovementInput2, 10, GUILayout.Width(textFieldW));
                GUILayout.EndHorizontal();
            }
            if (GoFast.secondObstacleTransform != null)
            {
                GUILayout.BeginHorizontal();
                secondObstacleMovementInput = GUILayout.TextField(secondObstacleMovementInput, 10, GUILayout.Width(textFieldW));
                secondObstacleMovementInput2 = GUILayout.TextField(secondObstacleMovementInput2, 10, GUILayout.Width(textFieldW));
                GUILayout.EndHorizontal();
            }


            GUILayout.Label("Ball Position: " + GoFast.ballMovement.transform.position);
            if (GoFast.objectMouseIsOver != null)
            {
                GUILayout.Label("Cursor is over: " + GoFast.objectMouseIsOver.name);
                GUILayout.Label("  Pos: " + GoFast.objectMouseIsOver.transform.position);
                GUILayout.Label("  Euler: " + GoFast.objectMouseIsOver.transform.rotation.eulerAngles);
            }

            GUILayout.EndArea();
        }
    }
}
