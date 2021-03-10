using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks.Builder
{
    public class SlopeFactoryStrategy : BlockFactoryStrategy
    {
        [SerializeField] private Vector3Int size = new Vector3Int(1, 1, 1);
        [SerializeField] private Vector2Int offset = new Vector2Int(0, 0);
        [SerializeField] private Mesh blockMesh;
     
        public override Mesh BuildMesh()
        {
            return ExtendMesh();
        }

        public override void SetupPins(GameObject go)
        {
            AddSocket(go.transform, SocketType.Male, new Vector3(0, 0.2f * (size.y+1), 0), offset.x + 1, size.z);
            AddSocket(go.transform, SocketType.Female, new Vector3(0, 0, 0), size.x + 1, size.z);
        }

        private Mesh ExtendMesh()
        {
            var bounds = blockMesh.bounds;
            var vertices = blockMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                if (Mathf.Abs(0.5f - vert.x) < 0.01f)
                {
                    vert.x = vert.x + offset.x * .5f;
                }
                else
                {
                    vert.x = vert.x > .51f ? vert.x + (size.x - 1) * .5f : vert.x;
                }
                
                if (Mathf.Abs(0.2f - vert.y) < 0.01f)
                {
                    vert.y = vert.y + offset.y * .2f;
                }
                else
                {
                    vert.y= vert.y > .21f ? vert.y + (size.y - 1) * .2f : vert.y;
                }
                vert.z = vert.z > bounds.center.z / 2 ? vert.z + (size.z - 1) * .5f : vert.z;
                vertices[i] = vert;
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