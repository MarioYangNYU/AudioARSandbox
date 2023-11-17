using UnityEngine;
using System.Collections.Generic;
using System.IO.Ports;
using TMPro;
using UnityEngine.UI;
using System.Threading;
using System;

public class COMManager : MonoBehaviour {
    public List<string> comPorts = new List<string>();
    public string selectedPort = "COM1";
    public int baudRate = 38400;

    public TMP_Dropdown COMDropdown;
    public TMP_InputField baudRateInput;
    public Button refreshCOMButton;
    public Button connectButton;
    public string receivedData = "";
    public TextMeshProUGUI receivedDataText;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isReading = false;

    public static COMManager Singleton;

    public event Action<string> OnDataReceived;

    private void Awake() {
        if (Singleton == null) {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else {
            Destroy(this.gameObject);
        }
    }

    void Start() {
        RefreshCOMPortList();
        baudRateInput.text = baudRate.ToString();
        refreshCOMButton.onClick.AddListener(RefreshCOMPortList);
        connectButton.onClick.AddListener(ToggleCOMConnection);
    }

    private void Update() {
        lock (this) { 
            if (receivedData != "") {
                receivedDataText.text = receivedData;
                receivedData = ""; 
            }
        }
    }

    private void RefreshCOMPortList() {
        comPorts.Clear();
        string[] ports = SerialPort.GetPortNames();

        foreach (string port in ports) {
            comPorts.Add(port);
            Debug.Log("Found COM Port: " + port);
        }

        UpdateCOMDropdown();
    }

    private void UpdateCOMDropdown() {
        COMDropdown.ClearOptions();
        COMDropdown.AddOptions(comPorts);

        COMDropdown.value = comPorts.Count - 1;

    }

    private void ToggleCOMConnection() {
        selectedPort = COMDropdown.options[COMDropdown.value].text;
        if (serialPort != null && serialPort.IsOpen) {
            CloseCOMPort();
        }
        else {
            ConnectToCOM();
        }
    }

    private void ConnectToCOM() {
        if (!int.TryParse(baudRateInput.text, out baudRate)) {
            Debug.LogError("Invalid baud rate entered");
            return;
        }

        serialPort = new SerialPort(selectedPort, baudRate);
        serialPort.ReadTimeout = 50;

        try {
            serialPort.Open();
            StartReading();
            Debug.Log($"Connected to {selectedPort} with baud rate {baudRate}");
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to open {selectedPort}: {e.Message}");
        }
    }

    private void StartReading() {
        if (serialPort != null && serialPort.IsOpen) {
            readThread = new Thread(ReadData);
            isReading = true;
            readThread.Start();
        }
    }

    private void ReadData() {
        while (isReading && serialPort != null && serialPort.IsOpen) {
            try {
                string data = serialPort.ReadLine();
                if (data != null) {
                    //Debug.Log("Received: " + data);
                    receivedData = data;
                    OnDataReceived?.Invoke(data);
                }
            }
            catch (TimeoutException) {
            }
        }
    }

    private void CloseCOMPort() {
        if (isReading) {
            isReading = false;
            readThread.Join();
        }

        if (serialPort != null && serialPort.IsOpen) {
            serialPort.Close();
        }
        Debug.Log("COM port closed");
    }

    void OnDisable() {
        CloseCOMPort();
    }
}
