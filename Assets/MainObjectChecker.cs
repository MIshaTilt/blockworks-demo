using System.Collections.Generic;
using UnityEngine;

public class MainObjectChecker : MonoBehaviour
{
    // ������, ������� ����� ��������� ����� �������� ���� �������� ��������
    public GameObject replacementObject;

    void Update()
    {
        if (CheckAllChildConditions())
        {
            ReplaceChildrenWithObject();
        }
    }

    private bool CheckAllChildConditions()
    {
        foreach (Transform child in transform)
        {
            ChildObjectChecker checker = child.GetComponent<ChildObjectChecker>();
            if (checker == null || !checker.isTargetChildFound)
            {
                return false; // ���� ���� �� ���� �������� ������ �� ������������� �������
            }
        }
        return true; // ��� �������� ������� ������������� �������
    }

    private void ReplaceChildrenWithObject()
    {
        // ������� ��������� ������ �������� ��������, ����� �������� ������ ��������� ��������� �� ����� ��������
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        // ������� ���� �������� ��������
        foreach (Transform child in children)
        {
            Destroy(child.gameObject);
        }

        // ������� ����� ������ �� ����� ��������
        if (replacementObject != null)
        {
            Instantiate(replacementObject, transform.position, transform.rotation, transform.parent);
        }
    }
}
