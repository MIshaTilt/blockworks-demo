using System;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using Blocks.Sockets;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Blocks
{
    [ExecuteInEditMode]
    public class Block : MonoBehaviour
    {
        [SerializeField] private Chunk group;
        [SerializeField] private BlockMaterial blockMaterial;
        [FormerlySerializedAs("isLinkAnchored")] [SerializeField] private bool isBlockAnchored;
        [SerializeField] private Socket[] sockets;

        public Action<Chunk> OnBlockConnected = group => { };
        public Action<Chunk> OnBlockDisconnected = group => { };

        public Chunk Group
        {
            get => group;
            set
            {
                if (group) OnBlockDisconnected(group);
                group = value;
                if (group) OnBlockConnected(group);
            }
        }

        private HashSet<Block> connections
        {
            get
            {
                var set = new HashSet<Block>();
                for (var i = 0; i < sockets.Length; i++)
                {
                    var socket = sockets[i];
                    if (socket.ConnectedSocket != null)
                    {
                        set.Add(socket.ConnectedSocket.Owner);
                    }
                }

                return set;
            }
        }

        public List<Block> Connections => connections.ToList();
        public List<Socket> Sockets
        {
            get => sockets.ToList();
        }

        private void Awake()
        {
            FindSockets();
        }

        public void FindSockets()
        {
            sockets = GetComponentsInChildren<Socket>();
        }

        public HashSet<Block> GetBlocksInGroup()
        {
            var allConnections = new HashSet<Block>();
            allConnections.Add(this);
            GetBlocksInGroup(this, allConnections, null);
            return allConnections;
        }
        
        public HashSet<Block> GetBlocksInGroup(ISet<Block> ignore)
        {
            var allConnections = new HashSet<Block>();
            allConnections.Add(this);
            GetBlocksInGroup(this, allConnections, ignore);
            return allConnections;
        }

        private void GetBlocksInGroup(Block parent, HashSet<Block> allConnections, ISet<Block> ignore)
        {
            foreach (var connection in parent.connections)
            {
                if (ignore != null && ignore.Contains(connection))
                {
                    continue;                    
                }
                
                if (!allConnections.Contains(connection))
                {
                    allConnections.Add(connection);
                    GetBlocksInGroup(connection, allConnections, ignore);
                }
            }
        }

        public HashSet<(Block,Block)> GetAllEdges()
        {
            var allConnections = new HashSet<(Block,Block)>();
            GetAllEdges(this, allConnections);
            return allConnections;
        }

        private void GetAllEdges(Block parent, HashSet<(Block,Block)> allConnections)
        {
            foreach (var connection in parent.connections)
            {
                var edge = (parent, connection);
                if (!allConnections.Contains(edge))
                {
                    allConnections.Add(edge);
                    GetAllEdges(connection, allConnections);
                }
            }
        }

        public bool IsChunkAnchored => GetBlocksInGroup().Any(l => l.isBlockAnchored);

        public bool IsBlockAnchored
        {
            get => isBlockAnchored;
            set => isBlockAnchored = value;
        }

        public float Mass
        {
            get
            {
                var collider = GetComponent<Collider>();
                if (collider)
                {
                    var boundsSize = collider.bounds.size;
                    return boundsSize.x * boundsSize.y * boundsSize.z * blockMaterial.Density;
                }

                return blockMaterial.Density;
            }
        }

        public BlockMaterial BlockMaterial
        {
            get => blockMaterial;
            set => blockMaterial = value;
        }
    }
}