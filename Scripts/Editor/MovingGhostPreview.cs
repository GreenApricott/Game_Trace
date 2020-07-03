using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MovingGhostPreview
{
    static public MovingGhostPreview s_Preview = null;
    static public GameObject preview;

    static protected MovingGhost movingGhost;

    static MovingGhostPreview()
    {
        Selection.selectionChanged += SelectionChanged;
    }

    static void SelectionChanged()
    {
        if (movingGhost != null && Selection.activeGameObject != movingGhost.gameObject)
        {
            DestroyPreview();
        }
    }

    static public void DestroyPreview()
    {
        if (preview == null)
            return;

        Object.DestroyImmediate(preview);
        preview = null;
        movingGhost = null;
    }

    static public void CreateNewPreview(MovingGhost origin)
    {
        if (preview != null)
        {
            Object.DestroyImmediate(preview);
        }

        movingGhost = origin;

        preview = Object.Instantiate(origin.gameObject);
        preview.hideFlags = HideFlags.DontSave;
        MovingGhost ght = preview.GetComponentInChildren<MovingGhost>();
        Object.DestroyImmediate(ght);


        Color c = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        SpriteRenderer[] rends = preview.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < rends.Length; ++i)
            rends[i].color = c;
    }
}
