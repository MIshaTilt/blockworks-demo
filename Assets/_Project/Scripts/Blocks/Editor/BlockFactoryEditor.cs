using Blocks.Builder;
using UnityEditor;
using UnityEngine;

namespace Blocks.Editor
{
    [CustomEditor(typeof(BlockFactory))]
    public class BlockFactoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var blockFactory = target as BlockFactory;
            if (GUILayout.Button("Create Prefab"))
            {
                blockFactory.CreatePrefab();
            }
        }
    }
}