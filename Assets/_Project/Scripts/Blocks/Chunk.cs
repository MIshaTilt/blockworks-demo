using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Blocks;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    public class Chunk : MonoBehaviour
    {
        private void Start()
        {
            var rb = GetComponent<Rigidbody>();
            rb.mass = GetComponentsInChildren<Block>().Sum(block => block.Mass);
            rb.ResetCenterOfMass();
        }

        public (Transform thisSocket, Transform otherSocket)[] GetConnections()
        {
            var sockets = transform.GetComponentsInChildren<Socket>();
            var connected = new Socket[sockets.Length];
            
            // Find all connected pairs
            for (var i = 0; i < sockets.Length; i++)
            {
                var socket = sockets[i];
                var candidate = socket.GetComponent<Socket>().Trigger();
                if (candidate.IsEmpty() == false)
                {
                    var closest = candidate
                        .OrderBy(o => o.transform.position.Distance(socket.transform.position))
                        .First();
                    
                    var sockets2 = closest;
                    if (sockets2.Type != socket.Type)
                    {
                        connected[i] = closest;
                    }
                }
            }

            // Sort connection for the closest one
            return sockets.Select((socket, i) =>
                {
                    if (connected[i] == null) return null;
                    var dist = sockets[i].transform.position.Distance(connected[i].transform.position);
                    return new {Index = i, Distance = dist};
                })
                .Where(it => it != null)
                .OrderBy(arg => arg.Distance)
                .Select(arg => (sockets[arg.Index].transform, connected[arg.Index].transform))
                .ToArray();
        }

        public List<ISet<Block>> SplitBy(ISet<Block> chunk)
        {
            var all = GetComponentInChildren<Block>().GetBlocksInGroup().ToList();
            var rest = all.Except(chunk).ToList();
            foreach (var link in rest)
            {
                foreach (var socket in link.Sockets)
                {
                    if (socket.ConnectedSocket != null)
                    {
                        if (chunk.Contains(socket.ConnectedSocket.Owner))
                        {
                            socket.Disconnect();
                        }
                    }
                }
            }
            var groups = new List<ISet<Block>>();
            
            groups.Add(chunk);
            
            while (rest.Any())
            {
                var first = rest.First();
                var groupA = first.GetBlocksInGroup(chunk);
                rest = rest.Except(groupA).ToList();
                groups.Add(groupA);
            }

            return groups;
        }

        private void OnDrawGizmosSelected()
        {
            var connections = GetConnections();
            for (var i = 0; i < connections.Length; i++)
            {
                var color = Color.white;
                var size = 0.01f;
                if (i == 0) color = Color.red;
                if (i == 1) color = Color.blue;
                if (i >= 2) size = 0.005f;
                
                var from = connections[i].thisSocket.transform.position;
                var to = connections[i].otherSocket.transform.position;
                
                Gizmos.color = color.SetAlpha(.5f);
                Gizmos.DrawSphere(from, size);
                Gizmos.DrawSphere(to, size);
                Gizmos.DrawLine(from, to);
            }

            foreach (var (a, b) in GetComponentInChildren<Block>().GetAllEdges())
            {
                Gizmos.color = Color.yellow;

                var from = a.transform.TransformPoint(a.gameObject.GetCompositeMeshBounds().center);
                var to = b.transform.TransformPoint(b.gameObject.GetCompositeMeshBounds().center);
                Gizmos.DrawLine(from, to);
                Gizmos.DrawSphere(from, .0025f);
                Gizmos.DrawSphere(to, .0025f);
            }

            var centerOfMass = transform.InverseTransformPoint(GetComponent<Rigidbody>().centerOfMass);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(centerOfMass, .0025f);
        }
    }
}