using System.Linq;
using Blocks;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Sandbox
{
    public class ChunkSpawner : MonoBehaviour
    {
        [SerializeField] private Chunk[] blocks;
    
        public void Spawn()
        {
            var element = Instantiate(blocks.RandomElement(),transform.position,Quaternion.identity);
            element.transform.position = transform.position;
        
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.HSVToRGB(Random.value, 1, 1, false);
            mat.SetFloat("_Glossiness", 0.8f);
        
            foreach (var r in element.GetComponentsInChildren<Renderer>())
            {
                r.materials = r.materials.Select(r => mat).ToArray();
            }
        }
    }
}
