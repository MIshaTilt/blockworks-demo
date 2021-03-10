using UnityEngine;

namespace Blocks
{
    public class ChunkSnapper : MonoBehaviour
    {
        private bool isSnapping;
        
        private SnapPreview snapPreview;

        public bool IsSnapping => isSnapping;

        public void BeginSnap()
        {
            if (isSnapping == false)
            {
                snapPreview = new SnapPreviewFactory(GetComponent<Chunk>()).Build();
                snapPreview.BeginSnap();
                isSnapping = true;
            }
        }

        public void EndSnap()
        {
            if (isSnapping)
            {
                snapPreview.EndSnap();
                DestroyImmediate(snapPreview.gameObject);
                isSnapping = false;
            }
        }
    }
}