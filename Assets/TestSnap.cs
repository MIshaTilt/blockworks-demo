using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class TestSnap : MonoBehaviour
{
    private void Start()
    {
        foreach (var snapper in FindObjectsOfType<ChunkSnapper>())
        {
            if (snapper.name.ToLowerInvariant().Contains("snap"))
                snapper.BeginSnap();
        }
    }
}
