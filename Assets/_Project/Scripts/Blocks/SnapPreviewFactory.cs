using System;
using System.Collections.Generic;
using _Project.Scripts.Blocks;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    public class SnapPreviewFactory
    {
        private Material material;
        private Chunk Chunk;
        private Dictionary<Socket, Socket> cloneToBlockMapping;

        public SnapPreviewFactory(Chunk Chunk)
        {
            this.Chunk = Chunk;
            
            material = new Material(Shader.Find("Standard"));
            material.SetupMaterialWithBlendMode(MaterialExtensions.Mode.Fade);
            
            cloneToBlockMapping = new Dictionary<Socket, Socket>();
        }

        public SnapPreview Build()
        {
            var preview = new GameObject(Chunk.name + " Preview");
            CopyTree(Chunk.transform, preview.transform);

            var component = preview.AddComponent<Rigidbody>();
            component.isKinematic = true;
            component.interpolation = RigidbodyInterpolation.Interpolate;
         
            var blockClone = preview.AddComponent<SnapPreview>();
            blockClone.Owner = Chunk;
            blockClone.Renderers = preview.GetComponentsInChildren<Renderer>();
            blockClone.Material = material;
            blockClone.CloneToBlockMapping = cloneToBlockMapping;
            blockClone.Visible = false;
            blockClone.gameObject.SetActive(false);

            return blockClone;
        }
        
        private void CopyTree(Transform from, Transform to)
        {
            CopyComponents(from, to);
            
            foreach (Transform fromChild in from)
            {
                var toChild = new GameObject().transform;
                toChild.CopyLocalFrom(fromChild);
                toChild.SetParent(to, false);
                toChild.name = fromChild.name;

                CopyTree(fromChild, toChild);
            }
        }

        private void CopyComponents(Transform from, Transform to)
        {
            CopyCollider(from, to);
            CopyMeshFilter(from, to);
            CopyRenderer(from, to);
            CopySocket(from, to);
        }

        private void CopySocket(Transform from, Transform to)
        {
            var fromSocket = from.GetComponent<Socket>();
            if (fromSocket)
            {
                var toSocket = to.gameObject.AddComponent<Socket>();
                toSocket.Type = fromSocket.Type;
                toSocket.Owner = fromSocket.Owner;
                toSocket.Active = false;
                cloneToBlockMapping[toSocket] = fromSocket;
            }
        }

        private void CopyRenderer(Transform from, Transform to)
        {
            var fromRenderer = from.GetComponent<MeshRenderer>();
            if (fromRenderer)
            {
                var toRenderer = to.gameObject.AddComponent<MeshRenderer>();

                var toMats = new Material[fromRenderer.materials.Length];
                for (var i = 0; i < toMats.Length; i++)
                {
                    toMats[i] = material;
                }

                toRenderer.materials = toMats;
            }
        }

        private static void CopyMeshFilter(Transform from, Transform to)
        {
            var fromMf = from.GetComponent<MeshFilter>();
            if (fromMf)
            {
                var toMf = to.gameObject.AddComponent<MeshFilter>();
                toMf.mesh = fromMf.mesh;
            }
        }

        private static void CopyCollider(Transform from, Transform to)
        {
            var isBlock = from.GetComponent<Block>() == true;
            if (isBlock)
            {
                var collider = from.GetComponent<Collider>();
                if (collider is BoxCollider == false)
                {
                    throw new InvalidOperationException("Only box colliders are supported at this time.");
                }

                var fromBox = collider as BoxCollider;

                var toBox = to.gameObject.AddComponent<BoxCollider>();
                toBox.center = fromBox.center;
                toBox.isTrigger = true;
                toBox.size = fromBox.size - Vector3.one * .001f;
            }
        }
    }
}