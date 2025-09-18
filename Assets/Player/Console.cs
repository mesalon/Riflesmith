using System;
using System.Diagnostics;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class Console : MonoBehaviour {
    [SerializeField] TextMeshProUGUI temp;
    public TextMeshProUGUI perm;
    static string tempText;
    static string permText;

    void Update() {
        temp.text = tempText;
        perm.text = permText;
    }

    void LateUpdate() {
        permText = tempText = ""; 
    }

    public static void Add(string message, bool permanent = true) {
        if(permanent) { permText += $"{message}\n"; }
        else { tempText += $"{message}\n"; }
    }
}
