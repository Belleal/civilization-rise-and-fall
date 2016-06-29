using UnityEngine;
using System.Collections;
using UnityEditor;
using Ikari.VertexPainter;
using System.Linq;

public class IVPRaycast : MonoBehaviour {
    public static void DoRaycast()
    {
        if (IVPVariables.Raycast.Raycasting)
        {
            //Disable click object when painting
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            //Disable active tools
            if (Tools.current != Tool.None)
            {
                Tools.current = Tool.None;
            }

            //If we move the mouse or we drag the mouse
            if (Event.current.type == EventType.MouseMove && Event.current.button == 0 || Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                //Cast a ray from the scene
                IVPVariables.Raycast.Ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                IVPVariables.Raycast.HitData = new RaycastHit();

                //If the ray hits something
                if (Physics.Raycast(IVPVariables.Raycast.Ray, out IVPVariables.Raycast.HitData, 1000.0f))
                {
                    IVPVariables.Raycast.Raycasting = true;            
                }
            }
        }

        else {
            if (Tools.current == Tool.None)
            {
                Tools.current = Tool.Move;
            }
        }
    }
}
