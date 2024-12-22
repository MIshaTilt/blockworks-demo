using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Blocks;

public class Menu : MonoBehaviour
{
    [SerializeField] private Chunk[] blocks; // ������ ������, ������� ����� ������ ����� Inspector
    public List<RectTransform> buttons;      // ������ ������
    public Transform spawn;                  // ����� ������
    public GameObject prefabToSpawn;         // ������ ��� ������
    private bool pressed;                    // ����, ����� �������� ������������� �������

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
        // ��������� ������ ��� ������ ������ (0-9 ������)
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

        // ��������� ������ ��� ������ ������ ��� ��������� ����� (10-13 ������)
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

        // ��������� ������ ��� ������ ������� (14-� ������)
        if (buttons[14].localScale.x != 1 && !pressed)
        {
            pressed = true;
            SpawnPrefab();
            StartCoroutine(Reset());
            return;
        }

        // ��������� ������ ��� ������ (15-� ������)
        if (buttons[15].localScale.x != 1 && !pressed)
        {
            pressed = true;
            Exit();
            return;
        }
    }

    // ������� ���� � ��������� ������
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

    // ������� ���� ��� ��������� �����
    private void SpawnBlockWithoutColor(int index)
    {
        if (index >= 0 && index < blocks.Length)
        {
            Vector3 newPosition = spawn.position;

            Chunk block = Instantiate(blocks[index], newPosition, Quaternion.identity);
            block.gameObject.SetActive(true);
        }
    }

    // ������� ������ � ����� ������
    private void SpawnPrefab()
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawn.position, Quaternion.identity);
        }
    }

    // ����� ����� ����� ������� ������
    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(1);
        pressed = false;
    }
}
