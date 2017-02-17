﻿using iDash.rFactor1Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iDash
{
    class RFactorConnector : ISimConnector
    {
        public delegate void StatusMessageHandler(string m);
        public StatusMessageHandler StatusMessageSubscribers;

        public Boolean running = false;
        private float firstRpm = 0;
        private float lastRpm = 0;
        private float currentRpm = 0;
        private Boolean mapped = false;
        private RFactorDataReader gameDataReader;
        private RF1SharedMemoryReader.RF1StructWrapper wrapper;
        private bool isConnected = false;
        private bool isGameRunning = false;

        private bool disposed = false;

        public RFactorConnector(SerialManager sm) : base(sm)
        {
            this.sm = sm;

            new Thread(new ThreadStart(start)).Start();
        }

        public async void start()
        {
            gameDataReader = new RF1SharedMemoryReader();

            Object rawGameData;
            NotifyStatusMessage("Waiting for RFactor...");

            isGameRunning = Utils.IsGameRunning(GameDefinition.automobilista.processName);

            while (!MainForm.stopThreads && !MainForm.stopRFactorThreads)
            {                
                if (isGameRunning)
                {                    
                    if (!mapped)
                    {
                        mapped = gameDataReader.Initialise();
                    }
                    else
                    {
                        try {
                            rawGameData = gameDataReader.ReadGameData();
                            wrapper = (RF1SharedMemoryReader.RF1StructWrapper)rawGameData;
                            if (wrapper.data.numVehicles > 0)
                            {
                                lastRpm = wrapper.data.engineMaxRPM;
                                firstRpm = FIRST_RPM * lastRpm;
                                //calibrate shift gear light rpm
                                lastRpm *= 0.93f;
                                currentRpm = wrapper.data.engineRPM;                                
                                sendRPMShiftMsg(currentRpm, firstRpm, lastRpm);
                                send7SegmentMsg();

                                if (!isConnected)
                                {
                                    string s = DateTime.Now.ToString("hh:mm:ss") + ": Connected to Automobilista.";
                                    NotifyStatusMessage(s);
                                    isConnected = true;
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Logger.LogExceptionToFile(e);
                            isGameRunning = false;
                            isConnected = false;
                        }
                    }                    
                }
                else
                {
                    isGameRunning = Utils.IsGameRunning(GameDefinition.automobilista.processName);
                } 

                if(!isConnected)
                {
                    sm.sendCommand(Utils.getDisconnectedMsgCmd(), false);
                }

                await Task.Delay(5);
            }            
            
            Dispose();
        }

        private string getValue(string name, string type, object clazz)
        {
            string result = "";

            Type pType = clazz.GetType();

            System.Reflection.FieldInfo field = pType.GetField(name);

            if (field != null)
            {
                try
                {
                    switch (type)
                    {
                        case "time":
                            float seconds = 0;
                                                        
                            if (field.FieldType.Name.Equals("Single"))
                            {
                                seconds = (float)field.GetValue(clazz);
                            }

                            TimeSpan interval = TimeSpan.FromSeconds(seconds);
                            result = interval.ToString(@"mm\.ss\.fff");
                            break;
                        case "kmh":
                            if (field.FieldType.Name.Equals("Single"))
                            {
                                result = ((int)Math.Floor((Single)field.GetValue(clazz) * 3.6)).ToString();
                            }
                            break;                        
                        default:
                            if (name.Equals("gear"))
                            {
                                int gear = (int)field.GetValue(clazz) - 1;
                                if (gear < 0)
                                {
                                    return "R";
                                }

                                result = (gear + 1).ToString();
                            }
                            else
                            {
                                result = field.GetValue(clazz).ToString();
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogExceptionToFile(e);
                }
            }

            return result;
        }

        protected override string getTelemetryValue(string name, string type, string clazz)
        {
            string result = "";

            if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(clazz))
            {
                switch (clazz)
                {
                    case "shared":
                        result = getValue(name, type, wrapper.data);
                        break;
                    case "vehicle":
                        result = getValue(name, type, getCurrentPlayer(wrapper.data.vehicle));
                        break;
                }
            }

            return result;
        }

        private rfVehicleInfo getCurrentPlayer(rfVehicleInfo[] vehicle)
        {
            foreach (rfVehicleInfo player in vehicle) {
                if (player.isPlayer == 1)
                    return player;
            }

            return vehicle[0];
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

        public override void Dispose()
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
                    if (gameDataReader != null)
                    {
                        gameDataReader.Dispose();
                    }
                }
                // Release unmanaged resources.
                disposed = true;
            }
        }

        ~RFactorConnector() { Dispose(false); }
    }
}
