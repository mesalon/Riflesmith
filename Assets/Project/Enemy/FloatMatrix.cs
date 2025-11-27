using UnityEngine;
using System;

[Serializable]
public class FloatMatrix {
	[SerializeField] private int rows;
	[SerializeField] private int cols;
	[SerializeField] private float[] data;

	public int Rows => rows;
	public int Cols => cols;

	public float this[int row, int col] {
		get {
			if (row < 0 || row >= rows || col < 0 || col >= cols) { throw new IndexOutOfRangeException($"Matrix indices ({row},{col}) are out of range. Dimensions are ({rows},{cols})."); }
			return data[row * cols + col];
		}
		set {
			if (row < 0 || row >= rows || col < 0 || col >= cols) { throw new IndexOutOfRangeException($"Matrix indices ({row},{col}) are out of range. Dimensions are ({rows},{cols})."); }
			data[row * cols + col] = value;
		}
	}

	public FloatMatrix(int rows, int cols) {
		this.rows = rows;
		this.cols = cols;
		data = new float[rows * cols];
	}
}
