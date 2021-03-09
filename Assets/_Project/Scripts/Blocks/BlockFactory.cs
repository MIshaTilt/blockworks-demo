using System.IO;
using _Project.Scripts.Blocks;
using Blocks.Sockets;
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
            var chunkGo = new GameObject(name);
            
            var go = new GameObject(name);
            var mesh = ExtendMesh();

            for (var x = 0; x < size.x; x++)
            {
                for (var z = 0; z < size.z; z++)
                {
                    var pin = new GameObject($"Male [{x}][{z}]");
                    var pinMf = pin.AddComponent<MeshFilter>();
                    pinMf.sharedMesh = pinMesh;
                    var pinMr = pin.AddComponent<MeshRenderer>();
                    pinMr.material = material;
                    var socket = pin.AddComponent<Socket>();
                    socket.Type = SocketType.Male;
                    pin.transform.SetParent(go.transform, false);
                    var offset = new Vector3(x + .5f, size.y, z + .5f);
                    pin.transform.position = offset.Multiply(blockMesh.bounds.size);
                    
                    var pin2 = new GameObject($"Female [{x}][{z}]");
                    var socket2 = pin2.AddComponent<Socket>();
                    socket2.Type = SocketType.Female;
                    pin2.transform.SetParent(go.transform, false);
                    pin2.transform.localRotation = Quaternion.Euler(180, 0, 0);
                    var offset2 = new Vector3(x + .5f, 0, z + .5f);
                    pin2.transform.position = offset2.Multiply(blockMesh.bounds.size);
                }
            }

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.material = material;

            var collider = go.AddComponent<BoxCollider>();
            collider.center = mesh.bounds.center;
            collider.size = mesh.bounds.size;

            
            var block = go.AddComponent<Block>();
            block.BlockMaterial = blockMaterial;
            
            go.transform.SetParent(chunkGo.transform, false);

            var rb = chunkGo.AddComponent<Rigidbody>();
            
            var chunk = chunkGo.AddComponent<Chunk>();

            var snapper = chunkGo.AddComponent<ChunkSnapper>();
            
            return chunkGo;
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