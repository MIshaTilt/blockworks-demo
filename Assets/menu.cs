using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Oculus.Interaction;
using Blocks;

public class Menu : MonoBehaviour
{
    [SerializeField] private Chunk[] blocks; // Массив блоков, который можно задать через Inspector
    public List<RectTransform> buttons;      // Список кнопок
    public Transform spawn;                  // Точка спавна
    public GameObject prefabToSpawn;         // Префаб для 11-й кнопки
    private bool pressed;                    // Флаг, чтобы избежать многократного нажатия

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        // Обработка кнопок для спавна блоков (9 кнопок)
        for (int i = 0; i < 9; i++)
        {
            // Проверяем, не нажата ли кнопка и не было ли нажатия до этого
            if (buttons[i].localScale.x != 1 && !pressed)
            {
                pressed = true; // Устанавливаем флаг нажатия
                SpawnBlock(i);  // Спавним блок из массива по индексу
                StartCoroutine(Reset());
                return; // Выходим из метода, чтобы избежать дальнейшей обработки
            }
        }

        // Обработка кнопки для спавна префаба (11-я кнопка)
        if (buttons[10].localScale.x != 1 && !pressed)
        {
            pressed = true;
            SpawnPrefab();  // Спавним префаб
            StartCoroutine(Reset());
            return;
        }

        // Обработка кнопки для выхода (12-я кнопка)
        if (buttons[11].localScale.x != 1 && !pressed)
        {
            pressed = true;
            Exit(); // Закрытие приложения
        }
    }

    // Спавним блок с случайным цветом
    private void SpawnBlock(int index)
    {
        // Проверяем, что индекс допустим для массива
        if (index >= 0 && index < blocks.Length)
        {
            // Убираем смещение и спавним блок в одной и той же позиции
            Vector3 newPosition = spawn.position; // Используем только позицию спавна

            Chunk block = Instantiate(blocks[index], newPosition, Quaternion.identity);
            block.gameObject.SetActive(true);

            // Генерируем случайный цвет
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.HSVToRGB(Random.value, 1, 1, false);
            mat.SetFloat("_Glossiness", 0.8f);

            // Применяем новый материал ко всем рендерерам блока
            foreach (var renderer in block.GetComponentsInChildren<Renderer>())
            {
                renderer.material = mat;
            }
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
