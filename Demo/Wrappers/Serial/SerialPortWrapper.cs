using System.IO;
using System.IO.Ports;

namespace Demo.Wrappers.Serial
{
    private SerialPort _serialPort;
    public string PortName { get; set; }
    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
    private MemoryStream _buffer = new MemoryStream();
    private System.Timers.Timer t_DataReceivedTimeout;

    public SerialWrapper(string port, int baudRate, Parity parity, int dataBits, StopBits stopBits, SerialWrapperDataReceived dataReceivedHandler)
    {
        this.PortName = port;
        this.BaudRate = baudRate;
        this.Parity = parity;
        this.DataBits = dataBits;
        this.StopBits = stopBits;
        this.DataReceivedHandler = dataReceivedHandler;
        
        // Timers settings and elapsed events definition
        t_DataReceivedTimeout = new System.Timers.Timer(500);
        t_DataReceivedTimeout.Elapsed += DataReceivedTimeout_Elapsed;
        t_DataReceivedTimeout.AutoReset = false;
        
        // Serial Ports settings definitions
        _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
        _serialPort.Handshake = Handshake.None;
        _serialPort.DataReceived += SerialPort_DataReceived;
    }

    public void Start()
    {
        if (_serialPort.IsOpen) _serialPort.Close();
        _serialPort.Open();
    }
    public void Stop()
    {
        t_DataReceivedTimeout.Stop();
        _serialPort.Close();
    }

    public void StationPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        // If we received new data in the buffer we need to stop the timeout timers we have
        if (t_StationDataReceivedTimeout.Enabled)
            t_StationDataReceivedTimeout.Stop();

        // Create a memory stream object that will contain the new data received
        MemoryStream buffer = new MemoryStream();
        byte[] receivedByte = new byte[1];

        // Read byte by byte the data received and store it in the memory stream object
        while (_stationSerialPort.BytesToRead > 0)
        {
            _stationSerialPort.Read(receivedByte, 0, 1);
            buffer.Write(receivedByte, 0, 1);
        }

        // Once we got all the new bytes, add the bytes to the internal buffer (memory stream object)
        byte[] aux = buffer.ToArray();
        _stationBuffer.Write(aux, 0, aux.Length);

        // Restart the timeout timers
        t_StationDataReceivedTimeout.Start();
    }

    private void DataReceivedTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        // If this event is fired it means we havent received any new data in the established time for the buffer.
        byte[] currentBuffer = _buffer.ToArray();
        _buffer.Close();
        _buffer= new MemoryStream();

        OnDataReceived(new SerialWrapperDataReceivedArgs(currentBuffer, ByteArrayToHex(currentBuffer)));
    }
    public void OnDataReceived(SerialWrapperDataReceivedArgs e)
    {
        DataReceivedHandler.Invoke(this, e);
    }
    
    #region byte array to hex string conversion via byte manipulation
    public string ByteArrayToHex(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        int b;
        for (int i = 0; i < bytes.Length; i++)
        {
            b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }

        return new string(c);
    }
    #endregion
}