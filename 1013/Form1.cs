using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SQLite;

// ScottPlot start
using ScottPlot.Plottable;
using static OfficeOpenXml.ExcelErrorValue;
using System.Collections;
using ScottPlot;
// ScottPlot end


namespace _1013
{
    public partial class Form1 : Form
    {
        int index = 1; //for db

        // ScottPlot start
        private Crosshair Crosshair;
        // ScottPlot end

        public Form1()
        {
            InitializeComponent();

            // 打開程式後，跳出Form2，確認帳號密碼
            Form2 form2;
            form2 = new Form2();
            form2.ShowDialog();

            //資料庫讀取
            Load_DB();
            Show_DB();

            //設定成最後一筆ID
            label5.Text = index.ToString();

            // ScottPlot start
            Crosshair = formsPlot1.Plot.AddCrosshair(0, 0);
            formsPlot1.Refresh();
            // ScottPlot end


            //更新顯示圖表
            updateChart();
        }

        public class DBConfig
        {
            //log.db要放在【bin\Debug底下】      
            public static string dbFile = Application.StartupPath + @"\log.db";

            public static string dbPath = "Data source=" + dbFile;

            public static SQLiteConnection sqlite_connect;
            public static SQLiteCommand sqlite_cmd;
            public static SQLiteDataReader sqlite_datareader;
        }

        private void Load_DB()
        {
            DBConfig.sqlite_connect = new SQLiteConnection(DBConfig.dbPath);
            DBConfig.sqlite_connect.Open();// Open

        }

        private void Show_DB()
        {
            this.dataGridView1.Rows.Clear();

            string sql = @"SELECT * from record;";
            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_datareader = DBConfig.sqlite_cmd.ExecuteReader();

            if (DBConfig.sqlite_datareader.HasRows)
            {
                while (DBConfig.sqlite_datareader.Read()) //read every data
                {
                    
                    int _serial = Convert.ToInt32(DBConfig.sqlite_datareader["serial"]);
                    int _date = Convert.ToInt32(DBConfig.sqlite_datareader["date"]);
                    int _type = Convert.ToInt32(DBConfig.sqlite_datareader["type"]);
                    string _name = Convert.ToString(DBConfig.sqlite_datareader["name"]);
                    double _price = Convert.ToDouble(DBConfig.sqlite_datareader["price"]);
                    double _number = Convert.ToDouble(DBConfig.sqlite_datareader["number"]);
                    double _total = _price * _number;

                    string _date_str = DateTimeOffset.FromUnixTimeSeconds(_date).ToString("yy-MM-dd hh:mm:ss");

                    string _type_str = "";
                    if (_type == 0)
                    { _type_str = "進貨"; }
                    else { _type_str = "出貨"; }

                    index = _serial;
                    DataGridViewRowCollection rows = dataGridView1.Rows;
                    rows.Add(new Object[] { index, _date_str, _type_str, _name, _price, _number, _total });
                }
                DBConfig.sqlite_datareader.Close();
            }
        }

        public void updateChart()
        {
            // 1. 進貨統計

            string sql = @"SELECT * from record where type = 0;";
            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_datareader = DBConfig.sqlite_cmd.ExecuteReader();

            Dictionary<string, double> _stocks_bar_out = new Dictionary<string, double>();
            Dictionary<string, double> _stocks_bar_out_sum = new Dictionary<string, double>();

            if (DBConfig.sqlite_datareader.HasRows)
            {
                while (DBConfig.sqlite_datareader.Read()) //read every data
                {
                    string _name = Convert.ToString(DBConfig.sqlite_datareader["name"]);
                    double _price = Convert.ToDouble(DBConfig.sqlite_datareader["price"]);
                    double _number = Convert.ToDouble(DBConfig.sqlite_datareader["number"]);
                    if (!_stocks_bar_out.ContainsKey(_name))
                    {
                        _stocks_bar_out.Add(_name, 0);
                        _stocks_bar_out_sum.Add(_name, 0);
                    }
                    _stocks_bar_out[_name] = _stocks_bar_out[_name] + _number;
                    _stocks_bar_out_sum[_name] = _stocks_bar_out_sum[_name] + _number * _price;
                }
                DBConfig.sqlite_datareader.Close();
            }



            this.chart1.Series["stocks"].Points.Clear();
            foreach (var OneItem in _stocks_bar_out)
            {
                this.chart1.Series["stocks"].Points.AddXY(OneItem.Key, OneItem.Value);
            }

            this.chart2.Series["stocks"].Points.Clear();
            foreach (var OneItem in _stocks_bar_out_sum)
            {
                this.chart2.Series["stocks"].Points.AddXY(OneItem.Key, OneItem.Value);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string _name = "";
            long _date = 0;
            int _stock_type = 0;
            double _price = 0;
            double _number = 0;
            double _sum = 0;

            // 抓取textbox的資料
            _name = comboBox1.Text;
            _price = Convert.ToDouble(textBox1.Text);
            _number = Convert.ToDouble(textBox2.Text);

            _sum = _price * _number;
            _date = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (radioButton1.Checked == true)
            {
                _stock_type = 0;
            }
            else
            {
                _stock_type = 1;
            }
            // update
            this.index = this.index + 1;

            // add item into database

            string sql = @"INSERT INTO record (date, type, name,price,number)
                VALUES( "
                       + " '" + _date.ToString() + "' , "
                       + " '" + _stock_type.ToString() + "' , "
                       + " '" + _name.ToString() + "' , "
                       + " '" + _price.ToString() + "' , "
                       + " '" + _number.ToString() + "'   "
                      + ");";

            string ot = String.Format("Insert : {0} , {1} , {2} , {3} , {4}", _date.ToString(), _stock_type.ToString(), _name.ToString(), _price.ToString(), _number.ToString());
            richTextBox1.AppendText(ot);


            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_cmd.ExecuteNonQuery();
            

            // show database in the gui
            Show_DB();
            updateChart(); // function的最後面加上這一行
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string _name = "";
            int _serial = 0;
            int _stock_type = 0;
            double _price = 0;
            double _number = 0;

            if (radioButton1.Checked == true)
            {
                _stock_type = 0;
            }
            else
            {
                _stock_type = 1;
            }

            // 抓取textbox的資料
            _name = comboBox1.Text;


            _price = Convert.ToDouble(textBox1.Text);
            _number = Convert.ToDouble(textBox2.Text);
            _serial = Convert.ToInt32(textBox3.Text);


            string sql = @"UPDATE record " +
                      " SET name = '" + _name + "',"
                        + " type = '" + _stock_type.ToString() + "' , "
                        + " price = '" + _price.ToString() + "',"
                        + " number = '" + _number.ToString() + "' "
                        + "   where serial = " + _serial.ToString() + ";";

            string ot = String.Format("Update : {0} , {1} , {2} , {3} , {4}", _name, _stock_type.ToString(), _price.ToString(), _number.ToString(), _serial.ToString());
            richTextBox1.AppendText(ot);

            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_cmd.ExecuteNonQuery();
            Show_DB();
            updateChart(); // function的最後面加上這一行
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            save.FileName = "Export_Data";
            save.Filter = "*.xlsx|*.xlsx";
            if (save.ShowDialog() != DialogResult.OK) return;

            // Excel 物件
            Microsoft.Office.Interop.Excel.Application xls = null;
            try
            {
                xls = new Microsoft.Office.Interop.Excel.Application();
                // Excel WorkBook
                Microsoft.Office.Interop.Excel.Workbook book = xls.Workbooks.Add();
                //Excel.Worksheet Sheet = (Excel.Worksheet)book.Worksheets[1];
                Microsoft.Office.Interop.Excel.Worksheet Sheet = xls.ActiveSheet;

                // 把 DataGridView 資料塞進 Excel 內

                // DataGridView 標題
                for (int k = 0; k < this.dataGridView1.Columns.Count; k++)
                {
                    Sheet.Cells[1, k + 1] = this.dataGridView1.Columns[k].HeaderText.ToString();
                }

                // DataGridView 內容
                for (int i = 0; i < this.dataGridView1.Rows.Count - 1; i++)
                {
                    for (int j = 0; j < this.dataGridView1.Columns.Count; j++)
                    {
                        string value = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        Sheet.Cells[i + 2, j + 1] = value;
                    }
                }

                // 儲存檔案
                book.SaveAs(save.FileName);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                xls.Quit();
            }

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection selRowData = dataGridView1.Rows[e.RowIndex].Cells;

            string _type = "";
            _type = Convert.ToString(selRowData[2].Value);

            if (_type.Equals("進貨"))
            {
                radioButton1.Checked = true;
            }
            else
            {
                radioButton2.Checked = true;
            }


            this.comboBox1.Text = Convert.ToString(selRowData[3].Value);
            this.textBox1.Text = Convert.ToString(selRowData[4].Value);
            this.textBox2.Text = Convert.ToString(selRowData[5].Value);
            this.label5.Text = Convert.ToString(selRowData[0].Value);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            save.FileName = "Export_bar_Chart_Data";
            save.Filter = "*.xlsx|*.xlsx";
            if (save.ShowDialog() != DialogResult.OK) return;

            // Excel 物件

            Microsoft.Office.Interop.Excel.Application xls = null;
            try
            {
                xls = new Microsoft.Office.Interop.Excel.Application();
                // Excel WorkBook
                Microsoft.Office.Interop.Excel.Workbook book = xls.Workbooks.Add();
                //Excel.Worksheet Sheet = (Excel.Worksheet)book.Worksheets[1];
                Microsoft.Office.Interop.Excel.Worksheet Sheet = xls.ActiveSheet;

                // 把資料塞進 Excel 內

                // 標題
                Sheet.Cells[1, 1] = "標籤";
                Sheet.Cells[1, 2] = "數量";

                // 內容
                for (int k = 0; k < this.chart1.Series["stocks"].Points.Count; k++)
                {
                    Sheet.Cells[k + 2, 1] = this.chart1.Series["stocks"].Points[k].AxisLabel.ToString();
                    Sheet.Cells[k + 2, 2] = this.chart1.Series["stocks"].Points[k].YValues[0].ToString();
                }


                // 儲存檔案
                book.SaveAs(save.FileName);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                xls.Quit();
            }
        }
        
        private void label6_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            save.FileName = "Export_bar_Chart1_JPG";
            save.Filter = "*.jpg|*.jpg";
            if (save.ShowDialog() != DialogResult.OK) return;

            chart1.SaveImage(save.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            save.FileName = "Export_bar_Chart1_JPG";
            save.Filter = "*.jpg|*.jpg";
            if (save.ShowDialog() != DialogResult.OK) return;

            chart1.SaveImage(save.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        private void button8_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            save.FileName = "Export_pie_Chart_Data";
            save.Filter = "*.xlsx|*.xlsx";
            if (save.ShowDialog() != DialogResult.OK) return;

            // Excel 物件

            Microsoft.Office.Interop.Excel.Application xls = null;
            try
            {
                xls = new Microsoft.Office.Interop.Excel.Application();
                // Excel WorkBook
                Microsoft.Office.Interop.Excel.Workbook book = xls.Workbooks.Add();
                //Excel.Worksheet Sheet = (Excel.Worksheet)book.Worksheets[1];
                Microsoft.Office.Interop.Excel.Worksheet Sheet = xls.ActiveSheet;

                // 把資料塞進 Excel 內

                // 標題
                Sheet.Cells[1, 1] = "標籤";
                Sheet.Cells[1, 2] = "總價格";

                // 內容
                for (int k = 0; k < this.chart2.Series["stocks"].Points.Count; k++)
                {
                    Sheet.Cells[k + 2, 1] = this.chart2.Series["stocks"].Points[k].AxisLabel.ToString();
                    Sheet.Cells[k + 2, 2] = this.chart2.Series["stocks"].Points[k].YValues[0].ToString();
                }


                // 儲存檔案
                book.SaveAs(save.FileName);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                xls.Quit();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            save.FileName = "Export_Chart_pie_JPG";
            save.Filter = "*.jpg|*.jpg";
            if (save.ShowDialog() != DialogResult.OK) return;

            chart2.SaveImage(save.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        

        private void button9_Click(object sender, EventArgs e)
        {
            //stamp 物品數值
            List<double> Bandage = new List<double> { };
            List<double> Alcohol = new List<double> { };
            List<double> Mask = new List<double> { };
            List<double> Thermometer = new List<double> { };
            List<double> Wetwipes = new List<double> { };

            //用plt這個變數，當作【圖表數據】的捷徑
            var plt = formsPlot1.Plot;
            
            plt.Title("進出貨統計圖表");
            
            string sql = @"SELECT * FROM record WHERE type = 0;";

            using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect))
            {
                using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                {
                    Dictionary<string, double> _stocks_bar_out = new Dictionary<string, double>();
                    Dictionary<string, double> _stocks_bar_out_sum = new Dictionary<string, double>();
                    
                    //遍歷
                    while (sqlite_datareader.Read())
                    {
                        //convert datetime to string
                        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeSeconds((long)sqlite_datareader["date"]);
                        string timestampString = timestamp.ToString("yyyy-MM-dd");
                        //get column data
                        string name = sqlite_datareader["name"].ToString();
                        double price = Convert.ToDouble(sqlite_datareader["price"]);
                        double number = Convert.ToDouble(sqlite_datareader["number"]);
                        
                        switch (name)
                        {
                            case "繃帶":
                                Bandage.Add(number);
                                break;
                            case "酒精":
                                Alcohol.Add(number);
                                break;
                            case "口罩":
                                Mask.Add(number);
                                break;
                            case "溫度計":
                                Thermometer.Add(number);
                                break;
                            case "濕紙巾":
                                Wetwipes.Add(number);
                                break;
                            }

                    }

                    // 使用 AddSignal 方法添加折線圖
                    double[] arrBandage = Bandage.ToArray();
                    double[] arrAlcohol = Alcohol.ToArray();
                    double[] arrMask = Mask.ToArray();
                    double[] arrThermometer = Thermometer.ToArray();
                    double[] arrWetwipes = Wetwipes.ToArray();

                    for (int i = 1; i < arrBandage.Length; arrBandage[i] += arrBandage[i - 1], i++) ;
                    for (int i = 1; i < arrAlcohol.Length; arrAlcohol[i] += arrAlcohol[i - 1], i++) ;
                    for (int i = 1; i < arrMask.Length; arrMask[i] += arrMask[i - 1], i++) ;
                    for (int i = 1; i < arrThermometer.Length; arrThermometer[i] += arrThermometer[i - 1], i++) ;
                    for (int i = 1; i < arrWetwipes.Length; arrWetwipes[i] += arrWetwipes[i - 1], i++) ;


                    int n1 = arrBandage.Length;
                    int n2 = arrAlcohol.Length;
                    int n3 = arrMask.Length;
                    int n4 = arrThermometer.Length;
                    int n5 = arrWetwipes.Length;
                    double[] X1 = Enumerable.Range(1, n1 * 50).Where(x => x % 50 == 0).Select(x => (double)x).ToArray();
                    double[] X2 = Enumerable.Range(1, n2 * 50).Where(x => x % 50 == 0).Select(x => (double)x).ToArray();
                    double[] X3 = Enumerable.Range(1, n3 * 50).Where(x => x % 50 == 0).Select(x => (double)x).ToArray();
                    double[] X4 = Enumerable.Range(1, n4 * 50).Where(x => x % 50 == 0).Select(x => (double)x).ToArray();
                    double[] X5 = Enumerable.Range(1, n5 * 50).Where(x => x % 50 == 0).Select(x => (double)x).ToArray();


                    plt.AddScatter(X1, arrBandage, color: Color.Red,label:"繃帶");
                    plt.AddScatter(X2, arrAlcohol, color: Color.Aqua, label: "酒精");
                    plt.AddScatter(X3, arrMask, color: Color.Black, label: "口罩");
                    plt.AddScatter(X4, arrThermometer, color: Color.Brown, label: "溫度計");
                    plt.AddScatter(X5, arrWetwipes, color: Color.DimGray, label: "濕紙巾");
                    


                    




                    // 顯示圖例
                    plt.Legend();

                    // 顯示標題
                    plt.Title("存貨數量");

                    // 顯示 x 軸和 y 軸標籤
                    plt.XLabel("走勢");
                    plt.YLabel("剩餘數量");

                    // 更新圖表
                    formsPlot1.Refresh();
                }
            }

            plt.SetAxisLimits(0, 5, -25, 25);
            formsPlot1.Refresh();
        }

        // 滑鼠移動時，顯示座標
        private void formsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            (double coordinateX, double coordinateY) =
                                                 formsPlot1.GetMouseCoordinates();

            Crosshair.X = coordinateX;
            Crosshair.Y = coordinateY;

            formsPlot1.Refresh(lowQuality: true, skipIfCurrentlyRendering: true);
        }

        // 滑鼠移動進入圖表時，顯示座標
        private void formsPlot1_MouseEnter(object sender, EventArgs e)
        {
            Crosshair.IsVisible = true;
        }

        // 滑鼠移動離開圖表時，關閉顯示座標
        private void formsPlot1_MouseLeave(object sender, EventArgs e)
        {
            Crosshair.IsVisible = false;
            formsPlot1.Refresh();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int _serial = Convert.ToInt32(textBox3.Text);

            string sql = @"DELETE FROM record " +
                         "WHERE serial = " + _serial.ToString() + ";";
            string ot = String.Format("DELETE : ID = {0}", _serial.ToString());
            richTextBox1.AppendText(ot);

            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_cmd.ExecuteNonQuery();
            Show_DB();
            updateChart(); // function的最後面加上這一行
        }
    }
}
