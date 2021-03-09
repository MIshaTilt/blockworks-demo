using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    public class SnapPreview : MonoBehaviour
    {
        [SerializeField] private Chunk owner;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material material;
        [SerializeField] private Dictionary<Socket, Socket> cloneToBlockMapping;
        [SerializeField] private int minContactPoint = 2;
        public (Socket ThisSocket, Socket OtherSocket)[] Snap { get; private set; }

        private const float LockDistanceEpsilon = 1E-03f;

        private void Update()
        {
            var snapSucessful = TrySnap();
            if (snapSucessful == false)
            {
                Visible = false;
                ColorBlock(false);
                Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
                return;
            }
            
            var isValid = IsPositionValid();
            collidersOverllaping.Clear();

            if (isValid == false)
            {
                Visible = true;
                ColorBlock(false);
                Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
                return;
            }
            
            var blockSockets = owner.GetComponentsInChildren<Socket>().ToSet();

            var cloneSockets = GetComponentsInChildren<Socket>();
            var candidates = cloneSockets
                .Select(s => (ThisSocket: s, OtherSocket: s.Trigger().FirstOrDefault()))
                .Where(pair => pair.OtherSocket != null)
                .Where(pair => blockSockets.Contains(pair.OtherSocket) == false)
                .ToArray();

            Snap = candidates
                .Where(pair =>
                {
                    var (thisSocket, otherSocket) = pair;
                    var distance = otherSocket.transform.position.Distance(thisSocket.transform.position);
                    return distance < LockDistanceEpsilon;
                })
                .Select(pair =>
                {
                    var (thisSocket, otherSocket) = pair;
                    return (cloneToBlockMapping[thisSocket], otherSocket);
                })
                .ToArray();

            if (Snap.Length < minContactPoint)
            {
                ColorBlock(false);
                Visible = false;
                Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
                return;
            }

            Visible = true;
            ColorBlock(true);
        }

        private void ColorBlock(bool isValid)
        {
            var color = isValid ? Color.blue : Color.red;
            material.color = color.SetAlpha(.25f);
        }

        private bool IsPositionValid()
        {
            return collidersOverllaping
                .Select(c => c.GetComponentInParent<Chunk>())
                .None(b => b != owner);
        }

        private HashSet<Collider> collidersOverllaping = new HashSet<Collider>();

        private void OnTriggerStay(Collider other)
        {
            collidersOverllaping.Add(other);
        }

        public void BeginSnap()
        {
            gameObject.SetActive(true);
            SwitchLayerInChildren(transform, "Default", "Snap");
        }

        public void EndSnap()
        {
            if (Snap.Any())
            {
                // Copy the transform back
                owner.transform.CopyWorldFrom(transform);
                
                var newBlock = BlockUtils.ConnectBlocks(Snap);
                SwitchLayerInChildren(newBlock.transform, "Snap", "Default");
            }
            else
            {
                transform.GetComponent<Rigidbody>().isKinematic = false;
                SwitchLayerInChildren(transform, "Snap", "Default");
                gameObject.SetActive(false);
            }
        }

        private bool? visible;

        public bool Visible
        {
            get => visible.Value;
            set
            {
                if (value != visible)
                {
                    foreach (var renderer in renderers)
                    {
                        renderer.enabled = value;
                    }
                    
                    visible = value;
                }
            }
        }

        public Chunk Owner
        {
            get => owner;
            set => owner = value;
        }

        public Renderer[] Renderers
        {
            get => renderers;
            set => renderers = value;
        }

        public Material Material
        {
            get => material;
            set => material = value;
        }

        public Dictionary<Socket, Socket> CloneToBlockMapping
        {
            get => cloneToBlockMapping;
            set => cloneToBlockMapping = value;
        }

        public int MinContactPoint
        {
            get => minContactPoint;
            set => minContactPoint = value;
        }

        private void SwitchLayerInChildren(Transform target, string from, string to)
        {
            foreach (var child in target.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.layer == LayerMask.NameToLayer(from))
                {
                    child.gameObject.layer = LayerMask.NameToLayer(to);
                }
            }
        }

        private bool TrySnap()
        {
            var connections = owner.GetConnections();
            connections = FilterOutCollinear(connections);

            
            // Chose two closes connections and choose origin and alignment.
            // If only one connection is available use that one.
            if (connections.Length == 0)
            {
                return false;
            }
            else if (connections.Length == 1)
            {
                var thisSocketA = connections[0].thisSocket;
                var otherSocketA = connections[0].otherSocket;
            
                Align(thisSocketA, otherSocketA, owner.transform);
                return true;
            }
            else
            {
                var thisSocketA = connections[0].thisSocket;
                var otherSocketA = connections[0].otherSocket;
                var thisSocketB = connections[1].thisSocket;
                var otherSocketB = connections[1].otherSocket;
            
                Align(thisSocketA, thisSocketB, otherSocketA, otherSocketB, owner.transform);
                return true;
            }
        }

        private (Transform thisSocket, Transform otherSocket)[] FilterOutCollinear((Transform thisSocket, Transform otherSocket)[] connections)
        {
            var output = new List<(Transform thisSocket, Transform otherSocket)>();
            foreach (var connection1 in connections)
            {
                var isCollinear = false;
                foreach (var connection2 in connections)
                {
                    if (connection1 != connection2)
                    {
                        // Chose two closes connections and choose origin and alignment.
                        var thisSocketA = connection1.thisSocket;
                        var otherSocketA = connection1.otherSocket;
                        var thisSocketB = connection2.thisSocket;
                        var otherSocketB = connection2.otherSocket;
            
                        var directionA = otherSocketA.up;
                        var directionB = otherSocketA.position - otherSocketB.position;

                        // Check if the directions are collinear
                        if (Math.Abs(directionA.Dot(directionB)) > 0.0001f)
                        {
                            isCollinear = true;
                        }
                    }
                }

                if (isCollinear == false)
                {
                    output.Add(connection1);
                }
            }

            return output.ToArray();
        }

        private void Align(Transform thisA, Transform otherA, Transform block)
        {
            var thisDir = thisA.right.normalized;
            var otherDir = otherA.right.normalized;
            
            Align(thisA, thisDir, otherA, otherDir, block);
        }

        private void Align(Transform thisA, Transform thisB, Transform otherA, Transform otherB, Transform block)
        {
            var thisDir = (thisB.position - thisA.position).normalized;
            var otherDir = (otherB.position - otherA.position).normalized;

            Align(thisA, thisDir, otherA, otherDir, block);
        }
        
        private void Align(Transform thisA, Vector3 thisDir, Transform otherA, Vector3 otherDir, Transform block)
        {
            var thisToOtherRotation = Quaternion.FromToRotation(thisDir, otherDir);

            // Correct for rotation along the direction (multiple valid states for resulting rotation)
            var correctedUpVector = thisToOtherRotation * -thisA.up;
            var angle = Vector3.SignedAngle(correctedUpVector, otherA.up, otherDir);
            var correction = Quaternion.AngleAxis(angle, otherDir);

            var targetRotation = correction * thisToOtherRotation * block.rotation;

            transform.rotation = targetRotation;
            
            // TODO Get the resulting position and rotation without touching the transforms
            // Adjust position
            transform.position = block.position;
            var blockSocketLocalPosition = block.InverseTransformPoint(thisA.position);
            var adjustedWorldPosition = transform.TransformPoint(blockSocketLocalPosition);
            var offset = otherA.position - adjustedWorldPosition;
            transform.position += offset;
        }

        private class SocketTag : MonoBehaviour
        {
            public string id;
        }

        private void OnDrawGizmos()
        {
            var colors = new[]
            {
                Color.red, Color.green, Color.blue,
                Color.yellow, Color.cyan, Color.magenta,
                Color.white, Color.black
            };
            
            var connections = owner.GetConnections();
            var nonCollinear = FilterOutCollinear(connections);

            for (var i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                var from = connection.thisSocket.transform.position;
                var to = connection.otherSocket.transform.position;
                
                Gizmos.color = nonCollinear.Contains(connection) ? colors[i % colors.Length] : Color.grey;
                Gizmos.DrawSphere(from, 0.01f);
                Gizmos.DrawSphere(to, 0.01f);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}