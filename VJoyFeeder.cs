﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace iDash
{
    class VJoyFeeder
    {
        // Declaring one joystick (Device id 1) and a position structure. 
        public vJoy joystick;
        public uint jID = 1;

        //events
        public delegate void StatusMessageHandler(string m);
        public StatusMessageHandler StatusMessageSubscribers;        


        public VJoyFeeder(ButtonHandler bh)
        {
            bh.ButtonStateSubscribers += new ButtonHandler.ButtonStateHandler(ButtonStateReceived);
        }        

        public void InitializeJoystick()
        {
            joystick = new vJoy();           

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                NotifyStatusMessage("vJoy driver not enabled: Failed Getting vJoy attributes.");
                return;
            }
            else
                NotifyStatusMessage(String.Format("Vendor: {0} Product :{1} Version Number:{2}", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString()));

            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(jID);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    NotifyStatusMessage(String.Format("vJoy Device {0} is already owned by this feeder", jID));
                    break;
                case VjdStat.VJD_STAT_FREE:
                    NotifyStatusMessage(String.Format("vJoy Device {0} is free", jID));
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    NotifyStatusMessage(String.Format("vJoy Device {0} is already owned by another feeder. Cannot continue", jID));
                    return;
                case VjdStat.VJD_STAT_MISS:
                    NotifyStatusMessage(String.Format("vJoy Device {0} is not installed or disabled. Cannot continue", jID));
                    return;
                default:
                    NotifyStatusMessage(String.Format("vJoy Device {0} general error. Cannot continue", jID));
                    return;
            };

            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(jID, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(jID, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(jID, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(jID, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(jID, HID_USAGES.HID_USAGE_RZ);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(jID);
            int ContPovNumber = joystick.GetVJDContPovNumber(jID);
            int DiscPovNumber = joystick.GetVJDDiscPovNumber(jID);

            // Print results
            NotifyStatusMessage(String.Format("vJoy Device {0} capabilities:", jID));
            NotifyStatusMessage(String.Format("Numner of buttons\t{0}", nButtons));
            NotifyStatusMessage(String.Format("Numner of Continuous POVs\t{0}", ContPovNumber));
            NotifyStatusMessage(String.Format("Numner of Descrete POVs\t{0}", DiscPovNumber));
            NotifyStatusMessage(String.Format("Axis X\t\t{0}", AxisX ? "Yes" : "No"));
            NotifyStatusMessage(String.Format("Axis Y\t\t{0}", AxisX ? "Yes" : "No"));
            NotifyStatusMessage(String.Format("Axis Z\t\t{0}", AxisX ? "Yes" : "No"));
            NotifyStatusMessage(String.Format("Axis Rx\t\t{0}", AxisRX ? "Yes" : "No"));
            NotifyStatusMessage(String.Format("Axis Rz\t\t{0}", AxisRZ ? "Yes" : "No"));

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                NotifyStatusMessage(String.Format("Version of Driver Matches DLL Version ({0:X})", DllVer));
            else
                NotifyStatusMessage(String.Format("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})", DrvVer, DllVer));


            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(jID))))
            {
                NotifyStatusMessage(String.Format("Failed to acquire vJoy device number {0}.", jID));
                return;
            }
            else
                NotifyStatusMessage(String.Format("Acquired: vJoy device number {0}.", jID));

        }

        //----------------------------------------- Events

        public void NotifyStatusMessage(string args)
        {
            StatusMessageHandler handler = StatusMessageSubscribers;

            if (handler != null)
            {
                handler(args + "\n");
            }
        }

        public void ButtonStateReceived(byte[] states)
        {
            for (uint i = 0; i < states.Length; i++)
            {
                joystick.SetBtn(states[i] == 1, jID, i + 1);
            }
        }

    }
}