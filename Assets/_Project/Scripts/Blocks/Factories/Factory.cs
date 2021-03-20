using System;
using UnityEngine;

namespace Blocks.Factories
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] private string blockPrefabPath;

        [SerializeField] private Mesh blockMesh;
        [SerializeField] private Mesh slopeBlockMesh;
        [SerializeField] private Mesh pinMesh;
        [SerializeField] private Material material;
        [SerializeField] private BlockMaterial blockMaterial;

        public GameObject Build(BlockTemplate template)
        {
            var chunkGo = new GameObject(template.Name);
            var chunk = chunkGo.AddComponent<Chunk>();
            var go = new GameObject(template.Name);

            var block = go.AddComponent<Block>();
            block.Chunk = chunk;
            block.BlockMaterial = blockMaterial;

            go.AddComponent<MeshFilter>();

            switch (template.Strategy)
            {
                case BlockTemplate.BlockStrategy.Basic:
                    new BasicStrategy(blockMesh, pinMesh, template.Size).Build(block);
                    break;
                case BlockTemplate.BlockStrategy.Slope:
                    new SlopeStrategy(slopeBlockMesh, pinMesh, template.Size, template.Offset).Build(block);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var mr = go.AddComponent<MeshRenderer>();

            var mf = go.AddComponent<MeshFilter>();


            foreach (var mr2 in go.GetComponentsInChildren<MeshRenderer>())
            {
                mr2.material = material;
            }


            go.transform.SetParent(chunkGo.transform, false);

            var rb = chunkGo.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var snapper = chunkGo.AddComponent<ChunkSnapper>();

            return chunkGo;
        }
    }
}