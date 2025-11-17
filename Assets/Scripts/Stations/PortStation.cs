using UnityEngine;
using PirateCoop;

/// <summary>
/// Port Station (Station 7): Opens the Port Purchase Screen (Screen 8).
/// </summary>
public class PortStation : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject portMenuPrefab;
    private PortMenu portMenuInstance;

    public void Interact(PlayerController player)
    {
        if (GameManager.Instance.CurrentState != GameState.AtPort)
        {
            return;
        }

        if (portMenuInstance == null)
        {
            GameObject menuObject = Instantiate(portMenuPrefab);
            portMenuInstance = menuObject.GetComponent<PortMenu>();
            portMenuInstance.Initialize(player, ShipController.Instance.ShipState);
        }
        portMenuInstance.gameObject.SetActive(true);
    }

    public void StopInteract() { }

    public bool CanInteract(PlayerController player)
    {
        return GameManager.Instance.CurrentState == GameState.AtPort;
    }

    public string GetInteractPrompt(PlayerController player)
    {
        if (GameManager.Instance.CurrentState != GameState.AtPort)
        {
            return "[Not At Port]";
        }
        return "Open Port Market (E)";
    }
}