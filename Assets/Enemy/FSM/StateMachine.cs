using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine {
	public IState currentState;

	private Dictionary<Type, List<Transition>> _transitions = new();
	private List<Transition> _currentTransitions = new();
	private List<Transition> _anyTransitions = new();
	private static List<Transition> EmptyTransitions = new(0);

	public void Tick() {
		Transition transition = GetTransition();
		if (transition != null)
			SetState(transition.To);

		currentState?.Tick();
	}

	public void SetState(IState state) {
		if (state == currentState)
			return;
		if (state == null) {
			currentState = null;
			return;
		}
		currentState?.OnExit();
		currentState = state;

		_transitions.TryGetValue(currentState.GetType(), out _currentTransitions);
		if (_currentTransitions == null)
			_currentTransitions = EmptyTransitions;

		currentState.OnEnter();
	}

	public void AddTransition(IState from, IState to, Func<bool> predicate) {
		if (_transitions.TryGetValue(from.GetType(), out List<Transition> transitions) == false) {
			transitions = new();
			_transitions[from.GetType()] = transitions;
		}

		transitions.Add(new Transition(to, predicate));
	}

	public void AddAnyTransition(IState state, Func<bool> predicate) {
		_anyTransitions.Add(new(state, predicate));
	}

	private class Transition {
		public Func<bool> Condition { get; }
		public IState To { get; }

		public Transition(IState to, Func<bool> condition) {
			To = to;
			Condition = condition;
		}
	}

	private Transition GetTransition() {
		foreach (Transition transition in _anyTransitions) {
			if (transition.Condition()) return transition;
		}
			

		foreach (Transition transition in _currentTransitions)
			if (transition.Condition()) return transition;

		return null;
	}
}