using UnityEngine;

public abstract class BotAction {
	public abstract float Score();
	public abstract void Enter();
	public abstract void Tick();
	public abstract void Exit();
}
