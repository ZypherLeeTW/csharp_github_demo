using System;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace _1013
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            // 打開程式後，跳出Form2，確認帳號密碼
            Form2 form2;
            form2 = new Form2();
            form2.ShowDialog();

        }


        private void button1_Click(object sender, EventArgs e)
        {
            string i_input_price = textBox1.Text;

            string i_input_num = textBox2.Text;

            double _price = Convert.ToDouble(i_input_price);

            double _num = Convert.ToDouble(i_input_num);

            double total = 0;

            total = _price * _num;

            string _radiobutton_log = "";

            if (radioButton1.Checked == true)

            { _radiobutton_log = "進貨"; }

            else

            { _radiobutton_log = "出貨"; }

            string _combobox_log = comboBox1.SelectedItem.ToString();

            richTextBox1.Text = String.Format("{0} : {1} {2} "

            , _price * _num, _radiobutton_log, _combobox_log);

            DataGridViewRowCollection rows = dataGridView1.Rows;

            DateTime date = DateTime.Now; // 現在時間

            rows.Add(new Object[] { "", date.ToString("yyyy/MM/dd HH:mm:ss")
                , _radiobutton_log, _combobox_log, _price, _num, _price * _num });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 非商業用途

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            saveFileDialog.Title = "Save as Excel File";
            saveFileDialog.FileName = "exported_data.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Data");

                    // 將dataGridView1中的資料寫入工作表
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1].Value = dataGridView1[j, i].Value;
                        }
                    }

                    // 設定標題行格式
                    worksheet.Row(1).Style.Font.Bold = true;

                    // 儲存工作簿到指定路徑
                    File.WriteAllBytes(filePath, package.GetAsByteArray());
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

    }
}
