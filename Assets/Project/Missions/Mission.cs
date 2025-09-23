using UnityEngine;

public abstract class Mission {
	public abstract bool IsComplete { get; }
	public abstract void Tick();
	public abstract void Exit();
	public abstract Vector3 CompassPosition { get; }
}