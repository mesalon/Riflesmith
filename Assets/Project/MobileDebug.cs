using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MobileDebug {
	public readonly Dictionary<string, Queue<float>> fields = new();
	private readonly int smoothingWindowSize;

	public MobileDebug(int smoothingWindowSize = 1) {
		this.smoothingWindowSize = Math.Max(1, smoothingWindowSize);
	}

	public void Add(string key, object value) {
		float numericValue;
		try { numericValue = Convert.ToSingle(value); } 
		catch (FormatException) { return; }

		if (!fields.TryGetValue(key, out Queue<float> valueQueue)) {
			valueQueue = new Queue<float>();
			fields[key] = valueQueue;
		}
		valueQueue.Enqueue(numericValue);
		while (valueQueue.Count > smoothingWindowSize) { valueQueue.Dequeue(); }
	}

	public override string ToString() {
		StringBuilder sb = new();
		foreach (KeyValuePair<string, Queue<float>> kvp in fields) {
			if (kvp.Value.Any()) {
				sb.AppendLine($"{kvp.Key}: {kvp.Value.Average():F4}");
			}
		}
		return sb.ToString();
	}
}
