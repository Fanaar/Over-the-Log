using System;
using UnityEngine;

public class TriggerObjectActivator : MonoBehaviour
{
    [Serializable]
    public class TriggerAction
    {
        public GameObject targetObject;
        public bool setActive = true; // True = activate, False = deactivate
    }

    [Header("Objects to activate/deactivate when the Player enters")]
    public TriggerAction[] actions;

    private void OnTriggerEnter(Collider other)
    {
        // React only to the Player tag
        if (!other.CompareTag("Player")) return;

        foreach (var action in actions)
        {
            if (action.targetObject != null)
                action.targetObject.SetActive(action.setActive);
        }
    }
}
