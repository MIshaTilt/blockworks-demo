using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks.Builder
{
    public abstract class BlockFactoryStrategy : MonoBehaviour
    {
        [SerializeField] private Mesh pinMesh;
        public abstract Mesh BuildMesh();
        public abstract void SetupPins(GameObject go);
        
        protected virtual void OnValidate()
        {
            GetComponent<BlockFactory>().OnValidate();
        }

        protected void AddSocket(Transform parent, SocketType type, Vector3 offset, int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                for (var z = 0; z < height; z++)
                {
                    var pin = new GameObject($"{(type == SocketType.Male ? "Male" : "Female")} [{x}][{z}]");
                    var socket = pin.AddComponent<Socket>();
                    socket.Type = type;
                    pin.transform.SetParent(parent, false);
                    if (type == SocketType.Female)
                    {
                        pin.transform.localRotation = Quaternion.Euler(180, 0, 0);
                    }
                    else
                    {
                        var pinMf = pin.AddComponent<MeshFilter>();
                        pinMf.sharedMesh = pinMesh;
                    }
                    var offset2 = new Vector3(x + .5f, 0, z + .5f);
                    pin.transform.position = offset + offset2.Multiply(new Vector3(0.5f, 1, 0.5f)).Snap(0.01f);
                }
            }
        }
    }
}