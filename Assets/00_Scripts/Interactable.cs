using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual bool Interact(PlayerController player, Vector2 direction)
    {
        return false;
    }
}