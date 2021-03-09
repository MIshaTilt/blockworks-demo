using UnityEditor;
using UnityEngine;

namespace Blocks.Editor
{
    [CustomEditor(typeof(ChunkSnapper))]
    public class ChunkSnapperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var block = (ChunkSnapper) target;
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Begin Snap")) block.BeginSnap();
            if (GUILayout.Button("End Snap")) block.EndSnap();
            GUILayout.EndHorizontal();
        }
    }
}