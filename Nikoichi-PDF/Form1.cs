using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nikoichi_PDF
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string savePath = null;
        public string loadPath = null;
        public List<nikoJsonWrap> jsons = new List<nikoJsonWrap>();


        private void ReDrawList_Gen()
        {
            listView2.Items.Clear();
            foreach(nikoJsonWrap values in jsons)
            {
                string[] invalue = {values.ExportFileName,values.TotalPages.ToString() };
                listView2.Items.Add(new ListViewItem(invalue));
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PDFGen pDf = new PDFGen();
            OpenFolderDialog ofd = new OpenFolderDialog()
            {
                Title = "生成用jsonのあるフォルダを選択してください"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadPath = ofd.Path;
                jsons = pDf.loadfiles(loadPath);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog()
            {
                Title = "保存先フォルダを選択してください"
            };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.Path;
                savePath = ofd.Path;
            }
        }
    }
}
