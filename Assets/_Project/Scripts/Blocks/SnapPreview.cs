using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using NUnit.Framework;
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
            var (state, socketPairs) = Loool();

            switch (state)
            {
                case State.NotAligned:
                    Visible = true;
                    Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
                    material.color = Color.green.SetAlpha(.25f);
                    break;
                case State.Blocking:
                    Visible = true;
                    Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
                    material.color = Color.red.SetAlpha(.25f);
                    break;
                case State.SocketsTooFar:
                    Visible = true;
                    Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
                    material.color = Color.yellow.SetAlpha(.25f);
                    break;
                case State.Ok:
                    Visible = true;
                    Snap = socketPairs;
                    material.color = Color.blue.SetAlpha(.25f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        //  private void Update()
        // {
        //     var alignment = this.GetShadowAlign(owner);
        //     if (alignment.valid == false)
        //     {
        //         Visible = false;
        //         ColorBlock(false);
        //         Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
        //         return;
        //     }
        //
        //     transform.position = alignment.position;
        //     transform.rotation = alignment.rotation;
        //     
        //     var isValid = IsPositionValid();
        //     collidersOverllaping.Clear();
        //
        //     if (isValid == false)
        //     {
        //         Visible = true;
        //         ColorBlock(false);
        //         Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
        //         return;
        //     }
        //     
        //     var blockSockets = owner.GetComponentsInChildren<Socket>().ToSet();
        //
        //     var cloneSockets = GetComponentsInChildren<Socket>();
        //     var candidates = cloneSockets
        //         .Select(s => (ThisSocket: s, OtherSocket: s.Trigger().FirstOrDefault()))
        //         .Where(pair => pair.OtherSocket != null)
        //         .Where(pair => blockSockets.Contains(pair.OtherSocket) == false)
        //         .ToArray();
        //
        //     Snap = candidates
        //         .Where(pair =>
        //         {
        //             var (thisSocket, otherSocket) = pair;
        //             var distance = otherSocket.transform.position.Distance(thisSocket.transform.position);
        //             return distance < LockDistanceEpsilon;
        //         })
        //         .Select(pair =>
        //         {
        //             var (thisSocket, otherSocket) = pair;
        //             return (cloneToBlockMapping[thisSocket], otherSocket);
        //         })
        //         .ToArray();
        //
        //     if (Snap.Length < minContactPoint)
        //     {
        //         ColorBlock(false);
        //         Visible = false;
        //         Snap = new (Socket ThisSocket, Socket OtherSocket)[0];
        //         return;
        //     }
        //
        //     Visible = true;
        //     ColorBlock(true);
        // }

        private enum State
        {
            NotAligned, Blocking, SocketsTooFar, Ok
        }
        
        private (State, (Socket ThisSocket, Socket OtherSocket)[]) Loool()
        {
            var alignment = this.GetShadowAlign(owner);
            if (alignment.valid == false)
            {
                return (State.NotAligned, null);
            }

            transform.position = alignment.position;
            transform.rotation = alignment.rotation;
            
            var isValid = IsPositionValid();
            collidersOverllaping.Clear();

            if (isValid == false)
            {
                return (State.Blocking, null);
            }
            
            var blockSockets = owner.GetComponentsInChildren<Socket>().ToSet();

            var snap = GetComponentsInChildren<Socket>()
                .Select(s => (ThisSocket: s, OtherSocket: s.Trigger().FirstOrDefault()))
                .Where(pair => pair.OtherSocket != null)
                .Where(pair => blockSockets.Contains(pair.OtherSocket) == false)
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

            if (snap.Length < minContactPoint)
            {
                return (State.SocketsTooFar, null);
            }

            return (State.Ok, snap);
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


        private class SocketTag : MonoBehaviour
        {
            public string id;
        }

        private void OnDrawGizmos()
        {
            // var colors = new[]
            // {
            //     Color.red, Color.green, Color.blue,
            //     Color.yellow, Color.cyan, Color.magenta,
            //     Color.white, Color.black
            // };
            //
            // var connections = owner.GetConnections();
            // var nonCollinear = FilterOutCollinear(connections);
            //
            // for (var i = 0; i < connections.Length; i++)
            // {
            //     var connection = connections[i];
            //     var from = connection.thisSocket.transform.position;
            //     var to = connection.otherSocket.transform.position;
            //     
            //     Gizmos.color = nonCollinear.Contains(connection) ? colors[i % colors.Length] : Color.grey;
            //     Gizmos.DrawSphere(from, 0.01f);
            //     Gizmos.DrawSphere(to, 0.01f);
            //     Gizmos.DrawLine(from, to);
            // }
        }
    }
}