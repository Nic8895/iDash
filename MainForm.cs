﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iDash
{

    public partial class MainForm : Form
    {
        private const int WAIT_ARDUINO_SET_DEBUG_MODE = 100;
        private const int WAIT_UI_FREQUENCY = 1000;
        private const int WAIT_THREADS_TO_CLOSE = 3500;

        private SerialManager sm;
        private ButtonHandler bh;
        private VJoyFeeder vf;
        private IRacingConnector irc;
        private RaceRoomConnector rrc;

        public static bool stopThreads = false;
        public static bool stopIRacingThreads = false;
        public static bool stopRaceRoomThreads = false;
        private static List<string> _7Segment;
        private static string strFormat = "";
        private static readonly Object listLock = new Object();

        public delegate void AppendToStatusBarDelegate(String s);
        public AppendToStatusBarDelegate appendToStatusBar;
        public delegate void AppendToDebugDialogDelegate(String s);
        public AppendToDebugDialogDelegate appendToDebugDialog;        
        private bool isSearchingButton = false;
        private const string BUTTON_PREFIX = "Button_";
        private bool isWaitingForKey = false;

        public delegate void HandleButtonActions(List<State> states);
        public HandleButtonActions handleButtonActions;                


        public MainForm()
        {
            this.appendToStatusBar = new AppendToStatusBarDelegate(UpdateStatusBar);
            this.appendToDebugDialog = new AppendToDebugDialogDelegate(AppendToDebugDialog);
            InitializeComponent();

            handleButtonActions = new HandleButtonActions(handleButtons);            

            sm = new SerialManager();
            bh = new ButtonHandler(sm);
            vf = new VJoyFeeder(bh);
            sm.StatusMessageSubscribers += UpdateStatusBar;
            vf.StatusMessageSubscribers += UpdateStatusBar;
            sm.DebugMessageSubscribers += UpdateDebugData;
            bh.buttonStateHandler += ButtonStateReceived;

            sm.Init();
            vf.InitializeJoystick();                                    
        }

        private void parseViews()
        {
            if (this.views.Items.Count > 0)
            {
                string[] items = this.views.SelectedItem.ToString().Split(Utils.LIST_SEPARATOR);
                lock (listLock)
                {
                    _7Segment = new List<string>();

                    //ignoring "when connected" flag
                    for (int x = 0; x < items.Length - 1; x++)
                    {
                        if (x == items.Length - 2)
                        {
                            strFormat = items[x];
                        }
                        else
                        {
                            _7Segment.Add(items[x]);
                        }
                    }
                }
            }
        }

        private void saveAppSettings()
        {
            Properties.Settings.Default.TM1637 = new ArrayList(views.Items);
            Properties.Settings.Default.BUTTONS = new ArrayList(views2.Items);
            Properties.Settings.Default.Save();
        }

        private void buttonSend_Click(object sender, EventArgs e) // send button  event
        {
            byte[] aux = System.Text.Encoding.ASCII.GetBytes(richTextBoxSend.Text);
            Command command = new Command(aux[0], Utils.getSubArray<byte>(aux, 1, aux.Length - 1));
            sm.sendCommand(command);     //transmit data
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            stopThreads = true;
            sm.StatusMessageSubscribers -= UpdateStatusBar;
            vf.StatusMessageSubscribers -= UpdateStatusBar;
            sm.DebugMessageSubscribers -= UpdateDebugData;
            if (irc != null)
                irc.StatusMessageSubscribers -= UpdateStatusBar;
            if (rrc != null)
                rrc.StatusMessageSubscribers -= UpdateStatusBar;

            saveAppSettings();

            //wait all threads to close
            await Task.Delay(WAIT_THREADS_TO_CLOSE);
        }

        public void AppendToStatusBar(String s)
        {
            statusBar.AppendText(s);
        }

        public void UpdateStatusBar(string s)
        {
            if (InvokeRequired)
                this.statusBar.BeginInvoke(this.appendToStatusBar, new Object[] { s });
            else this.AppendToStatusBar(s);

        }

        public void AppendToDebugDialog(String s)
        {
            debugData.AppendText(s);
        }

        public void UpdateDebugData(string s)
        {
            if (InvokeRequired)
                this.debugData.BeginInvoke(this.appendToDebugDialog, new Object[] { s });
            else this.AppendToDebugDialog(s);

        }

        private void statusBar_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            statusBar.SelectionStart = statusBar.Text.Length;
            // scroll it automatically
            statusBar.ScrollToCaret();
        }

        private void clearData_Click(object sender, EventArgs e)
        {
            debugData.Clear();
        }

        private async void debugModes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //dbug mode off
            byte[] state = { 0x00 };
            if (debugModes.SelectedIndex != 0)
            {
                //debug mode on
                state[0] = 0x01;
            }
            Command command = new Command(Command.CMD_SET_DEBUG_MODE, state);
            SerialManager.debugMode = (DebugMode)debugModes.SelectedItem;
            //make sure Arduino has received/updated the debug mode state
            while (SerialManager.isArduinoInDebugMode != (state[0] == 1))
            {
                sm.sendCommand(command);     //transmit data
                await Task.Delay(WAIT_ARDUINO_SET_DEBUG_MODE);
            }
        }

        private void debugData_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            debugData.SelectionStart = statusBar.Text.Length;
            // scroll it automatically
            debugData.ScrollToCaret();

            if (debugData.Lines.Count() > 200)
            {
                debugData.Clear();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            byte[] PDF = Properties.Resources.telemetry_11_23_15;
            MemoryStream ms = new MemoryStream(PDF);

            //Create PDF File From Binary of resources folders help.pdf
            FileStream f = new FileStream("telemetry_11_23_15.pdf", FileMode.OpenOrCreate);

            //Write Bytes into Our Created help.pdf
            ms.WriteTo(f);
            f.Close();
            ms.Close();

            // Finally Show the Created PDF from resources
            Process.Start("telemetry_11_23_15.pdf");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (string item in props.SelectedItems)
            {
                if (!selected.Items.Contains(item))
                {
                    selected.Items.Add(item);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selected.SelectedIndex != -1)
            {
                for (int i = selected.SelectedItems.Count - 1; i >= 0; i--)
                    selected.Items.Remove(selected.SelectedItems[i]);
            }
        }

        public void MoveItem(ListBox lb, int direction)
        {
            // Checking selected item
            if (lb.SelectedItem == null || lb.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = lb.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= lb.Items.Count)
                return; // Index out of range - nothing to do

            object selected = lb.SelectedItem;

            // Removing removable element
            lb.Items.Remove(selected);
            // Insert it in new position
            lb.Items.Insert(newIndex, selected);
            // Restore selection
            lb.SetSelected(newIndex, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selected.SelectedIndex > 0)
            {
                this.MoveItem(selected, -1);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (selected.SelectedIndex < selected.Items.Count - 1)
            {
                this.MoveItem(selected, 1);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (views.SelectedIndex != -1)
            {
                for (int i = views.SelectedItems.Count - 1; i >= 0; i--)
                    views.Items.Remove(views.SelectedItems[i]);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string viewValue = null;

            for (int i = 0; i < selected.SelectedItems.Count; i++) {             
                viewValue += (string)selected.SelectedItems[i] + Utils.LIST_SEPARATOR;
            }
            if (viewValue != null && viewValue.Length > 0)
            {
                viewValue += textFormat.Text + Utils.LIST_SEPARATOR + isSimConnected.Checked;
                if (!views.Items.Contains(viewValue))
                {
                    views.Items.Add(viewValue);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (views.SelectedIndex > 0)
            {
                this.MoveItem(views, -1);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (views.SelectedIndex < views.Items.Count - 1)
            {
                this.MoveItem(views, 1);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (buttonActions.SelectedIndex > 0)
            {
                this.MoveItem(buttonActions, -1);
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (buttonActions.SelectedIndex < buttonActions.Items.Count - 1)
            {
                this.MoveItem(buttonActions, 1);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (views2.SelectedIndex > 0)
            {
                this.MoveItem(views2, -1);
            }
        }


        private void button13_Click(object sender, EventArgs e)
        {
            if (views2.SelectedIndex < views2.Items.Count - 1)
            {
                this.MoveItem(views2, 1);
            }
        }

        private void views_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (views.SelectedIndex >= 0)
            {
                string temp = views.SelectedItem.ToString();
                string[] selectedValue = views.SelectedItem.ToString().Split(Utils.LIST_SEPARATOR);
                isSimConnected.Checked = Convert.ToBoolean(selectedValue[selectedValue.Length - 1]);
                textFormat.Text = selectedValue[selectedValue.Length - 2];
                selected.Items.Clear();
                for (int i = 0; i < selectedValue.Length - 2; i++)
                {
                    selected.Items.Add(selectedValue[i]);
                }

                //irc.UpdateViewSelected(views.GetItemText(views.SelectedIndex));
                this.parseViews();
            }
        }

        public void setNextView()
        {
            if (views.SelectedIndex < views.Items.Count - 1)
            {
                views.SelectedIndex++;
            }
            else
            {
                views.SelectedIndex = 0;
            }

            this.parseViews();
        }

        public void setPreviousView()
        {
            if (views.SelectedIndex > 0)
            {
                views.SelectedIndex--;
            }
            else
            {
                views.SelectedIndex = views.Items.Count - 1;
            }

            this.parseViews();
        }

        public static List<string> get7SegmentData()
        {
            lock (listLock)
            {
                return _7Segment;
            }
        }

        public static string getStrFormat()
        {
            return strFormat;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSearchingButton = tabControl1.SelectedIndex == 1;
        }

        private void addButtonToList(int index)
        {
            if (buttonsActive != null)
            {
                if (!buttonsActive.Items.Contains(BUTTON_PREFIX + index))
                {
                    buttonsActive.Items.Add(BUTTON_PREFIX + index);
                    buttonsActive.SelectedIndex = buttonsActive.Items.Count - 1;
                }
                else
                {
                    for (int x = 0; x < buttonsActive.Items.Count; x++)
                    {
                        if (buttonsActive.Items[x].ToString() == BUTTON_PREFIX + index)
                        {
                            buttonsActive.SelectedIndex = x;
                        }
                    }
                }
            }
        }

        private void processButton(string actionId)
        {
            ActionHandler actionHandler = (new ActionHandlerFactory(this)).getInstance(actionId);
            
            if (actionHandler != null)
                actionHandler.process();
        }

        private void handleButtons(List<State> states)
        {
            if (states != null)
            {
                for (int x = 0; x < states.Count; x++)
                {
                    if (states[x] == State.KeyUp)
                    {
                        if (isSearchingButton)
                        {
                            this.addButtonToList(x);
                        }

                        for (int y = 0; y < views2.Items.Count; y++)
                        {
                            string[] actions = views2.Items[y].ToString().Split(Utils.SIGN_EQUALS);
                            if (actions[0] == BUTTON_PREFIX + x)
                            {
                                this.processButton(actions[1]);
                            }
                        }
                    }
                }
            }            
        }

        public void ButtonStateReceived(List<State> states)
        {
            if (this.views2.InvokeRequired)
            {
                this.BeginInvoke
                    (handleButtonActions, states);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if(views2.SelectedIndex > -1)
            {
                views.Items.Remove(views2.SelectedIndex);
            }
        }

        private bool isButtonBinded(string buttonId)
        {
            foreach(string s in views2.Items) {
                string id = s.Split(Utils.SIGN_EQUALS)[0];
                if (buttonId.Equals(id))
                    return true;
            }

            return false;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if(buttonsActive.SelectedIndex > -1 && buttonActions.SelectedIndex > -1)
            {
                string buttonId = buttonsActive.SelectedItem.ToString();
                
                if(isButtonBinded(buttonId))
                {
                    MessageBox.Show(string.Format("Button {0} already binded.", buttonId),
                                "Important Note",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                    return;
                }

                string value = buttonId + Utils.SIGN_EQUALS + buttonActions.SelectedItem.ToString();
                if (!views2.Items.Contains(value))
                    views2.Items.Add(value);
            }
            else
            {
                MessageBox.Show("Please select a buttons and one action.",
                                "Important Note",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (views2.SelectedIndex != -1)
            {
                for (int i = views2.SelectedItems.Count - 1; i >= 0; i--)
                    views2.Items.Remove(views2.SelectedItems[i]);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //restore TM1637 and Buttons settings
            if (Properties.Settings.Default.TM1637 != null)
            {
                this.views.Items.AddRange(Properties.Settings.Default.TM1637.ToArray());
            }
            if (Properties.Settings.Default.BUTTONS != null)
            {
                this.views2.Items.AddRange(Properties.Settings.Default.BUTTONS.ToArray());
            }

            if (this.views.Items.Count > 0)
            {
                this.views.SelectedIndex = 0;
                this.parseViews();
            }

            foreach (string s in ActionHandler.ACTIONS) {
                buttonActions.Items.Add(s);
            }
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            if (buttonsActive.SelectedIndex > -1)
            {                
                isWaitingForKey = true;
                label4.Visible = true;
                while(isWaitingForKey)
                {
                    if (label4.ForeColor == Color.Black)
                        label4.ForeColor = Color.Red;
                    else
                        label4.ForeColor = Color.Black;
                    await Task.Delay(250);
                }
            }
            else
            {
                MessageBox.Show("Please select a buttons.",
                                "Important Note",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isWaitingForKey)
            {
                isWaitingForKey = false;
                label4.Visible = false;
                string buttonId = buttonsActive.SelectedItem.ToString();

                if (isButtonBinded(buttonId))
                {
                    MessageBox.Show(string.Format("Button {0} already binded.", buttonId),
                                "Important Note",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                    return;
                }

                string value = buttonId + Utils.SIGN_EQUALS + e.KeyChar;
                if (!views2.Items.Contains(value))
                    views2.Items.Add(value);
            }
        }

        private void resetAllConnectors()
        {
            irc = null;
            rrc = null;
            ToolStripMenuItem menu = (ToolStripMenuItem)mainmenu.Items[0];
            foreach (ToolStripMenuItem mItem in menu.DropDownItems)
            {
                mItem.CheckState = CheckState.Unchecked;
            }
        }

        private void iRacingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetAllConnectors();
            stopIRacingThreads = false;
            stopRaceRoomThreads = true;
            ((ToolStripMenuItem)sender).CheckState = CheckState.Checked;            
            if (irc == null)
            {
                irc = new IRacingConnector(sm);
                irc.StatusMessageSubscribers += UpdateStatusBar;
            }
        }

        private void raceroomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetAllConnectors();
            stopIRacingThreads = true;
            stopRaceRoomThreads = false;
            ((ToolStripMenuItem)sender).CheckState = CheckState.Checked;
            ToolStripMenuItem iRacingMenu = (ToolStripMenuItem)((ToolStripMenuItem)mainmenu.Items[0]).DropDownItems[0];
            iRacingMenu.CheckState = CheckState.Unchecked;
            if (rrc == null)
            {
                rrc = new RaceRoomConnector(sm);
                rrc.StatusMessageSubscribers += UpdateStatusBar;
            }
        }
    }
}
