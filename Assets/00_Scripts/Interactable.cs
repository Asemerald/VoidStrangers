using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void Interact(PlayerController player, Vector2 direction) {}
}