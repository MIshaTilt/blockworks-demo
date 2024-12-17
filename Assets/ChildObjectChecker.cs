using System.Collections.Generic;
using UnityEngine;

public class ChildObjectChecker : MonoBehaviour
{
    // ������ �������� ��� ��������
    public List<GameObject> targetObjects;

    // ����������, �����������, ������ �� ������� ������ ����� ��������
    public bool isTargetChildFound = false;

    // ����� ����������: ���� ����������� � true, �� isTargetChildFound ������ true
    public bool alwaysTrue = false;

    // ����������� ������ ���� ��������, � ������� ������ ������ �������� ������
    public static List<ChildObjectChecker> foundObjects = new List<ChildObjectChecker>();

    void Update()
    {
        // ��������� ��������� � ��������� ����������
        bool wasFound = isTargetChildFound;
        if (alwaysTrue)
        {
            isTargetChildFound = true;
        }
        else
        {
            isTargetChildFound = CheckForTargetChild();
        }

        // ���� ��������� ����������, ��������� ������ foundObjects
        if (isTargetChildFound && !wasFound)
        {
            foundObjects.Add(this);
        }
        else if (!isTargetChildFound && wasFound)
        {
            foundObjects.Remove(this);
        }
    }

    private bool CheckForTargetChild()
    {
        // ��������� ������� ������ �������
        foreach (Transform child in transform)
        {
            // ���� �������� ������ ���������� � ������ targetObjects, ���������� true
            if (targetObjects.Contains(child.gameObject))
            {
                return true;
            }
        }
        // ���� �� ���� ������ �� ������, ���������� false
        return false;
    }

    private void OnValidate()
    {
        // �������� �� ������������ ������ � ����������
        if (targetObjects == null)
        {
            targetObjects = new List<GameObject>();
        }
    }

    private void OnDestroy()
    {
        // ������� ������ �� ������, ���� �� ���������
        if (foundObjects.Contains(this))
        {
            foundObjects.Remove(this);
        }
    }
}
