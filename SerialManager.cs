﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iDash
{
    public class SerialManager : IDisposable
    {

        int bs = 0;

        private const int BUFFER_SIZE = 40;
        private const int WAIT_TO_RECONNECT = 500;        
        //arduino will wait 5 secs for a SYN ACK        
        private const int ARDUINO_TIMED_OUT = 500;
        private const int WAIT_SERIAL_CONNECT = 100;
        //lets try to send a SYN to arduino, 5 times, before it times out
        private const int WAIT_FOR_ARDUINO_DATA = 100;

        //arduino command length
        private int commandLength;        
        private byte[] serialCommand = new byte[BUFFER_SIZE];
        private SerialPort serialPort = new SerialPort(); //create of serial port
        private static long lastArduinoResponse = -1;        
        private object readLock = new object();
        private object sendLock = new object();

        //debug mode set on form
        public DebugMode formDebugMode = DebugMode.None;   
        //debug mode currently set in arduino
        public DebugMode arduinoDebugMode = DebugMode.None;
        //disable messages when in Default debugging mode. This is set by the mainform. (ignore incoming data)
        public bool isDisabledSerial = false;
        //indicates how often a debug message need to be logged in the debug dialog
        private long lastMessageLogged = 0; 
        //show debug commands in hex or int (show as hexadecimal)
        public bool asHex = false;

        //events
        public delegate void CommandReceivedHandler(Command command);
        public CommandReceivedHandler CommandReceivedSubscribers;
        public delegate void StatusMessageHandler(string m);
        public StatusMessageHandler StatusMessageSubscribers;
        public delegate void DebugMessageHandler(string m);
        public DebugMessageHandler DebugMessageSubscribers;        

        private bool disposed = false;

        private string[] portNames;

        public void Init()
        {
            serialPort.Parity = Parity.None;     //selected parity 
            serialPort.StopBits = StopBits.One;  //selected stopbits
            serialPort.DataBits = 8;             //selected data bits
            serialPort.BaudRate = 38400;         //selected baudrate            
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);//received even handler                         
            
            this.tryToConnect();
        }

        private bool isArduinoAlive()
        {
            return !Utils.hasTimedOut(lastArduinoResponse, ARDUINO_TIMED_OUT);
        }

        private async void tryToConnect()
        {
            HashSet<string> notificationSent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);            

            while (!MainForm.stopThreads)
            {
                //check if new usb was connected
                if(lastArduinoResponse == -1)
                    portNames = SerialPort.GetPortNames();

                if (isArduinoAlive())
                {                    
                    await Task.Delay(WAIT_FOR_ARDUINO_DATA);
                }
                else
                {
                    foreach (string port in portNames)
                    {
                        if (serialPort.IsOpen)  //if port is  open 
                        {
                            serialPort.Close();  //close port
                        }
                        else if (lastArduinoResponse > 0 && portNames.Length == 1)
                        {
                            NotifyStatusMessage("Arduino at port " + port + " disconnected.");
                            lastArduinoResponse = 0;
                        }

                        serialPort.PortName = port;    //selected name of port

                        if (!notificationSent.Contains(port))
                        {
                            NotifyStatusMessage("Searching for Arduino at " + port + "...");                            
                        }

                        if (MainForm.formFinishedLoading)
                        {
                            notificationSent.Add(port);
                        }

                        try
                        {
                            serialPort.Open();        //open serial port                
                        }
                        catch(Exception e)
                        {
                            /*if (!notificationSent.Contains(port))
                            {
                                MessageBox.Show(String.Format("Port {0} already in use", port),
                                    "Warning",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }*/
                            await Task.Delay(WAIT_TO_RECONNECT);        //port is probably closing, wait...  

                            continue;                          
                        }
                       
                        //check if connected
                        sendSynAck();

                        //wait for arduino ACK message
                        await Task.Delay(WAIT_SERIAL_CONNECT);

                        if (isArduinoAlive())
                        {
                            NotifyStatusMessage("Arduino found at port " + port + "...");
                            //if arduino found always try to reconnect to the same port
                            portNames = new string[] {port};
                            break;
                        }
                    }
                }
            }

        }

        private void sendSynAck()
        {
            Command synack = new Command(Command.CMD_SYN_ACK, new byte[0]);
            sendCommand(synack, true);
        }        

        private string processCommand(Command command)
        {
            string type = "invalid";
            //called by processData that is already sync
            try {                
                byte c = command.getData()[0];
                switch (c)
                {
                    //ACK message sent by Arduino
                    case Command.CMD_SYN:
                        break;
                    //Arduino response to the set debug mode command
                    case Command.CMD_RESPONSE_SET_DEBUG_MODE:
                        arduinoDebugMode = (DebugMode)command.getData()[1];                     
                        break;
                    //Arduino buttons state message
                    case Command.CMD_BUTTON_STATUS:                            
                        NotifyCommandReceived(command);
                        break;
                    //Arduino response when crc command failed
                    case Command.CMD_INVALID:
                        break;
                }
                type = command.getCommandType();

                if (formDebugMode == DebugMode.Default) {
                    if (isDisabledSerial) {
                        if (command.getRawData()[0] == Command.CMD_INIT_DEBUG) {
                            NotifyDebugMessage(String.Format("Command processed:{0} - ({1})\n", 
                                Utils.byteArrayToString(command.getRawData(), asHex), 
                                type));
                            lastMessageLogged = Utils.getCurrentTimeMillis();
                        }
                    }
                    else
                    {
                        if ((c != Command.CMD_BUTTON_STATUS && c != Command.CMD_SYN) || Utils.hasTimedOut(lastMessageLogged, 1000))
                        {
                            NotifyDebugMessage(String.Format("Command processed:{0} - ({1})\n", Utils.byteArrayToString(command.getRawData(), asHex), type));
                            lastMessageLogged = Utils.getCurrentTimeMillis();
                        }
                    }
                }
            } 
            catch(Exception e)
            {
                Logger.LogExceptionToFile(e);
            }

            return type;                        
        }      

        public void sendCommand(Command command, bool forcePost)
        {
            lock(sendLock)
            {
                try
                {
                    if (serialPort.IsOpen && (!isDisabledSerial || forcePost))
                    {

                        bs += command.getLength();
                        serialPort.Write(command.getRawData(), 0, command.getLength());

                        /*if (formDebugMode == DebugMode.Verbose)
                        {
                            Logger.LogDataToFile(String.Format("Data sent:{0} - ({1})\n", 
                                Utils.byteArrayToString(command.getRawData(), asHex),
                                command.getCommandType()));
                        }*/
                    }
                }
                catch (Exception e)
                {                    
                    Logger.LogExceptionToFile(e);

                    if(lastArduinoResponse > 0)
                        NotifyStatusMessage("Error sending command to Arduino."); //if there are not is any COM port in PC show message
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("{0}/s", bs);
            bs = 0;
        }

        public void processData(byte[] serialData)
        {
            StringBuilder logData = new StringBuilder("");

            lock (readLock)
            {
                if (serialData != null) { 
                    foreach (byte b in serialData)
                    {
                        switch (b)
                        {
                            //init command char found
                            case Command.CMD_INIT_DEBUG:
                            case Command.CMD_INIT:
                                serialCommand[0] = b;
                                commandLength = 1;

                                logData.Append("\n");
                                logData.Append(b + "-");
                                break;

                            //end of command char found, send command to be processed
                            case Command.CMD_END:
                                serialCommand[commandLength] = b;

                                Command command = new Command(serialCommand);

                                if (command != null && command.getLength() > 0)
                                {
                                    string type = processCommand(command);
                                    logData.Append(b);
                                    logData.Append(" - " + type + "\n");
                                }
                                commandLength = 0;
                                Utils.resetArray(serialCommand);
                                break;

                            //command init char already found, start adding the command data to buffer
                            default:
                                logData.Append(b + "-");
                                if (commandLength > 0)
                                {
                                    serialCommand[commandLength++] = b;
                                }
                                if (commandLength == BUFFER_SIZE - 1)
                                {
                                    commandLength = 0;
                                    Utils.resetArray(serialCommand);

                                    Logger.LogDataToFile(" - CMD_INVALID \n");
                                }
                                break;
                        }                        
                    }
                }
            }
            if (formDebugMode == DebugMode.Verbose)
            {
                Logger.LogDataToFile(logData.ToString());
            }
        }
                
        //event handler triggered by serial port
        public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                byte[] data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, data.Length);                

                if(data.Length > 0)
                {
                    lastArduinoResponse = Utils.getCurrentTimeMillis();
                    processData(data);
                }
            }
        }

        //notify subscribers that a command was received
        protected virtual void NotifyCommandReceived(Command args)
        {
            CommandReceivedHandler handler = CommandReceivedSubscribers;

            if (handler != null)
            {
                handler(args);
            }
        }

        //notify subscribers (statusbar) that a message has to be logged
        public void NotifyStatusMessage(string args)
        {
            StatusMessageHandler handler = StatusMessageSubscribers;

            if (handler != null)
            {
                handler(args + "\n");
            }
        }

        //notify subscribers (debug field) that a message has to be logged
        public void NotifyDebugMessage(string args)
        {
            DebugMessageHandler handler = DebugMessageSubscribers;

            if (handler != null)
            {
                handler(args);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }
                // Release unmanaged resources.
                disposed = true;
            }
        }

        ~SerialManager() { Dispose(false); }

    }
}
