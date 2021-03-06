﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iDash
{
    public abstract class ISimConnector : BackgroundWorker
    {
        public delegate void StatusMessageHandler(string m);
        public StatusMessageHandler StatusMessageSubscribers;

        protected List<SerialManager> sm;

        private const int LED_NUM_TOTAL = 16;
        public const float FIRST_RPM = 0.7f;
        private bool stillRunning = true;

        public ISimConnector()
        {
            this.DoWork += this.worker_DoWork;
            this.WorkerSupportsCancellation = true;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.sm = (List<SerialManager>)e.Argument;
            start();

            e.Cancel = true;
            stillRunning = false;
            Dispose();
        }

        public bool isStillRunning()
        {
            return stillRunning;
        }

        protected abstract void start();

        protected void sendRPMShiftMsg(float currentRpm, float firstRpm, float lastRpm, int flag)
        {
            //black, last byte indicate state - 0 = no blink, 1 = blink
            byte[] rpmLed = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, Constants.LED_NO_BLINK };
            byte[] pattern = new byte[rpmLed.Length - 1];

            Array.Copy(Constants.RPM_PATTERN, 0, pattern, 0, pattern.Length);
            int ledOffset = 0;

            if ((flag & (int)Constants.FLAG_TYPE.YELLOW_FLAG) != 0) {
                Array.Copy(Constants.YELLOW_RGB, 0, pattern, ledOffset, pattern.Length);
                ledOffset++;
            }
            if ((flag & (int)Constants.FLAG_TYPE.BLUE_FLAG) != 0) {
                Array.Copy(Constants.BLUE_RGB, 0, pattern, ledOffset, pattern.Length - (2 * ledOffset));
                ledOffset++;
            }
            if (((flag & (int)Constants.FLAG_TYPE.IN_PIT_FLAG) != 0) || ((flag & (int)Constants.FLAG_TYPE.SPEED_LIMITER) != 0)) {
                Array.Copy(Constants.WHITE_RGB, 0, pattern, ledOffset, pattern.Length - (2 * ledOffset));
            }

            float rpmPerLed = (lastRpm - firstRpm) / LED_NUM_TOTAL; //rpm range per led                
           
            Command rgbShift = null;
            if (rpmPerLed > 0)
            {
                if (currentRpm >= firstRpm)
                {
                    int numActiveLeds = (int)(Math.Ceiling((currentRpm - firstRpm) / rpmPerLed)) + 1;

                    if (numActiveLeds > LED_NUM_TOTAL)
                    {
                        numActiveLeds = LED_NUM_TOTAL;
                    }
                    Array.Copy(pattern, 0, rpmLed, 0, numActiveLeds * 3); //each led colour has 3 bytes
                             
                    if (currentRpm < lastRpm)
                    {
                        rpmLed[rpmLed.Length - 1] = Constants.LED_NO_BLINK;
                    }
                    else //blink
                    {
                        rpmLed[rpmLed.Length - 1] = Constants.LED_BLINK;
                    }                    
                }
                else
                {
                    if (flag > 0)
                    {
                        Array.Copy(pattern, 0, rpmLed, 0, pattern.Length);
                        rpmLed[rpmLed.Length - 1] = Constants.LED_BLINK;
                    }
                    else
                    {
                        //clear shift lights
                        Array.Copy(Constants.BLACK_RGB, 0, rpmLed, 0, Constants.BLACK_RGB.Length);                        
                    }
                }
                
                //Array.Copy(pattern, 0, rpmLed, 0, pattern.Length);
                rgbShift = new Command(Command.CMD_RGB_SHIFT, rpmLed);
                foreach (SerialManager serialManager in sm)
                {
                    serialManager.enqueueCommand(rgbShift, false);
                }
            }
        }
        
        //implemented by child classes
        protected abstract string getTelemetryValue(string name, string type, string clazz);

        protected string getTelemetryData(string name, string strPattern)
        {
            if (name.Equals("hour.time"))
                return DateTime.Now.ToString("hh.mm.ss.ff");

            string result = "";

            if (!String.IsNullOrEmpty(name))
            {
                string[] type = name.Split('.');

                if (type.Length > 1)
                {
                    string clazz = null;
                    if(type.Length > 2)
                    {
                        clazz = type[2];
                    }

                    //type[0] = property name, type[1] = property type
                    result = this.getTelemetryValue(type[0], type[1], clazz);
                }

                if (!String.IsNullOrEmpty(result))
                {
                    result = Utils.formatString(result.Trim(), strPattern);
                }
            }

            if (!String.IsNullOrEmpty(result))
                return result;

            return "";
        }

        //needs to wait until MainForm 7Segment is loaded
        protected void send7SegmentMsg()
        {
            StringBuilder msg = new StringBuilder();
            List<string> _7SegmentData = MainForm.get7SegmentData();
            string[] strPatterns = MainForm.getStrFormat().Split(Constants.ITEM_SEPARATOR);

            if (_7SegmentData.Count > 0)
            {
                for (int x = 0; x < _7SegmentData.Count; x++)
                {
                    string name = _7SegmentData.ElementAt(x);
                    string pattern = "{0}";

                    if (strPatterns.Length > 0)
                    {
                        if (x < strPatterns.Length)
                            pattern = string.IsNullOrEmpty(strPatterns[x]) ? "{0}" : strPatterns[x];
                    }

                    msg.Append(this.getTelemetryData(name, pattern));
                }

                if (msg.Length > 0)
                {
                    byte[] b = Utils.getBytes(msg.ToString());
                    Command c = new Command(Command.CMD_7_SEGS, Utils.convertByteTo7Segment(b, 0));

                    foreach (SerialManager serialManager in sm)
                    {
                        serialManager.enqueueCommand(c, false);
                    }
                }
            }
        }

        //notify subscribers (statusbar) that a message has to be logged
        public void NotifyStatusMessage(string args)
        {
            StatusMessageHandler handler = StatusMessageSubscribers;

            if (handler != null)
            {                
                handler(string.Format("[{0:G}]: " + args + "\n", System.DateTime.Now));
            }
        }

        public new abstract void Dispose();        
    }
}
