using System.Collections.Generic;
using UnityEngine;

public class MainObjectChecker : MonoBehaviour
{
    // Объект, который будет заспавнен после удаления всех дочерних объектов
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
                return false; // Если хотя бы один дочерний объект не удовлетворяет условию
            }
        }
        return true; // Все дочерние объекты удовлетворяют условию
    }

    private void ReplaceChildrenWithObject()
    {
        // Создаем временный список дочерних объектов, чтобы избежать ошибок изменения коллекции во время итерации
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        // Удаляем всех дочерних объектов
        foreach (Transform child in children)
        {
            Destroy(child.gameObject);
        }

        // Спавним новый объект на месте родителя
        if (replacementObject != null)
        {
            Instantiate(replacementObject, transform.position, transform.rotation, transform.parent);
        }
    }
}
