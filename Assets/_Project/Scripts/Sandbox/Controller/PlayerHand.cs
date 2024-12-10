using System;
using System.Linq;
using Blocks;
using Blocks.Builder;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Sandbox.Controller
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] private HandGrabInteractor handGrabInteractor;
        [SerializeField] private Renderer sphere;
        [SerializeField] private ChunkSpawner chunkSpawner;

        private Helper handHelper;
        private Chunk chunkHeld;

        private enum State
        {
            Grab,
            Disconnect
        }

        [SerializeField] private State state;

        private void Start()
        {
            // Настройка HandHelper
            handHelper = gameObject.AddComponent<Helper>();
            handHelper.HandInteractor = handGrabInteractor;

            handHelper.RegisterInteraction(isGrabbing =>
            {
                switch (state)
                {
                    case State.Grab:
                        if (isGrabbing)
                        {
                            GrabStart();
                        }
                        else
                        {
                            GrabEnd();
                        }
                        break;

                    case State.Disconnect:
                        if (!isGrabbing)
                        {
                            Disconnect();
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            UpdateState(); // Установить начальное состояние
        }

        private void Update()
        {
            if (chunkHeld)
            {
                // Привязка удерживаемого кубика к руке
                var component = chunkHeld.GetComponent<Rigidbody>();
                component.isKinematic = true;
                chunkHeld.transform.position = handGrabInteractor.transform.position;
                chunkHeld.transform.rotation = handGrabInteractor.transform.rotation;
            }

            // Обновляем состояние
            UpdateState();
        }

        private void UpdateState()
        {
            switch (state)
            {
                case State.Grab:
                    sphere.material.color = Color.green;
                    break;

                case State.Disconnect:
                    sphere.material.color = Color.red;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GrabStart()
        {
            var blockCandidate = CheckForChunk();

            if (blockCandidate)
            {
                chunkHeld = blockCandidate;
                chunkHeld.GetComponent<BuildPreviewManager>().StartPreview();
            }
            else
            {
                chunkHeld = null;
            }
        }

        private void GrabEnd()
        {
            if (chunkHeld)
            {
                chunkHeld.GetComponent<Rigidbody>().isKinematic = false;
                chunkHeld.GetComponent<BuildPreviewManager>().StopPreview();
            }

            chunkHeld = null;
        }

        private void Disconnect()
        {
            var blockCandidate = CheckForBlock();
            if (blockCandidate)
            {
                ChunkFactory.Disconnect(blockCandidate.Chunk, new[] { blockCandidate });
            }
        }

        private Chunk CheckForChunk()
        {
            var blockCandidate = Physics.OverlapSphere(handGrabInteractor.transform.position, 0.05f)
                .Where(c => c.GetComponent<Block>())
                .Where(c => !c.GetComponent<Block>().IsAnchored)
                .OrderBy(c => Vector3.Distance(c.transform.position, handGrabInteractor.transform.position))
                .FirstOrDefault();


            if (blockCandidate)
            {
                var chunk = blockCandidate.GetComponentInParent<Chunk>();
                if (!chunk.GetComponent<Rigidbody>().isKinematic)
                {
                    return chunk;
                }
            }

            return null;
        }

        private Block CheckForBlock()
        {
            var blockCandidate = Physics.OverlapSphere(handGrabInteractor.transform.position, 0.05f)
                .Where(c => c.GetComponent<Block>())
                .OrderBy(c => Vector3.Distance(c.transform.position, handGrabInteractor.transform.position))
                .FirstOrDefault();

            return blockCandidate ? blockCandidate.GetComponent<Block>() : null;
        }
    }
}

