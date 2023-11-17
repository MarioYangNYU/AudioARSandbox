using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour {
    public float speedFactor = 1f;
    private Queue<string> dataQueue = new Queue<string>();

    void Start() {
        COMManager.Singleton.OnDataReceived += OnDataReceived;
    }

    void Update() {
        if (dataQueue.Count > 0) {
            string data = dataQueue.Dequeue();
            ProcessData(data);
        }
    }

    void OnDisable() {
        COMManager.Singleton.OnDataReceived -= OnDataReceived;
    }

    public void OnDataReceived(string data) {
        dataQueue.Enqueue(data);
    }

    private void ProcessData(string data) {
        Debug.Log("Processing data: " + data);
        string[] values = data.Split(',');
        if (values.Length == 4) {
            float w = float.Parse(values[0]);
            float x = float.Parse(values[1]);
            float y = float.Parse(values[2]);
            float z = float.Parse(values[3]);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, new Quaternion(w, y, x, z), Time.deltaTime * speedFactor);
        }
        else if (values.Length != 4) {
            Debug.LogWarning(data);
        }
    }
}
