using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Blocks;

public class Menu : MonoBehaviour
{
    [SerializeField] private Chunk[] blocks; // Массив блоков, который можно задать через Inspector
    public List<RectTransform> buttons;      // Список кнопок
    public Transform spawn;                  // Точка спавна
    public GameObject prefabToSpawn;         // Префаб для спавна
    private bool pressed;                    // Флаг, чтобы избежать многократного нажатия

    void Start()
    {
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }

    void Update()
    {
        // Обработка кнопок для спавна блоков (0-9 кнопки)
        for (int i = 0; i < 10; i++)
        {
            if (buttons[i].localScale.x != 1 && !pressed)
            {
                pressed = true;
                SpawnBlock(i);
                StartCoroutine(Reset());
                return;
            }
        }

        // Обработка кнопок для спавна чанков без изменения цвета (10-13 кнопки)
        for (int i = 10; i < 14; i++)
        {
            if (buttons[i].localScale.x != 1 && !pressed)
            {
                pressed = true;
                SpawnBlockWithoutColor(i);
                StartCoroutine(Reset());
                return;
            }
        }

        // Обработка кнопки для спавна префаба (14-я кнопка)
        if (buttons[14].localScale.x != 1 && !pressed)
        {
            pressed = true;
            SpawnPrefab();
            StartCoroutine(Reset());
            return;
        }

        // Обработка кнопки для выхода (15-я кнопка)
        if (buttons[15].localScale.x != 1 && !pressed)
        {
            pressed = true;
            Exit();
            return;
        }
    }

    // Спавним блок с случайным цветом
    private void SpawnBlock(int index)
    {
        if (index >= 0 && index < blocks.Length)
        {
            Vector3 newPosition = spawn.position;

            Chunk block = Instantiate(blocks[index], newPosition, Quaternion.identity);
            block.gameObject.SetActive(true);

            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.HSVToRGB(Random.value, 1, 1, false);
            mat.SetFloat("_Glossiness", 0.8f);

            foreach (var renderer in block.GetComponentsInChildren<Renderer>())
            {
                renderer.material = mat;
            }
        }
    }

    // Спавним блок без изменения цвета
    private void SpawnBlockWithoutColor(int index)
    {
        if (index >= 0 && index < blocks.Length)
        {
            Vector3 newPosition = spawn.position;

            Chunk block = Instantiate(blocks[index], newPosition, Quaternion.identity);
            block.gameObject.SetActive(true);
        }
    }

    // Спавним префаб в точке спавна
    private void SpawnPrefab()
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawn.position, Quaternion.identity);
        }
    }

    // Сброс флага после нажатия кнопки
    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(1);
        pressed = false;
    }
}
