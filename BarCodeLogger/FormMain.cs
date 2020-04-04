using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace BarCodeLogger
{
    public partial class FormMain : Form
    {
        private DataTable dtCurrent = new DataTable("BarcodeDataCurrent");
        private DataTable dtHist = new DataTable("BarcodeDataHist");
        private string histFilePath = Path.Combine(System.Environment.CurrentDirectory, @"RawData\BarcodeDataHist.csv");

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            dtInitial(dtCurrent);
            dtInitial(dtHist);
            this.dgvData.DataSource = dtCurrent;
            //Load Hist
            if (File.Exists(histFilePath))
            {
                dtHist = csvLoadToDt(dtHist, histFilePath, ",");
            }
        }

        private void dtInitial(DataTable dt)
        {
            dt.Columns.Add("BarCode", typeof(String));
            dt.Columns.Add("ReciveDate", typeof(String));
            dt.Columns.Add("UpdateDate", typeof(String));
            dt.Columns.Add("UpdateResult", typeof(String));
        }

        public enum dataColName
        {
            BarCode, ReciveDate, UpdateDate, UpdateResult
        }

        public bool readerRaw2dt(string s)
        {
            try
            {
                foreach (string r in s.Split(new char[] { '\r', '\n' }))
                {
                    var d = r.Split(',');
                    if (d.Length == 2)
                    {
                        DataRow row = dtCurrent.NewRow();
                        row[dataColName.BarCode.ToString()] = Convert.ToString(d[0]);
                        row[dataColName.ReciveDate.ToString()] = Convert.ToString(d[1]);
                        dtCurrent.Rows.Add(row);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public void dtSaveToCSV(DataTable oTable, string FilePath)
        {
            string data = "";
            StreamWriter wr = new StreamWriter(FilePath, false, System.Text.Encoding.Default);
            foreach (DataColumn column in oTable.Columns)
            {
                data += column.ColumnName + ",";
            }
            data = data.Substring(0, data.Length - 1);
            data += "\n";
            wr.Write(data);
            data = "";

            foreach (DataRow row in oTable.Rows)
            {
                foreach (DataColumn column in oTable.Columns)
                {
                    data += row[column].ToString().Trim() + ",";
                }
                data = data.Substring(0, data.Length - 1);
                data += "\n";
                wr.Write(data);
                data = "";
            }

            wr.Dispose();
            wr.Close();
        }

        public DataTable csvLoadToDt(DataTable dt, string FilePath, string delimiter)
        {
            StreamReader s = new StreamReader(FilePath, Encoding.Default);
            string ss = s.ReadLine();//skip the first line
            #region *no need
            //string[] columns = s.ReadLine().Split(delimiter.ToCharArray());
            //foreach (string col in columns)
            //{
            //    bool added = false;
            //    string next = "";
            //    int i = 0;
            //    while (!added)
            //    {
            //        string columnname = col + next;
            //        columnname = columnname.Replace("#", "");
            //        columnname = columnname.Replace("'", "");
            //        columnname = columnname.Replace("&", "");

            //        if (!dt.Columns.Contains(columnname))
            //        {
            //            dt.Columns.Add(columnname.ToUpper());
            //            added = true;
            //        }
            //        else
            //        {
            //            i++;
            //            next = "_" + i.ToString();
            //        }
            //    }
            //}
            #endregion
            string AllData = s.ReadToEnd();
            string[] rows = AllData.Split("\n".ToCharArray());

            foreach (string r in rows)
            {
                if (r.Length > 0)
                {
                    string[] d = r.Split(delimiter.ToCharArray());
                    dt.Rows.Add(d);
                }
            }

            s.Close();
            return dt;
        }

        private void btLoadData_Click(object sender, EventArgs e)
        {
            //Hide MainForm
            this.Hide();
            FormDataReciveBarcode formDRB = new FormDataReciveBarcode();
            formDRB.ShowDialog(this);

            //TODO 移除已處理Barcode資料form dtCurrent
            int deleteCnt = 0;
            for (int i = dtCurrent.Rows.Count - 1; i >= 0; i--)
            {
                var dts = dtHist.Select(String.Format("BarCode='{0}' and ReciveDate='{1}'", dtCurrent.Rows[i]["BarCode"], dtCurrent.Rows[i]["ReciveDate"]));
                if (dts.Length > 0)
                {
                    dtCurrent.Rows[i].Delete();
                    deleteCnt++;
                }
            }
            dtCurrent.AcceptChanges();

            //DataGridView Show
            this.Show();
            this.dgvData.DataSource = null;
            this.dgvData.DataSource = dtCurrent;

            if (dtCurrent.Rows.Count > 0)
            {
                //Save Current DataTable to RawData
                string filePath = Path.Combine(System.Environment.CurrentDirectory, String.Format(@"RawData\ReaderRaw{0}.csv", DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    //新增資料夾
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                dtSaveToCSV(dtCurrent, filePath);
            }
            if (deleteCnt > 0) { MessageBox.Show("已移除" + deleteCnt + "筆重複資料"); }

        }

        private void btBarcodeRecive_Click(object sender, EventArgs e)
        {
            if (dtCurrent.Rows.Count > 0)
            {
                //Recive the barcode by TAP
                //WCF????
                dtHist.Merge(dtCurrent);
                //TODO　移除重複資料
                dtSaveToCSV(dtHist, histFilePath);
            }
            else { MessageBox.Show("沒有資料可新增!!"); }

        }

        private void btQueryHistroy_Click(object sender, EventArgs e)
        {
            dgvData.DataSource = null;
            dgvData.DataSource = dtHist;
        }
    }
}

