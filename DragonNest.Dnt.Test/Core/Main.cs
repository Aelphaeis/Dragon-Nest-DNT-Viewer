﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO.Pipes;
using WeifenLuo.WinFormsUI.Docking;
using DragonNest.ResourceInspection.Dnt.Viewer;
using DragonNest.ResourceInspection.Pak.Viewer;
using System.ServiceModel;

namespace DragonNest.ResourceInspection.Core
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class Main : Form, DNRIService
    {
        const string PipeName = "net.pipe://localhost";
        const string PipeService = "DNRIS"; //dragon neset resource inspection service;
        ServiceHost @this;
        public Main()
        {
            InitializeComponent();
        }

        public Main(String [] args) : this()
        {
            @this = new ServiceHost(this, new Uri(PipeName));
            @this.AddServiceEndpoint(typeof(DNRIService), new NetNamedPipeBinding(), PipeService);
            @this.BeginOpen((IAsyncResult ar) => @this.EndOpen(ar), null);

            foreach (var v in args)
                using (FileStream fs = new FileStream(v, FileMode.Open))
                    OpenDntWindowFromStream(fs);
        }


        private void dntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog() { Filter = "DNT | *.dnt"};
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Task.Run(() => OpenDntWindowFromStream(ofd.OpenFile()));
        }

        public void OpenDntWindowFromStream(Stream stream) 
        {

            DntViewer viewer = new DntViewer();
            viewer.LoadDNT(stream);
            viewer.Show(dockPanel1, DockState.Document);
        }

        public void OpenPakWindowFromStream(Stream stream)
        {
            PakViewer viewer = new PakViewer();
            viewer.LoadPakStream(stream);
            viewer.Show(dockPanel1, DockState.Document);
        }
             private void pakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog() { Filter = "PAK | *.pak" };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Task.Run(() => OpenPakWindowFromStream(ofd.OpenFile()));
        }


        public void OpenDnt(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
                OpenDntWindowFromStream(fs);
        }
     
        private void showLinqToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DntViewer.ShowLinq = !showLinqToolStripMenuItem.Checked;
            showLinqToolStripMenuItem.Checked = DntViewer.ShowLinq;
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {

            Task.Run(() => @this.BeginClose((IAsyncResult ar) => @this.EndClose(ar), null));
        }

   
    }
}
