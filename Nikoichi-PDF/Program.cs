using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikoichi_PDF
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 実行ファイルにドラッグ&ドロップされたファイルを取得
            string[] exeDaDfiles = Environment.GetCommandLineArgs().Skip(1).ToArray();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form1 = new Form1();
            form1.exeDaDFiles = exeDaDfiles;
            Application.Run(form1);
        }
    }
}
