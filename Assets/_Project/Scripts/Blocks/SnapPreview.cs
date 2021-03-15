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
                case State.OneSocketAlignment:
                    Visible = true;
                    Snap = socketPairs;
                    material.color = Color.magenta.SetAlpha(.25f);
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

        private enum State
        {
            NotAligned, Blocking, SocketsTooFar, OneSocketAlignment, Ok
        }
        
        private (State, (Socket ThisSocket, Socket OtherSocket)[]) Loool()
        {
            var (position, rotation, connections, valid) = this.AlignShadow(owner);
            if (valid == false)
            {
                return (State.NotAligned, null);
            }

            transform.position = position;
            transform.rotation = rotation;
            
            var isValid = IsPositionValid();
            collidersOverllaping.Clear();

            if (isValid == false)
            {
                return (State.Blocking, null);
            }
            
            var blockSockets = owner.GetComponentsInChildren<Socket>().ToSet();

            var connectionCandidates = GetComponentsInChildren<Socket>()
                .Select(s => (ThisSocket: s, OtherSocket: s.Trigger().FirstOrDefault()))
                .Where(pair => pair.OtherSocket != null)
                .Where(pair => blockSockets.Contains(pair.OtherSocket) == false)
                .ToList();
            
            var snap = connectionCandidates
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
                if (connections == snap.Length)
                {
                    return (State.OneSocketAlignment, snap);
                }
                else
                {
                    return (State.SocketsTooFar, null);
                }
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
    }
}