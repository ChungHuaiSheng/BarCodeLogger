using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarCodeLogger
{
    public partial class FormDataReciveBarcode : Form
    {
        public FormDataReciveBarcode()
        {
            InitializeComponent();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            FormMain formMain = (FormMain)this.Owner;
            //formMain.textToListBarcodeData(tbData.Text.ToString());
            if (formMain.readerRaw2dt(tbData.Text.ToString()))
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("請確認窗格內資料是否有誤");
            }
            
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            tbData.Clear();
        }
    }
}
