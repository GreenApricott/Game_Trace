using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingGhost))]
public class MovingGhostEditor : Editor
{
    MovingGhost m_movingGhost;

    float m_PreviewPosition = 0;

    private void OnEnable()
    {
        m_PreviewPosition = 0;
        m_movingGhost = target as MovingGhost;

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            MovingGhostPreview.CreateNewPreview(m_movingGhost);
    }

     private void OnDisable()
     {
        MovingGhostPreview.DestroyPreview();
     }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        m_PreviewPosition = EditorGUILayout.Slider("Preview position", m_PreviewPosition, 0.0f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            MovePreview();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical("box");//开始阴影
        EditorGUI.BeginChangeCheck();
        bool isStartingMoving = EditorGUILayout.Toggle("Start moving", m_movingGhost.isMovingAtStart);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed move at start");
            m_movingGhost.isMovingAtStart = isStartingMoving;
        }

        if (isStartingMoving)
        {
            EditorGUI.indentLevel += 1;
            EditorGUI.BeginChangeCheck();
            bool startOnlyWhenVisible = EditorGUILayout.Toggle("When becoming visible", m_movingGhost.startMovingOnlyWhenVisible);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed move when visible");
                m_movingGhost.startMovingOnlyWhenVisible = startOnlyWhenVisible;
            }
            EditorGUI.indentLevel -= 1;
        }
        EditorGUILayout.EndVertical();//结束阴影

        EditorGUI.BeginChangeCheck();
        MovingGhost.MovingGhostType platformType = (MovingGhost.MovingGhostType)EditorGUILayout.EnumPopup("Looping", m_movingGhost.platformType);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Moving Platform type");
            m_movingGhost.platformType = platformType;
        }

        EditorGUI.BeginChangeCheck();
        float newSpeed = EditorGUILayout.FloatField("Speed", m_movingGhost.speed);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Speed");
            m_movingGhost.speed = newSpeed;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (GUILayout.Button("添加移动节点"))
        {
            Undo.RecordObject(target, "added node");


            Vector3 position = m_movingGhost.localNodes[m_movingGhost.localNodes.Length - 1] + Vector3.right;

            ArrayUtility.Add(ref m_movingGhost.localNodes, position);
            ArrayUtility.Add(ref m_movingGhost.waitTimes, 0);
        }

        EditorGUIUtility.labelWidth = 64;
        int delete = -1;
        for (int i = 0; i < m_movingGhost.localNodes.Length; ++i)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            int size = 64;
            EditorGUILayout.BeginVertical(GUILayout.Width(size));
            EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));
            if (i != 0 && GUILayout.Button("Delete", GUILayout.Width(size)))
            {
                delete = i;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            Vector3 newPosition;
            if (i == 0)
                newPosition = m_movingGhost.localNodes[i];
            else
                newPosition = EditorGUILayout.Vector3Field("Position", m_movingGhost.localNodes[i]);

            float newTime = EditorGUILayout.FloatField("Wait Time", m_movingGhost.waitTimes[i]);
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "changed time or position");
                m_movingGhost.waitTimes[i] = newTime;
                m_movingGhost.localNodes[i] = newPosition;
            }
        }
        EditorGUIUtility.labelWidth = 0;

        if (delete != -1)
        {
            Undo.RecordObject(target, "Removed point moving platform");

            ArrayUtility.RemoveAt(ref m_movingGhost.localNodes, delete);
            ArrayUtility.RemoveAt(ref m_movingGhost.waitTimes, delete);
        }
    }

    private void OnSceneGUI()
    {
        MovePreview();

        for (int i = 0; i < m_movingGhost.localNodes.Length; ++i)
        {
            Vector3 worldPos;
            if (Application.isPlaying)
            {
                worldPos = m_movingGhost.worldNode[i];
            }
            else
            {
                worldPos = m_movingGhost.transform.TransformPoint(m_movingGhost.localNodes[i]);
            }


            Vector3 newWorld = worldPos;
            if (i != 0)
                newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

            Handles.color = Color.red;

            if (i == 0)
            {
                if (m_movingGhost.platformType != MovingGhost.MovingGhostType.LOOP)
                    continue;

                if (Application.isPlaying)
                {
                    Handles.DrawDottedLine(worldPos, m_movingGhost.worldNode[m_movingGhost.worldNode.Length - 1], 10);
                }
                else
                {
                    Handles.DrawDottedLine(worldPos, m_movingGhost.transform.TransformPoint(m_movingGhost.localNodes[m_movingGhost.localNodes.Length - 1]), 10);
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    Handles.DrawDottedLine(worldPos, m_movingGhost.worldNode[i - 1], 10);
                }
                else
                {
                    Handles.DrawDottedLine(worldPos, m_movingGhost.transform.TransformPoint(m_movingGhost.localNodes[i - 1]), 10);
                }

                if (worldPos != newWorld)
                {
                    Undo.RecordObject(target, "moved point");
                    m_movingGhost.localNodes[i] = m_movingGhost.transform.InverseTransformPoint(newWorld);
                }
            }
        }
    }

    void MovePreview()
    {
        //compute pos from 0-1 preview pos

        if (Application.isPlaying)
            return;

        float step = 1.0f / (m_movingGhost.localNodes.Length - 1);

        int starting = Mathf.FloorToInt(m_PreviewPosition / step);

        if (starting > m_movingGhost.localNodes.Length - 2)
            return;

        float localRatio = (m_PreviewPosition - (step * starting)) / step;

        Vector3 localPos = Vector3.Lerp(m_movingGhost.localNodes[starting], m_movingGhost.localNodes[starting + 1], localRatio);

        MovingGhostPreview.preview.transform.position = m_movingGhost.transform.TransformPoint(localPos);

        SceneView.RepaintAll();
    }

}
