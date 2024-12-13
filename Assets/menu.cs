using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Blocks;

public class Menu : MonoBehaviour
{
    [SerializeField] private Chunk[] blocks; // ������ ������, ������� ����� ������ ����� Inspector
    public List<RectTransform> buttons;      // ������ ������
    public Transform spawn;                  // ����� ������
    private bool pressed;                    // ����, ����� �������� ������������� �������

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
        // ��������� ������ ��� ������ ������ (9 ������)
        for (int i = 0; i < 9; i++)
        {
            // ���������, �� ������ �� ������ � �� ���� �� ������� �� �����
            if (buttons[i].localScale.x != 1 && !pressed)
            {
                pressed = true; // ������������� ���� �������
                SpawnBlock(i);  // ������� ���� �� ������� �� �������
                StartCoroutine(Reset());
                return; // ������� �� ������, ����� �������� ���������� ���������
            }
        }

        // ��������� ������ ��� ������ (10-� ������)
        if (buttons[9].localScale.x != 1 && !pressed)
        {
            pressed = true;
            Exit(); // �������� ����������
        }
    }

    // ������� ���� � ��������� ������
    private void SpawnBlock(int index)
    {
        // ���������, ��� ������ �������� ��� �������
        if (index >= 0 && index < blocks.Length)
        {
            // ������� �������� � ������� ���� � ����� � ��� �� �������
            Vector3 newPosition = spawn.position; // ���������� ������ ������� ������

            Chunk block = Instantiate(blocks[index], newPosition, Quaternion.identity);
            block.gameObject.SetActive(true);

            // ���������� ��������� ����
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.HSVToRGB(Random.value, 1, 1, false);
            mat.SetFloat("_Glossiness", 0.8f);

            // ��������� ����� �������� �� ���� ���������� �����
            foreach (var renderer in block.GetComponentsInChildren<Renderer>())
            {
                renderer.material = mat;
            }
        }
    }

    // ����� ����� ����� ������� ������
    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(1);
        pressed = false;
    }
}

