using UnityEngine;
using System;

public class GrabInteractable : MonoBehaviour {
	public Action OnPicked, OnHold, OnHoldFixed, OnDropped;
	public Transform grabPoint;
	public HandPoseObject pose;
}
