using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual int Interact(PlayerController player, Vector2 direction)
    {
        return 0;
    }
}