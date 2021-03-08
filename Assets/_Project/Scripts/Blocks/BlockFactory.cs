using System.IO;
using ElasticSea.Framework.Extensions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blocks
{
    public class BlockFactory : MonoBehaviour
    {
        [SerializeField] private Vector3Int size = new Vector3Int(1, 1, 1);
        [SerializeField] private string name;
        [SerializeField] private string path;
        [SerializeField] private Mesh blockMesh;
        [SerializeField] private Mesh pinMesh;
        [SerializeField] private Material material;

#if UNITY_EDITOR
        public void CreatePrefab()
        {
            var go = BuildBlock();

            AssetDatabase.CreateAsset(go.GetComponent<MeshFilter>().sharedMesh, Path.Combine(path, $"{name}_mesh.asset"));
            PrefabUtility.SaveAsPrefabAsset(go, Path.Combine(path, $"{name}.prefab"));
            AssetDatabase.SaveAssets();

            DestroyImmediate(go);
        }
#endif

        private void OnValidate()
        {
            size = size.Max(new Vector3Int(1, 1, 1));
            this.RunDelayed(0, () =>
            {
                transform.DestroyChildren(true);

                var go = BuildBlock();
                go.transform.SetParent(transform, true);
            });
        }

        private GameObject BuildBlock()
        {
            var go = new GameObject(name);
            var mesh = ExtendMesh();

            for (var x = 0; x < size.x; x++)
            {
                for (var z = 0; z < size.z; z++)
                {
                    var pin = new GameObject($"Pin [{x}][{z}]");

                    var pinMf = pin.AddComponent<MeshFilter>();
                    pinMf.sharedMesh = pinMesh;

                    var pinMr = pin.AddComponent<MeshRenderer>();
                    pinMr.material = material;

                    pin.transform.SetParent(go.transform, false);

                    var offset = new Vector3(x + .5f, size.y, z + .5f);
                    pin.transform.position = offset.Multiply(blockMesh.bounds.size);
                }
            }

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.material = material;

            return go;
        }

        private Mesh ExtendMesh()
        {
            var bounds = blockMesh.bounds;
            var vertices = blockMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                var x = vert.x > bounds.center.x / 2 ? vert.x + (size.x - 1) * bounds.size.x : vert.x;
                var y = vert.y > bounds.center.y / 2 ? vert.y + (size.y - 1) * bounds.size.y : vert.y;
                var z = vert.z > bounds.center.z / 2 ? vert.z + (size.z - 1) * bounds.size.z : vert.z;
                vertices[i] = new Vector3(x, y, z);
            }

            var newMesh = new Mesh
            {
                vertices = vertices,
                triangles = blockMesh.triangles,
                normals = blockMesh.normals,
                uv = blockMesh.uv,
                tangents = blockMesh.tangents
            };
            newMesh.RecalculateBounds();
            return newMesh;
        }
    }
}