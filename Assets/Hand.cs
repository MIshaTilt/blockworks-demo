using System.Linq;
using Blocks;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using UnityEngine.XR;

public class Hand : MonoBehaviour
{
    private bool triggerHeld;
    private Chunk chunkHeld;

    private void Update()
    {
        var leftHandedDevices = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        var device = leftHandedDevices.GetDevice();


        if (chunkHeld)
        {
            var component = chunkHeld.GetComponent<Rigidbody>();
            component.isKinematic = true;
            component.MovePosition(transform.position);
            component.MoveRotation(transform.rotation);
        }
        
        
        if (triggerHeld == false)
        {
            if (device.GetFeatureValue(CommonUsages.triggerButton) == true)
            {
                triggerHeld = true;
                GrabStart();
            }
        }
        else
        {
            if (device.GetFeatureValue(CommonUsages.triggerButton) == false)
            {
                triggerHeld = false;
                GrabEnd();
            }
        }
    }

    private void GrabStart()
    {
        var blockCandidate = Physics.OverlapSphere(transform.position, 0.05f)
            .Where(c => c.GetComponent<Block>() )
            .Where(c => c.GetComponent<Block>().IsAnchored == false)
            .OrderBy(c => c.transform.position.Distance(transform.position))
            .FirstOrDefault();

        if (blockCandidate)
        {
            var chunk = blockCandidate.GetComponentInParent<Chunk>();
            if (chunk.GetComponent<Rigidbody>().isKinematic == false)
            {
                chunkHeld = chunk;
                chunkHeld.GetComponent<ChunkSnapper>().BeginSnap();
            }
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
            chunkHeld.GetComponent<ChunkSnapper>().EndSnap();
        }

        chunkHeld = null;
    }
}
