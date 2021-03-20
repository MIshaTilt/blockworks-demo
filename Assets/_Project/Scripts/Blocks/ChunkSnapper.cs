using Blocks.Previews;
using UnityEngine;

namespace Blocks
{
    public class ChunkSnapper : MonoBehaviour
    {
        private bool isSnapping;

        private ChunkPreview chunkPreview;

        public void BeginSnap()
        {
            if (isSnapping == false)
            {
                chunkPreview = ChunkPreviewFactory.Build(GetComponent<Chunk>());
                chunkPreview.BeginSnap();
                isSnapping = true;
            }
        }

        public void EndSnap()
        {
            if (isSnapping)
            {
                chunkPreview.EndSnap();
                DestroyImmediate(chunkPreview.gameObject);
                isSnapping = false;
            }
        }
    }
}