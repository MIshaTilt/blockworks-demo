using UnityEditor;
using UnityEngine;

namespace Blocks.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChunkSnapper))]
    public class ChunkSnapperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Begin Snap")) foreach (ChunkSnapper target in targets) target.BeginSnap();
            if (GUILayout.Button("End Snap")) foreach (ChunkSnapper target in targets) target.EndSnap();
            GUILayout.EndHorizontal();
        }
    }
}