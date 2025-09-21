using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DropPool", menuName = "Enemy/Drop Pool")]
public class DropPool : ScriptableObject {
	[SerializeField] public List<DropEntry> drops;
}