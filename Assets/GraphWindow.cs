using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GraphWindow : EditorWindow {
	// A private class to hold data for a single graph line.
	private class GraphData {
		public List<float> measurements = new List<float>();
		public Color color;
	}

	// --- Static Fields for Data Management ---
	private static readonly Dictionary<string, GraphData> graphs = new Dictionary<string, GraphData>();
	private static int capacity = 250; // The maximum number of data points to show.
	private static int colorIndex = 0;
	private static readonly Color[] graphColors = {
		new Color(0.2f, 0.6f, 1.0f), Color.green, Color.yellow, Color.red, Color.magenta, Color.cyan
	};

	// --- Instance Fields for Window State ---
	private int _hoverVisualIndex = -1; // The index the mouse is hovering over, in the visual space of the graph (0 to capacity-1).

	/// <summary>
	/// Public method to add a data point to a named graph.
	// This can be called from any other editor or runtime script.
	/// </summary>
	public static void AddToGraph(string name, float msTime) {
		// If the graph doesn't exist, create it with a new color.
		if (!graphs.ContainsKey(name)) {
			graphs.Add(name, new GraphData {
				color = graphColors[colorIndex % graphColors.Length]
			});
			colorIndex++;
		}
		
		var graph = graphs[name];
		graph.measurements.Add(msTime);
		
		// Trim the list to maintain the capacity, removing the oldest entry.
		while (graph.measurements.Count > capacity) {
			graph.measurements.RemoveAt(0);
		}
	}

	// Standard EditorWindow setup
	[MenuItem("Window/Analysis/MS Graph")]
	public static void ShowWindow() { GetWindow<GraphWindow>("MS Graph"); }
	private void OnEnable() { EditorApplication.update += Repaint; } // Repaint continuously
	private void OnDisable() { EditorApplication.update -= Repaint; }

	void OnGUI() {
		// --- 1. Header Display ---
		// Calculate the maximum value across all visible data points to scale the graph vertically.
		float maxMs = 0f;
		foreach (var graph in graphs.Values) {
			if (graph.measurements.Count > 0) { maxMs = Mathf.Max(maxMs, graph.measurements.Max()); }
		}

		string header = $"Max: {maxMs:F2} ms | ";
		if (graphs.Count == 0) { 
			header = "Awaiting Data. . . (e.g., call GraphWindow.AddToGraph(\"MyTimer\", 16.6f))"; 
		} else {
			// Display the latest value for each graph.
			foreach (var pair in graphs) {
				float currentMs = pair.Value.measurements.LastOrDefault();
				header += $"{pair.Key}: {currentMs:F2} ms | ";
			}
		}
		EditorGUILayout.LabelField(header);

		// --- 2. Graph Drawing Area ---
		Rect graphRect = GUILayoutUtility.GetRect(10, 1000, 10, 1000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		EditorGUI.DrawRect(graphRect, new Color(0.1f, 0.1f, 0.1f));

		// --- 3. Draw Graph Lines ---
		foreach (var pair in graphs) {
			var graph = pair.Value;
			if (graph.measurements.Count < 2) continue;

			Handles.color = graph.color;
			Vector3[] points = new Vector3[graph.measurements.Count];
			for (int i = 0; i < graph.measurements.Count; i++) {
				// The x-coordinate is shifted to the right if the buffer isn't full yet.
				// This makes new data appear from the right and scroll left.
				float x = graphRect.x + ((i + capacity - graph.measurements.Count) / (float)(capacity - 1)) * graphRect.width;
				
				// The y-coordinate is normalized against the max value and flipped (since GUI Y is top-down).
				float yNormal = (graph.measurements[i] / (maxMs > 0 ? maxMs : 1f));
				float y = graphRect.y + graphRect.height * (1 - yNormal);
				points[i] = new Vector3(x, y, 0);
			}
			Handles.DrawAAPolyLine(2.0f, points);
		}
		
		// --- 4. Tooltip and Hover Logic ---
		HandleTooltip(graphRect);
	}
	
	private void HandleTooltip(Rect graphRect) {
		Event e = Event.current;
		Vector2 mousePos = e.mousePosition;

		// Check if the mouse is inside the graph area.
		if (graphRect.Contains(mousePos)) {
			// --- Map Mouse X to a Visual Index ---
			// 1. Find the normalized position (0 to 1) of the mouse within the graph rect.
			float normalizedX = (mousePos.x - graphRect.x) / graphRect.width;
			// 2. Map this normalized position to an integer index based on our capacity.
			_hoverVisualIndex = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (capacity - 1)), 0, capacity - 1);
			
			// --- Draw Vertical Line ---
			float lineX = graphRect.x + (_hoverVisualIndex / (float)(capacity - 1)) * graphRect.width;
			Handles.color = Color.white;
			Handles.DrawLine(new Vector3(lineX, graphRect.y), new Vector3(lineX, graphRect.yMax));
			
			// --- Prepare Tooltip Data ---
			var tooltipLines = new List<(string text, Color color)>();
			foreach (var pair in graphs) {
				var graph = pair.Value;
				
				// --- Translate Visual Index to Data Index ---
				// This is the reverse of the calculation used to draw the points.
				int dataIndex = _hoverVisualIndex - (capacity - graph.measurements.Count);
				
				// If the resulting data index is valid for this specific graph's list...
				if (dataIndex >= 0 && dataIndex < graph.measurements.Count) {
					float value = graph.measurements[dataIndex];
					tooltipLines.Add(($"{pair.Key}: {value:F2} ms", graph.color));
				}
			}
			
			// --- Draw Tooltip Box and Text ---
			if (tooltipLines.Count > 0) {
				float lineHeight = 15f;
				float padding = 5f;
				float width = 150f;
				float height = (tooltipLines.Count * lineHeight) + (padding * 2);
				
				// Position the tooltip box intelligently to avoid going off-screen.
				// It appears to the right of the cursor, unless that would push it off-screen, in which case it appears to the left.
				float tooltipX = (mousePos.x + 15 + width > position.width) ? mousePos.x - width - 15 : mousePos.x + 15;
				float tooltipY = Mathf.Clamp(mousePos.y, 0, position.height - height);

				Rect tooltipRect = new Rect(tooltipX, tooltipY, width, height);

				EditorGUI.DrawRect(tooltipRect, new Color(0.2f, 0.2f, 0.2f, 0.9f));
				
				// Draw each line of text with its corresponding graph color.
				for (int i = 0; i < tooltipLines.Count; i++) {
					var line = tooltipLines[i];
					Rect lineRect = new Rect(tooltipRect.x + padding, tooltipRect.y + padding + (i * lineHeight), tooltipRect.width - (padding * 2), lineHeight);
					var style = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = line.color } };
					GUI.Label(lineRect, line.text, style);
				}
			}
		} else {
			// If mouse is outside, reset the hover index.
			_hoverVisualIndex = -1;
		}

		// Since we subscribe to EditorApplication.update, this isn't strictly necessary,
		// but it's good practice for responsiveness if you were to remove the constant update.
		if (e.type == EventType.MouseMove) {
            Repaint();
        }
	}
}
