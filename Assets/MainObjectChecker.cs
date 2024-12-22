using System.Collections.Generic;
using UnityEngine;

public class MainObjectChecker : MonoBehaviour
{
    // Объект, который будет использоваться для замены текущего объекта
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
                return false; // Если хотя бы один из детей не соответствует условиям
            }
        }
        return true; // Все дети соответствуют условиям
    }

    private void ReplaceChildrenWithObject()
    {
        // Сохраняем ссылки на всех детей
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        // Уничтожаем текущий объект
        Destroy(gameObject);

        // Создаём новый объект в позиции (0, 1, 0)
        if (replacementObject != null)
        {
            Instantiate(replacementObject, new Vector3(0, 1, 0), Quaternion.identity);
        }
    }
}
