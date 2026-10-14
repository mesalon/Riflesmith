[System.Serializable] public abstract class AIAction {
	public abstract float GetScore();
	public abstract void Enter();
	public abstract void Tick();
	public abstract void Exit();
}
