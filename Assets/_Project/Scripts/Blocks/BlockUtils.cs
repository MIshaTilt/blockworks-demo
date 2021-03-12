using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blocks
{
    public class BlockUtils
    {
        public static Chunk ConnectBlocks((Socket ThisSocket, Socket OtherSocket)[] result)
        {
            // Connect The sockets
            foreach (var (thisSocket, otherSocket) in result)
            {
                thisSocket.Connect(otherSocket);
            }

            return ConnectGroups(result.First().ThisSocket.Block.GetAllConnectedBlocks());
        }

        private static Chunk ConnectGroups(IEnumerable<Block> blocks)
        {
            // Get all connected blocks
            var allGroups = blocks
                .Select(socket => socket.GetComponentInParent<Chunk>())
                .OrderByDescending(group => group.GetComponent<Rigidbody>().isKinematic)
                .Distinct()
                .ToList();

            var allBlocks = allGroups.SelectMany(b => b.transform.Children()).ToList();
        
            foreach (var group in allGroups)
            {
                foreach (var block in group.GetComponentsInChildren<Block>())
                {
                    block.Chunk = null;
                }
            }
            
            var newGroup = CreateGroup(allBlocks, allGroups[0].transform);

            foreach (var group in allGroups)
            {
                Object.DestroyImmediate(group.gameObject);
            }
            
            foreach (var block in newGroup.GetComponentsInChildren<Block>())
            {
                block.Chunk = newGroup;
            }
            
            return newGroup;
        }

        private static Chunk CreateGroup(IEnumerable<Component> blocks, Transform blockGroup)
        {
            // Reparent blocks
            var newParent = new GameObject();
            newParent.transform.SetParent(blockGroup.parent);
            newParent.transform.CopyLocalFrom(blockGroup);

            foreach (var link in blocks)
            {
                link.transform.SetParent(newParent.transform, true);

                // Snap position & rotation
                link.transform.localPosition = link.transform.localPosition.Snap(0.0001f);
                link.transform.localRotation = Quaternion.Euler(link.transform.localRotation.eulerAngles.Snap(45f));
            }

            // Create a new chunk
            var chnk = newParent.AddComponent<Chunk>();
            var snapper = newParent.AddComponent<ChunkSnapper>();

            var newRb = newParent.AddComponent<Rigidbody>();
            newRb.interpolation = RigidbodyInterpolation.Interpolate;
            newRb.isKinematic = chnk.IsAnchored;
            
            return chnk;
        }

        public static List<Chunk> DisconnectChunk(Chunk group, IEnumerable<Block> chunk)
        {
            if (chunk.None())
                return new List<Chunk>();

            var newBlocks = group
                .SplitBy(chunk.ToSet())
                .Select((part, i) => 
                {
                    var currentGroup = part.First().GetComponentInParent<Chunk>().gameObject;
                    var newGroup = CreateGroup(part, currentGroup.transform);;
                    newGroup.name = $"{group.name} [{i}]";
                    return newGroup;
                })
                .ToList();
            
            Object.Destroy(group.gameObject);

            return newBlocks;
        }
    }
}