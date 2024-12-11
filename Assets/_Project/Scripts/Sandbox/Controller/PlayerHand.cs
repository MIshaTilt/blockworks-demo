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

		private void Update()
		{
			if (chunkHeld)
			{
				var component = chunkHeld.GetComponent<Rigidbody>();
				component.isKinematic = true;
				chunkHeld.transform.position = transform.position;
				chunkHeld.transform.rotation = transform.rotation;
			}
		}

		private void GrabStart()
		{
			Debug.Log("STARTED");
			var blockCandidate = CheckForChunk();

			chunkHeld = blockCandidate;

			chunkHeld.GetComponent<BuildPreviewManager>().StartPreview();
		}

		private void GrabEnd()
		{
			Debug.Log("ENDED");
			chunkHeld.GetComponent<Rigidbody>().isKinematic = false;
			chunkHeld.GetComponent<BuildPreviewManager>().StopPreview();
		}

		private Chunk CheckForChunk()
		{;
			var blockCandidate = Physics.OverlapSphere(transform.position, 0.05f)
				.Where(c => c.GetComponent<Block>() )
				.Where(c => c.GetComponent<Block>().IsAnchored == false)
				.OrderBy(c => c.transform.position.Distance(transform.position))
				.FirstOrDefault();
			var chunk = blockCandidate.GetComponentInParent<Chunk>();
			if (chunk.GetComponent<Rigidbody>().isKinematic == false)
			{
				return chunk;
			}
	  		return null;
		}

		private Block CheckForBlock()
		{
			var blockCandidate = Physics.OverlapSphere(transform.position, 0.05f)
				.Where(c => c.GetComponent<Block>() )
				.OrderBy(c => c.transform.position.Distance(transform.position))
				.FirstOrDefault();
			if (blockCandidate != null)
				return blockCandidate.GetComponent<Block>();
			return null;
		}
	}
}

