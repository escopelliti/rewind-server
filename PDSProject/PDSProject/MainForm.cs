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

        public MainForm()
        {
            InitializeComponent();
            clipboardTextDelegate += new SetTextToClipboard(SetClipboardText);
            clipboardFilesDelegate += new SetFileDropListClipboard(SetClipboardFileDropList);
            clipboardImageDelegate += new SetImageToClipboard(SetClipboardImage);
        }

        [STAThread]
        public static void Main()
        {
            ConnectionHandler handler = new ConnectionHandler();
            handler.Listen();
            mainForm = new MainForm();
            Application.Run(mainForm);
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
    }
}
