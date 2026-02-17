using Animancer;
using Animancer.Samples;
using UnityEngine;

public class PlayTransitionOnClick : MonoBehaviour
{
    [SerializeField] private AnimancerComponent _Animancer;
    [SerializeField] private ClipTransition _Idle;
    [SerializeField] private ClipTransition _Action;

		void Awake() {
			_Action.Events.OnEnd = OnEnable;
		}

    protected virtual void OnEnable() {
        _Animancer.Play(_Idle);
    }

    protected virtual void Update() {
        if (SampleInput.LeftMouseUp) {
            AnimancerState state = _Animancer.Play(_Action);
            state.Events(this).OnEnd ??= OnEnable;
        }
    }
}
