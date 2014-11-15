using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using CommunicationLibrary;
using Newtonsoft.Json;
using System.Threading;
using Clipboard;

namespace PDSProject
{
    public partial class MainForm : Form
    {
        private NotifyIcon mainNotifyIcon;
        private NotifyIcon feedbackNotifyIcon;
        private FormWindowState lastWindowState; 
        private static System.Windows.Forms.Timer timer;
        private bool exit = false;
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
        
        public MainForm()
        {
            InitializeComponent();
            clipboardTextDelegate += new SetTextToClipboard(SetClipboardText);
            clipboardFilesDelegate += new SetFileDropListClipboard(SetClipboardFileDropList);
            clipboardImageDelegate += new SetImageToClipboard(SetClipboardImage);

            //leggi le porte dal file o se non esiste apri il pannello e le fai inserire e te le prendi;
            Discovery.ServiceRegister sr = new Discovery.ServiceRegister(12000, 12001);
            StartBackgroundWorker();
            InitTrayIcon();
            StartTimer();
            //LEGGI DAL FILE DI CONFIGURAZIONE SE ESISTE ALTRIMENTI CREALO DI DEFAULT
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

        protected override void OnClientSizeChanged(EventArgs e)
        {
            if (this.WindowState != lastWindowState)
            {
                lastWindowState = this.WindowState;
                Window_StateChanged(e);
            }
            base.OnClientSizeChanged(e);
        }        

        private void MyNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }
     
        public void OnSetServerFocus(Object obj, Object param)
        {
            RequestEventArgs rea = (RequestEventArgs)param;
            RequestState rs = (RequestState)rea.requestState;

            string content = rs.stdRequest[Protocol.ProtocolUtils.CONTENT].ToString();
            if (content == Protocol.ProtocolUtils.FOCUS_ON)
            {
                timer.Tick += eventHandler;
                feedbackNotifyIcon.Visible = true;
            }
            else
            {
                feedbackNotifyIcon.Visible = false;
                timer.Tick -= eventHandler;
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
            ushort startingPort = 1024;
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
            ConnectionHandler handler = new ConnectionHandler(mainForm);
            handler.Listen();
            Application.Run(mainForm);
        }


        private void MainForm_MouseHover(object sender, System.EventArgs e)
        {
            if (bw.IsBusy != true && this.comboBox1.Items.Count == 0)
            {
                bw.RunWorkerAsync();
            }
            this.MouseHover -= MainForm_MouseHover;
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
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string psw = this.textBox1.Text;
            string dataPort = (string) this.comboBox1.SelectedItem;
            string cmdPort = (string)this.comboBox2.SelectedItem;
            if (psw != null && psw != String.Empty && psw != "")
            {
                if (dataPort != cmdPort) {
                    //scrivi nel file di configurazione il digest calcolato in base alla psw e le porte
                } else {
                    MessageBox.Show("Le porte non possono avere lo stesso valore");
                }                 
            }
            else
            {
                MessageBox.Show("Inserisci una password per continuare!");
            } 
            
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            exit = true;
            this.Close();
        }

        private void MainForm_WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!exit)
            {
                this.WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
        }
    }
}
