using System.Collections.Generic;
using UnityEngine;

public class ChildObjectChecker : MonoBehaviour
{
    // Список объектов для проверки
    public List<GameObject> targetObjects;

    // Переменная, указывающая, найден ли целевой объект среди дочерних
    public bool isTargetChildFound = false;

    // Новая переменная: если установлено в true, то isTargetChildFound всегда true
    public bool alwaysTrue = false;

    // Статический список всех объектов, у которых найден нужный дочерний объект
    public static List<ChildObjectChecker> foundObjects = new List<ChildObjectChecker>();

    void Update()
    {
        // Проверяем состояние и обновляем переменную
        bool wasFound = isTargetChildFound;
        if (alwaysTrue)
        {
            isTargetChildFound = true;
        }
        else
        {
            isTargetChildFound = CheckForTargetChild();
        }

        // Если состояние изменилось, обновляем список foundObjects
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
        // Проверяем каждого ребёнка объекта
        foreach (Transform child in transform)
        {
            // Если дочерний объект содержится в списке targetObjects, возвращаем true
            if (targetObjects.Contains(child.gameObject))
            {
                return true;
            }
        }
        // Если ни один объект не найден, возвращаем false
        return false;
    }

    private void OnValidate()
    {
        // Проверка на корректность списка в инспекторе
        if (targetObjects == null)
        {
            targetObjects = new List<GameObject>();
        }
    }

    private void OnDestroy()
    {
        // Удаляем объект из списка, если он уничтожен
        if (foundObjects.Contains(this))
        {
            foundObjects.Remove(this);
        }
    }
}
