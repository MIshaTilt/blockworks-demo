using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class TestSnap : MonoBehaviour
{
    private void Start()
    {
        foreach (var tag in FindObjectsOfType<TestSnapTag>())
        {
            switch (tag.type)
            {
                case SnapType.Preview:
                    tag.GetComponent<ChunkSnapper>().BeginSnap();
                    break;
                case SnapType.Snap:
                    tag.GetComponent<ChunkSnapper>().BeginSnap();
                    tag.GetComponent<ChunkSnapper>().EndSnap();
                    break;
                case SnapType.Disconnect:
                    var block = tag.GetComponentInChildren<Block>();
                    tag.GetComponent<ChunkSnapper>().BeginSnap();
                    tag.GetComponent<ChunkSnapper>().EndSnap();
                    ChunkFactory.Disconnect(block.Chunk, new[] {block});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
