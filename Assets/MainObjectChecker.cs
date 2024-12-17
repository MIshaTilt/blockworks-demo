using System.Collections.Generic;
using UnityEngine;

public class MainObjectChecker : MonoBehaviour
{
	// ������, ������� ����� ��������� ����� �������� ���� �������� ��������
	public GameObject replacementObject;
	private bool _carDone = false;

	void Update()
	{
		if (CheckAllChildConditions() && !_carDone)
		{
			ReplaceChildrenWithObject();
			_carDone = true;
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
		Destroy(gameObject);

		// ������� ����� ������ �� ����� ��������
		if (replacementObject != null)
		{
			Quaternion _rotation = new(-90f, 0f, -90f, 0);
			Instantiate(replacementObject, transform.position, transform.rotation, transform.parent);
		}
	}
}
