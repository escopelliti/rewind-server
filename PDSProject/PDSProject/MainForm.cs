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
        public delegate void SetTextToClipboard(string stringData);
        public delegate void SetFileDropListClipboard(StringCollection fileDropList);
        public delegate void SetImageToClipboard(Image image);
        public static SetTextToClipboard clipboardTextDelegate;
        public static SetFileDropListClipboard clipboardFilesDelegate;
        public static SetImageToClipboard clipboardImageDelegate;
        public static MainForm mainForm;
        private ushort[] portRange;
        private BackgroundWorker bw;
        
        public MainForm()
        {
            InitializeComponent();
            clipboardTextDelegate += new SetTextToClipboard(SetClipboardText);
            clipboardFilesDelegate += new SetFileDropListClipboard(SetClipboardFileDropList);
            clipboardImageDelegate += new SetImageToClipboard(SetClipboardImage);

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(CreateComboboxRange);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FillsCombobox);

            //LEGGI DAL FILE DI CONFIGURAZIONE SE ESISTE ALTRIMENTI CREALO DI DEFAULT
        }

        public static void OnSetServerFocus(Object obj, Object param)
        {
            RequestEventArgs rea = (RequestEventArgs)param;
            RequestState rs = (RequestState)rea.requestState;

            string content = rs.stdRequest[Protocol.ProtocolUtils.CONTENT].ToString();
            if (content == Protocol.ProtocolUtils.FOCUS_ON)
            {
                ////    myDispatcherTimer.Tick += new EventHandler(Each_Tick);
            }
            else
            {
                ////    myDispatcherTimer.Tick += new EventHandler(Each_Tick);
            }                        
        }

        public static ClipboardPOCO GetClipboardContent()
        {
            ClipboardPOCO clipboardPOCO = new ClipboardPOCO();
            if (System.Windows.Forms.Clipboard.ContainsFileDropList())
            {
                clipboardPOCO.content = System.Windows.Forms.Clipboard.GetFileDropList();
                clipboardPOCO.contentType = ClipboardPOCO.FILE_DROP_LIST;
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

        [STAThread]
        public static void Main()
        {
            ConnectionHandler handler = new ConnectionHandler();
            handler.Listen();
            mainForm = new MainForm();
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
            System.Windows.Forms.Clipboard.SetFileDropList(fileDropList);
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
    }
}
