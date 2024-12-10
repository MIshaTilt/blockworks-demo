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
    /// ������������ ���������� ��� ������� �������������� � �������� (������ ��� ����������).
    /// </summary>
    public void RegisterInteraction(Action<bool> action)
    {
        interactionMappings.Add((action, false));
    }

    private void Update()
    {
        // ��������� ������� ��������� �������������� ���� � ��������.
        bool isGrabbing = HandInteractor != null && HandInteractor.HasInteractable;

        for (var i = 0; i < interactionMappings.Count; i++)
        {
            var (action, wasGrabbing) = interactionMappings[i];

            if (!wasGrabbing && isGrabbing)
            {
                // ������� � ��������� "������".
                interactionMappings[i] = (action, true);
                action(true); // �������� ����������, ��������� true.
            }
            else if (wasGrabbing && !isGrabbing)
            {
                // ������� � ��������� "��������".
                interactionMappings[i] = (action, false);
                action(false); // �������� ����������, ��������� false.
            }
        }
    }
}
