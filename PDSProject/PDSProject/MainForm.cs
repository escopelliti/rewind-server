using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using CommunicationLibrary;
using Newtonsoft.Json;
using System.Threading;
using Clipboard;
using System.IO;
using System.Security.Cryptography;

namespace PDSProject
{
    public partial class MainForm : Form
    {
        private NotifyIcon mainNotifyIcon;
        private NotifyIcon feedbackNotifyIcon;
        
        private static System.Windows.Forms.Timer timer;        
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        public delegate void SetTextToClipboard(string stringData);
        public delegate void SetFileDropListClipboard(StringCollection fileDropList);
        public delegate void SetImageToClipboard(Image image);
        public static SetTextToClipboard clipboardTextDelegate;
        public static SetFileDropListClipboard clipboardFilesDelegate;
        public static SetImageToClipboard clipboardImageDelegate;
        public static MainForm mainForm;
        private ushort[] portRange;
        private BackgroundWorker bw;
        private EventHandler eventHandler;
        private Configuration.ConfigurationMgr confMgr;
        private static Configuration.Configuration conf;
        private Discovery.ServiceRegister sr;
        private ConnectionHandler connHandler;
        
        public MainForm()
        {
            InitializeComponent();
            clipboardTextDelegate += new SetTextToClipboard(SetClipboardText);
            clipboardFilesDelegate += new SetFileDropListClipboard(SetClipboardFileDropList);
            clipboardImageDelegate += new SetImageToClipboard(SetClipboardImage);
            this.MouseHover += OnMouseHover;
            this.FormClosing += MainForm_FormClosing;
            //leggi le porte dal file o se non esiste apri il pannello e le fai inserire e te le prendi;            
            confMgr = new Configuration.ConfigurationMgr();
            conf = null;
            StartBackgroundWorker();
            InitTrayIcon();
            StartTimer();
            if (confMgr.ExistConf())
            {
                conf = confMgr.ReadConf();
                connHandler = new ConnectionHandler(this, conf);
                Window_StateChanged(new EventArgs());                                                
                sr = new Discovery.ServiceRegister(Convert.ToUInt16(conf.DataPort), Convert.ToUInt16(conf.CmdPort), this);                
                //StartListening();
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }            
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {            
            Environment.Exit(0);
        }

        private void StartListening()
        {
            connHandler.CmdPort = Convert.ToUInt16(conf.CmdPort);
            connHandler.DataPort = Convert.ToUInt16(conf.DataPort);
            try
            {               
                connHandler.ListenCmd();
            }
            catch (Exception ex)
            {
                Environment.Exit(-1);
            }
        }

        private void StopListeningCmd()
        {
            connHandler.StopListeningCmd();    
        }

        private void StartBackgroundWorker()
        {
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(CreateComboboxRange);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FillsCombobox);
        }

        private void InitTrayIcon()
        {

            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { this.menuItem1 });

            // Initialize menuItem1 
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Exit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);


            mainNotifyIcon = new System.Windows.Forms.NotifyIcon();
            mainNotifyIcon.Icon = new System.Drawing.Icon("..\\..\\..\\resources\\computer.ico");
            mainNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);

            mainNotifyIcon.ContextMenu = this.contextMenu1;

            feedbackNotifyIcon = new System.Windows.Forms.NotifyIcon();
            feedbackNotifyIcon.Icon = new System.Drawing.Icon("..\\..\\..\\resources\\blinkingIcon.ico");
        }

        private void Window_StateChanged(EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                mainNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                mainNotifyIcon.BalloonTipText = "Minimized the app ";
                mainNotifyIcon.ShowBalloonTip(400);
                mainNotifyIcon.Visible = true;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {                
                mainNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }       

        private void MyNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {            
            this.WindowState = FormWindowState.Normal;
            Window_StateChanged(new EventArgs());
        }
     
        public void OnSetServerFocus(Object obj, Object param)
        {
            RequestEventArgs rea = (RequestEventArgs)param;
            RequestState rs = (RequestState)rea.requestState;

            string content = rs.stdRequest.content.ToString();
            if (content == Protocol.ProtocolUtils.FOCUS_ON)
            {
                timer.Tick += eventHandler;
                feedbackNotifyIcon.Visible = true;
                try
                {
                    this.connHandler.ListenData();
                }
                catch (Exception ex)
                {
                    Environment.Exit(-1);
                }
                this.connHandler.closed = false;
                //ascolto data
            }
            else
            {
                feedbackNotifyIcon.Visible = false;
                timer.Tick -= eventHandler;
                this.connHandler.StopListeningData();
                this.connHandler.closed = true;
            }
                       
        }

        public void StartTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            eventHandler += Each_Tick;
            timer.Start();
        }

        public void Each_Tick(object o, EventArgs sender)
        {
            feedbackNotifyIcon.Visible = !feedbackNotifyIcon.Visible;
        }

        public static ClipboardPOCO GetClipboardContent()
        {
            ClipboardPOCO clipboardPOCO = new ClipboardPOCO();
            if (System.Windows.Clipboard.ContainsFileDropList())
            {
                clipboardPOCO.content = System.Windows.Clipboard.GetFileDropList();
                clipboardPOCO.contentType = ClipboardPOCO.FILE_DROP_LIST;
                return clipboardPOCO;
            }
            if (System.Windows.Clipboard.ContainsText())
            {
                clipboardPOCO.content = System.Windows.Clipboard.GetText();
                clipboardPOCO.contentType = ClipboardPOCO.TEXT;
                return clipboardPOCO;
            }
            if (System.Windows.Clipboard.ContainsImage())
            {
                System.Drawing.Image img = System.Windows.Forms.Clipboard.GetImage();
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imageBytes = ms.ToArray();
                    clipboardPOCO.content = imageBytes;
                }
                clipboardPOCO.contentType = ClipboardPOCO.IMAGE;
                return clipboardPOCO;
            }
            return null;
        }

        private void FillsCombobox(Object sender, RunWorkerCompletedEventArgs e)
        {                        
            comboBox1.DataSource = portRange;
            var bindingSrc = new BindingSource(portRange, null);
            this.comboBox2.DataSource = bindingSrc;            
        }

        private void CreateComboboxRange(Object sender, DoWorkEventArgs e)
        {            
            ushort startingPort = 10000;
            portRange = new ushort[(UInt16.MaxValue - startingPort) + 1];
            int counter = 0;
            while (startingPort < UInt16.MaxValue)
            {
                portRange[counter] = startingPort;                
                startingPort++;
                counter++;
            }
            portRange[counter] = startingPort;
        }

        [STAThreadAttribute]
        public static void Main()
        {            
            mainForm = new MainForm();
            Application.Run(mainForm);            
        }

        public void OnMouseHover(object sender, System.EventArgs e)
        {
            if (bw.IsBusy != true && this.comboBox1.Items.Count == 0)
            {
                bw.RunWorkerAsync();
            }
            this.MouseHover -= OnMouseHover;
        }

        private void SetClipboardText(string contentToPaste)
        {
            System.Windows.Forms.Clipboard.SetText(contentToPaste);
        }

        private void SetClipboardImage(Image image)
        {
            System.Windows.Forms.Clipboard.SetImage(image);
        }

        private void SetClipboardFileDropList(StringCollection fileDropList)
        {
            System.Windows.Clipboard.SetFileDropList(fileDropList);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            Window_StateChanged(e);
        }

        public void OnServiceRegisterd(object sender, EventArgs e)
        {
            Thread Listener = new Thread(new ThreadStart(StartListening));
            Listener.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string psw = this.textBox1.Text;
            ushort dataPort = (ushort)this.comboBox1.SelectedItem;
            ushort cmdPort = (ushort) this.comboBox2.SelectedItem;
            if (psw != null && psw != String.Empty && psw != "")
            {
                if (dataPort != cmdPort) 
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(psw);
                    SHA256Managed hashstring = new SHA256Managed();
                    byte[] hash = hashstring.ComputeHash(bytes);
                     
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach(byte b in hash)
                    {
                        stringBuilder.AppendFormat("{0:X2}", b);
                    }
                    string hashString = stringBuilder.ToString();
                    confMgr.WriteConf(dataPort, cmdPort, hashString);
                    if (sr != null)
                    {
                        MessageBox.Show("Le modifiche saranno disponibili al riavvio dell'applicazione", "Informazione", MessageBoxButtons.OK, MessageBoxIcon.Information);                       
                    }
                    else
                    {
                        conf = confMgr.ReadConf();
                        connHandler = new ConnectionHandler(this, conf);
                        sr = new Discovery.ServiceRegister(Convert.ToUInt16(dataPort), Convert.ToUInt16(cmdPort), this);                        
                    }
                    
                    this.WindowState = FormWindowState.Minimized;
                    Window_StateChanged(e);
                } 
                else
                {
                    MessageBox.Show("Le porte non possono avere lo stesso valore");
                }                 
            }
            else
            {
                MessageBox.Show("Inserisci una password per continuare!");
            }             
        }

        private void StartCmdService(object state)
        {
            this.sr.RegisterCmdService();
        }

        private void StartDataService(object state)
        {
            this.sr.RegisterDataService();
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            this.Close();            
        }

        public void StopFeedbackIcon()
        {
            feedbackNotifyIcon.Visible = false;
            timer.Tick -= eventHandler;
        }
    }
}
