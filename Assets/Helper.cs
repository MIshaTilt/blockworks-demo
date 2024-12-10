using System;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public HandGrabInteractor HandInteractor { get; set; }

    private List<(Action<bool>, bool)> interactionMappings = new List<(Action<bool>, bool)>();

    /// <summary>
    /// Регистрирует обработчик для событий взаимодействия с объектом (захват или отпускание).
    /// </summary>
    public void RegisterInteraction(Action<bool> action)
    {
        interactionMappings.Add((action, false));
    }

    private void Update()
    {
        // Проверяем текущее состояние взаимодействия руки с объектом.
        bool isGrabbing = HandInteractor != null && HandInteractor.HasInteractable;

        for (var i = 0; i < interactionMappings.Count; i++)
        {
            var (action, wasGrabbing) = interactionMappings[i];

            if (!wasGrabbing && isGrabbing)
            {
                // Переход в состояние "захват".
                interactionMappings[i] = (action, true);
                action(true); // Вызываем обработчик, передавая true.
            }
            else if (wasGrabbing && !isGrabbing)
            {
                // Переход в состояние "отпущено".
                interactionMappings[i] = (action, false);
                action(false); // Вызываем обработчик, передавая false.
            }
        }
    }
}
