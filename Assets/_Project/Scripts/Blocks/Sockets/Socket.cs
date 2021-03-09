using System;
using System.Linq;
using _Project.Scripts.Blocks;
using ElasticSea.Framework.Util;
using UnityEngine;

namespace Blocks.Sockets
{
    public class Socket : MonoBehaviour
    {
        [SerializeField] private Block owner;
        [SerializeField] private SocketType type;
        [SerializeField] private float radius = 0.125f;
        [SerializeField] private bool active = true;

        [SerializeField] private Socket connectedSocket;
        
        private SphereCollider trigger;

        public Block Owner
        {
            get
            {
                if (owner == null)
                {
                    owner = GetComponentInParent<Block>();

                    if (owner == null)
                    {
                        throw new InvalidOperationException("This socket does not belong to any block. And no parent block could not be found.");
                    }
                }
                return owner;
            }
            set => owner = value;
        }

        public SocketType Type
        {
            get => type;
            set => type = value;
        }

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                if (trigger) trigger.radius = value;
            }
        }

        public bool Active
        {
            get => active;
            set
            {
                active = value;
                if (trigger) trigger.enabled = active && connectedSocket == false;
            }
        }

        public Socket ConnectedSocket => connectedSocket;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Socket");
            trigger = gameObject.AddComponent<SphereCollider>();
            trigger.center = Vector3.zero;
            trigger.isTrigger = trigger;
            Active = active;
            Radius = radius;
        }

        public Socket[] Trigger()
        {
            var position = transform.position;
            var radius = Radius * transform.lossyScale.x;
            var layerMask = LayerMask.GetMask("Socket");
            var candidates = Physics.OverlapSphere(position, radius, layerMask);

            return candidates
                .Select(c => c.GetComponent<Socket>())
                .Where(s => s.Type != Type)
                .Where(s => s.Owner != Owner)
                .ToArray();
        }
        
        public void Connect(Socket socket)
        {
            if (connectedSocket == null)
            {
                AttachSocket(socket);
                socket.AttachSocket(this);
            }
        }

        private void AttachSocket(Socket other)
        {
            connectedSocket = other;
            trigger.enabled = false;
        }
        
        public void Disconnect()
        {
            if (connectedSocket != null)
            {
                connectedSocket.DetachSocket();
                DetachSocket();
            }
        }
        private void DetachSocket()
        {
            connectedSocket = null;
            trigger.enabled = active;
        }

        private void OnDrawGizmos()
        {
            if (active == false)
            {
                return;
            }

            if (connectedSocket != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.gray.SetAlpha(.5f);
                Gizmos.DrawSphere(Vector3.zero, Radius/2);
                return;
            }
            
            var candidates = Trigger();
            if (candidates.Any())
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, Radius * .5f);
                
                foreach (var candidate in candidates)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.matrix = Matrix4x4.identity;
                    GizmoUtils.DrawLine(transform.position, candidate.transform.position, 5);
                }
                
                var color = Type == SocketType.Male ? Color.blue : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = color.SetAlpha(.5f);
                Gizmos.DrawWireSphere(Vector3.zero, Radius);
            }
            else
            {
                var color = Type == SocketType.Male ? Color.blue : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = color.SetAlpha(.5f);
                Gizmos.DrawSphere(Vector3.zero, Radius);
            }

        }
    }
}