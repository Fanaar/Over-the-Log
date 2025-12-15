using System.Collections.Generic;
using UnityEngine;

public class RabbitSettlementManager : MonoBehaviour
{
    [Header("Hond Settings")]
    public DogController2 dog;
    public int rabbitsToSettleForDog = 3; // aantal konijnen voordat hond verschijnt

    private HashSet<RabbitRunAwayController> settledRabbits = new HashSet<RabbitRunAwayController>();
    private bool dogActivated = false;

    // Wordt door elk konijn aangeroepen
    public void RabbitSettled(RabbitRunAwayController rabbit)
    {
        if (settledRabbits.Contains(rabbit))
            return; // voorkom dubbel tellen

        settledRabbits.Add(rabbit);
        Debug.Log("Konijnen gesettled: " + settledRabbits.Count);

        if (!dogActivated && settledRabbits.Count >= rabbitsToSettleForDog)
        {
            if (dog != null)
            {
                dog.gameObject.SetActive(true);
                dogActivated = true;
                Debug.Log("Hond geactiveerd!");
            }
        }
    }
}
