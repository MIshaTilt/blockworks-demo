using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks.Builder
{
    public class BasicFactoryStrategy : BlockFactoryStrategy
    {
        [SerializeField] private Vector3Int size = new Vector3Int(1, 1, 1);
        [SerializeField] private Mesh blockMesh;
        
        public override Mesh BuildMesh()
        {
            return ExtendMesh();
        }

        public override void SetupPins(GameObject go)
        {
            AddSocket(go.transform, SocketType.Male, new Vector3(0, 0.2f * size.y, 0), size.x, size.z);
            AddSocket(go.transform, SocketType.Female, new Vector3(0, 0, 0), size.x, size.z);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            size = size.Max(new Vector3Int(1, 1, 1));
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