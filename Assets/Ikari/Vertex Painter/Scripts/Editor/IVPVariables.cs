using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IVPVariables : MonoBehaviour {

    public static class Interaction
    {
        public static bool Painting;
    }

    public static class Raycast
    {
        public static bool Raycasting;
        public static Ray Ray;
        public static RaycastHit HitData;

        public static bool IsHitting()
        {
            if (HitData.collider != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static void ResetHit()
        {
            HitData = new RaycastHit();
        }
    }

    public static class Data
    {
        public static GameObject ActualObject;
        public static Mesh ActualMesh;
        public static GameObject[] SelectedObjects;
        public static List<GameObject> EditedObjects = new List<GameObject>();
        public static List<Color32> ColorClipboard = new List<Color32>();
    }

    public static class Gizmo
    {

    }
}
