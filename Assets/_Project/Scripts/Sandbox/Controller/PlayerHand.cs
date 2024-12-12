using System.Collections;
using System.Linq;
using Blocks;
using Blocks.Builder;
using ElasticSea.Framework.Extensions;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Sandbox.Controller
{
	public class PlayerHand : MonoBehaviour
	{
		[SerializeField] private HandGrabInteractor _grabInteractor;
		// [SerializeField] private ChunkSpawner chunkSpawner;
		private bool grabbed = false;
		private Chunk chunkHeld;
		private Block blockHeld;
		private bool flag = false;

		private void Start()
		{
			StartCoroutine(CheckGrab());
		}
		private IEnumerator CheckGrab()
		{
			while (true)
			{
				if (_grabInteractor.IsGrabbing && !grabbed)
				{
					grabbed = true;
					GrabStart();
				}
				else if (!_grabInteractor.IsGrabbing && grabbed)
				{
					yield return new WaitForFixedUpdate();
					if (!_grabInteractor.IsGrabbing)
					{
						grabbed = false;
						GrabEnd();
					}
				}
				yield return null;
			}
		}
        private void Disconnect()
        {
            var blockCandidate = CheckForBlock();
            if (blockCandidate)
            {
                ChunkFactory.Disconnect(blockCandidate.Chunk, new[] { blockCandidate });
            }
        }
        private void Update()
		{
			if (chunkHeld)
			{
				var component = chunkHeld.GetComponent<Rigidbody>();
				component.isKinematic = true;
				chunkHeld.transform.position = transform.position;
				chunkHeld.transform.rotation = transform.rotation;
			}
			else if (flag)
			{
				flag = false;
				Debug.Log("Зашло в else");
				if (chunkHeld)
				{
					var component = chunkHeld.GetComponent<Rigidbody>();
					component.isKinematic = false;
					chunkHeld = null;
				}
			}
		}

		private void GrabStart()
		{

			Debug.Log("STARTED");
			Disconnect();
			var blockCandidate = CheckForChunk();
			

			if (blockCandidate)
			{
				chunkHeld = blockCandidate;
				chunkHeld.GetComponent<BuildPreviewManager>().StartPreview();
				flag = true;
			}
			else
				chunkHeld = null;
		}

		private void GrabEnd()
		{
			if (chunkHeld)
			{
				chunkHeld.GetComponent<Rigidbody>().isKinematic = false;
				chunkHeld.GetComponent<BuildPreviewManager>().StopPreview();
				flag = false;
			}

			chunkHeld = null;
		}

		private Chunk CheckForChunk()
		{;
			var blockCandidate = Physics.OverlapSphere(transform.position, 0.3f)
				.Where(c => c.GetComponent<Block>() )
				.Where(c => c.GetComponent<Block>().IsAnchored == false)
				.OrderBy(c => c.transform.position.Distance(transform.position))
				.FirstOrDefault();
			if (blockCandidate)
			{
				var chunk = blockCandidate.GetComponentInParent<Chunk>();
				if (chunk.GetComponent<Rigidbody>().isKinematic == false)
				{
					return chunk;
				}
			}
			return null;
		}

		private Block CheckForBlock()
		{
			var blockCandidate = Physics.OverlapSphere(transform.position, 0.3f)
				.Where(c => c.GetComponent<Block>() )
				.OrderBy(c => c.transform.position.Distance(transform.position))
				.FirstOrDefault();
			if (blockCandidate)
			{
				return blockCandidate.GetComponent<Block>();
			}

			return null;
		}
	}
}

