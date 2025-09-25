using System.Collections.Generic;
using System.Text;

public class MobileDebug {
	private Dictionary<string, object> fields = new();
	public void Add(string key, object value) {
		fields.Add(key, value);
	}

	public override string ToString() {
		StringBuilder sb = new();
		foreach (KeyValuePair<string, object> kvp in fields) { sb.AppendLine($"{kvp.Key}: {kvp.Value:f2}"); }
		return sb.ToString();
	}
}
