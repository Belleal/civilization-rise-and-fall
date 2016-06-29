using UnityEngine;
using System.Collections;
using UnityEditor;
using Ikari.VertexPainter;

public class IVPGizmos : MonoBehaviour {
    public static void DrawHandle()
    {
        SceneView.RepaintAll();

        if (IVPController.IVPData.solidHandle)
        {
            Handles.color = IVPController.IVPData.handleColor;
            Handles.DrawSolidDisc(IVPVariables.Raycast.HitData.point, IVPVariables.Raycast.HitData.normal, IVPController.IVPData.brushSize);
            Handles.color = IVPController.IVPData.handleColor;
            Handles.DrawSolidDisc(IVPVariables.Raycast.HitData.point, IVPVariables.Raycast.HitData.normal, IVPController.IVPData.brushSize);
        }

        else
        {
            Handles.color = IVPController.IVPData.handleColor;
            Handles.DrawWireDisc(IVPVariables.Raycast.HitData.point, IVPVariables.Raycast.HitData.normal, IVPController.IVPData.brushSize);
            Handles.color = IVPController.IVPData.handleColor;
            Handles.DrawWireDisc(IVPVariables.Raycast.HitData.point, IVPVariables.Raycast.HitData.normal, IVPController.IVPData.brushSize);
        }

        if (IVPController.IVPData.drawHandleOutline)
        {
            Handles.color = IVPController.IVPData.outlineHandleColor;
            Handles.DrawWireDisc(IVPVariables.Raycast.HitData.point, IVPVariables.Raycast.HitData.normal, IVPController.IVPData.brushSize - 0.005f);
        }

        if (IVPController.IVPData.drawHandleAngle)
        {
            Handles.Label(new Vector3(IVPVariables.Raycast.HitData.point.x - 0.12f, IVPVariables.Raycast.HitData.point.y + 0.25f, IVPVariables.Raycast.HitData.point.z), Vector3.Angle(IVPVariables.Raycast.HitData.normal, Vector3.up).ToString("#.##"), EditorStyles.largeLabel);
        }
    }
}
