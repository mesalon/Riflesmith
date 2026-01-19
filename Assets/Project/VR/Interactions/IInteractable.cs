public interface IInteractable {
    public Interactor Interactor { get; set; }
    public bool PreventInteraction { get; set; }
    public void OnPicked();
    public void OnHold();
    public void OnHoldFixed();
    public void OnDropped();
}
