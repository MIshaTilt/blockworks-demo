using System.IO;
using _Project.Scripts.Blocks;
using ElasticSea.Framework.Extensions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blocks.Builder
{
    public class BlockFactory : MonoBehaviour
    {
        [SerializeField] private BlockFactoryStrategy strategy;
        [SerializeField] private string name;
        [SerializeField] private string path;
        [SerializeField] private Material material;
        [SerializeField] private BlockMaterial blockMaterial;

#if UNITY_EDITOR
        public void CreatePrefab()
        {
            var go = BuildBlock();

            AssetDatabase.CreateAsset(go.GetComponentInChildren<MeshFilter>().sharedMesh, Path.Combine(path, $"{name}_mesh.asset"));
            PrefabUtility.SaveAsPrefabAsset(go, Path.Combine(path, $"{name}.prefab"));
            AssetDatabase.SaveAssets();

            DestroyImmediate(go);
        }
#endif

        public void OnValidate()
        {
            this.RunDelayed(0, () =>
            {
                transform.DestroyChildren(true);

                var go = BuildBlock();
                go.transform.SetParent(transform, true);
            });
        }

        private GameObject BuildBlock()
        {
            var chunkGo = new GameObject(name);
            
            var go = new GameObject(name);

            var mesh = strategy.BuildMesh();
            strategy.SetupPins(go);

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();

            var collider = go.AddComponent<BoxCollider>();
            collider.center = mesh.bounds.center;
            collider.size = mesh.bounds.size;

            foreach (var mr2 in go.GetComponentsInChildren<MeshRenderer>())
            {
                mr2.material = material;
            }
            
            var block = go.AddComponent<Block>();
            block.BlockMaterial = blockMaterial;
            
            go.transform.SetParent(chunkGo.transform, false);

            var rb = chunkGo.AddComponent<Rigidbody>();
            
            var chunk = chunkGo.AddComponent<Chunk>();

            var snapper = chunkGo.AddComponent<ChunkSnapper>();
            
            return chunkGo;
        }
    }
}