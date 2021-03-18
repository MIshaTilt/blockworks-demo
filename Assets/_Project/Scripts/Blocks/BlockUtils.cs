using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blocks
{
    public static class BlockUtils
    {
        public static Chunk ConnectBlocks((Socket ThisSocket, Socket OtherSocket)[] socketPair)
        {
            var chunks = new HashSet<Chunk>();
            foreach (var (thisSocket, otherSocket) in socketPair)
            {
                thisSocket.Connect(otherSocket);
            }

            foreach (var (thisSocket, otherSocket) in socketPair)
            {
                chunks.Add(thisSocket.Block.Chunk);
                chunks.Add(otherSocket.Block.Chunk);
            }

            var targetChunk = chunks.FirstOrDefault(c => c.IsAnchored) ?? chunks.First();

            foreach (var chunk in chunks)
            {
                if (chunk != targetChunk)
                {
                    foreach (var block in chunk.Blocks)
                    {
                        block.transform.SetParent(targetChunk.transform, true);
                        FixBlockOffset(block);
                        block.Chunk = targetChunk;
                    }

                    Object.Destroy(chunk.gameObject);
                }
            }

            return targetChunk;
        }

        public static void FixBlockOffset(Block block)
        {
            var transform = block.transform;
            transform.localPosition = transform.localPosition.RoundTo(0.05f, 0.02f, 0.05f);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.RoundTo(90, 90, 90));
        }

        public static List<Chunk> DisconnectChunk(Chunk group, IEnumerable<Block> chunk)
        {
            if (chunk.None())
                return new List<Chunk>();

            var newBlocks = group
                .SplitBy(chunk.ToSet())
                .Select((part, i) => 
                {
                    var newGroup = CreateGroup(part);
                    newGroup.name = $"{group.name} [{i}]";
                    return newGroup;
                })
                .ToList();
            
            Object.Destroy(group.gameObject);

            return newBlocks;
        }

        private static Chunk CreateGroup(IEnumerable<Block> blocks)
        {
            // Reparent blocks
            var newParent = new GameObject();
            newParent.transform.position = blocks.First().transform.position;
            newParent.transform.rotation = blocks.First().transform.rotation;

            var chnk = newParent.AddComponent<Chunk>();
            
            foreach (var link in blocks)
            {
                link.transform.SetParent(newParent.transform, true);
                FixBlockOffset(link);
                link.Chunk = chnk;
            }

            // Create a new chunk
            var snapper = newParent.AddComponent<ChunkSnapper>();

            var newRb = newParent.AddComponent<Rigidbody>();
            newRb.interpolation = RigidbodyInterpolation.Interpolate;
            newRb.isKinematic = chnk.IsAnchored;
            
            return chnk;
        }
    }
}