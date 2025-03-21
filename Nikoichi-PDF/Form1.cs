using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Nikoichi_PDF
{
    public partial class Form1: Form
    {
        public string[] exeDaDFiles { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        public string savePath = null;
        public string loadPath = null;
        public List<nikoJsonWrap> jsons = new List<nikoJsonWrap>();

        private bool ReDrawlist_moto()
        {
            bool flg = false;
            listView1.Items.Clear();
            List<string[]> strings = new List<string[]>();
            foreach(nikoJsonWrap values in jsons)
            {
                foreach(RequiredFile reqfile in values.RequiredFiles)
                {
                    string[] ins = { reqfile.FilePath, reqfile.fileExists == true ? "はい" : "いいえ" };
                    if (flg == false && reqfile.fileExists == false) flg = true;
                    strings.Add(ins);
                }
            }
            strings.Distinct();
            foreach (string[] values1 in strings)
            {
                listView1.Items.Add(new ListViewItem(values1));
            }
            if(flg)MessageBox.Show("読み込んだ指定PDFのうち、読み込めないものがありました。\r\nフォルダ内をご確認ください。", "ファイルがみつかりません", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return flg;
        }

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
                savePath = ofd.Path + @"\generate\";
                textBox1.Text = ofd.Path + @"\generate\";
                jsons = pDf.loadfiles(loadPath);
                ReDrawList_Gen();
                ReDrawlist_moto();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(exeDaDFiles.Length > 0)
            {
                if(MessageBox.Show($"ディレクトリが入力されました\r\n{exeDaDFiles[0]}\r\n\r\nPDFファイル生成を行いますか？", "Nikoichi-PDF 簡易モード", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    PDFGen pDf = new PDFGen();
                    loadPath = exeDaDFiles[0];
                    savePath = exeDaDFiles[0] + @"\generate\";
                    textBox1.Text = exeDaDFiles[0] + @"\generate\";
                    jsons = pDf.loadfiles(exeDaDFiles[0]);
                    ReDrawList_Gen();
                    if (ReDrawlist_moto()) return;
                    string files = "";
                    foreach(nikoJsonWrap values in jsons)
                    {
                        files += $"\r\n・{values.ExportFileName}.pdf";
                    }
                    MessageBox.Show($"以下のファイルが生成されます。{files}", "Nikoichi-PDF 簡易モード", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    try
                    {
                        foreach (nikoJsonWrap niko in jsons)
                        {
                            pDf.Save_PDF(pDf.makePDF(niko), savePath + niko.ExportFileName + ".pdf", savePath);
                        }
                        MessageBox.Show($"保存に成功しました(ファイル数:{jsons.Count})", "Nikoichi-PDF 簡易モード", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    catch
                    {
                        MessageBox.Show("保存に失敗しました", "Nikoichi-PDF 簡易モード", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            panel1.AllowDrop = true;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog()
            {
                Title = "保存先フォルダを選択してください"
            };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.Path+@"\generate\";
                savePath = ofd.Path + @"\generate\";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PDFGen pg = new PDFGen();
            try
            {
                foreach (nikoJsonWrap niko in jsons)
                {
                    pg.Save_PDF(pg.makePDF(niko), savePath + niko.ExportFileName + ".pdf", savePath);
                }
                MessageBox.Show($"保存に成功しました(ファイル数:{jsons.Count})", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("保存に失敗しました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            PDFGen pDf = new PDFGen();
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s != null)
            {
                loadPath = s[0];
                savePath = s[0] + @"\generate\";
                textBox1.Text = s[0] + @"\generate\";
                jsons = pDf.loadfiles(s[0]);
                ReDrawList_Gen();
                ReDrawlist_moto();
            }
        }
    }
}
