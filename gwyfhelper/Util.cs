using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace gwyfhelper
{
    public static class Util
    {
        public static T GetInstanceField<T>(Type type, object instance, string fieldName)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)type.GetField(fieldName, flags).GetValue(instance);
        }

        public static void SetInstanceField<T>(Type type, object instance, string fieldName, T value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            type.GetField(fieldName, flags).SetValue(instance, value);
        }

        public static T GetStaticField<T>(Type type, string fieldName)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            return (T)type.GetField(fieldName, flags).GetValue(null);
        }

        public static string GetCurrentDLLDirectory()
        {
            return Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location
            );
        }

        public static void LogObjects()
        {
            MonoBehaviour[] allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach(var obj in allObjects)
            {
                Console.WriteLine(obj+" is an active MonoBehaviour");
            }

            // GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            // foreach(GameObject obj in allObjects)
            // {
            //     Console.WriteLine(obj+" is an active object with tag: " + obj.tag);
            // }
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
            Console.WriteLine(msg);
        }

        public static void Alert(string msg)
        {
            SteamInvites steamInvites = UnityEngine.Object.FindObjectOfType<SteamInvites>();
            if (steamInvites == null)
            {
                return;
            }
            steamInvites.noButton.transform.parent.gameObject.SetActive(true);
            steamInvites.yesButton.transform.parent.gameObject.SetActive(false);
            steamInvites.text.transform.parent.gameObject.SetActive(true);
            steamInvites.text.GetComponent<TMPro.TextMeshProUGUI>().SetText(msg);
        }
        public static void HideAlert()
        {
            SteamInvites steamInvites = UnityEngine.Object.FindObjectOfType<SteamInvites>();
            if (steamInvites == null)
            {
                return;
            }
            steamInvites.text.transform.parent.gameObject.SetActive(false);
        }

        public static GameObject GetGameObjectCursorIsOver()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                return hit.transform.gameObject;
            }
            return null;
        }
    }
}
