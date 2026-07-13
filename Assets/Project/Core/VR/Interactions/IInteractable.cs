using System.Collections.Generic;
using UnityEngine;

public abstract class Interactabledeprecated : MonoBehaviour {
    public List<Hand> interactors = new();
    public bool preventInteraction;
    public abstract void OnPicked();
    public abstract void OnHold();
    public abstract void OnHoldFixed();
    public abstract void OnDropped();
}
