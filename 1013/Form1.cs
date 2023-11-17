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



namespace _1013
{
    public partial class Form1 : Form
    {
        int index = 1; //for db

        public Form1()
        {
            InitializeComponent();
            // 打開程式後，跳出Form2，確認帳號密碼
            Form2 form2;
            form2 = new Form2();
            form2.ShowDialog();

            Load_DB();
            Show_DB();
            label5.Text = index.ToString();

        }

        public class DBConfig
        {
            //log.db要放在【bin\Debug底下】      
            public static string dbFile = Application.StartupPath + @"\DATA.db";

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
            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_cmd.ExecuteNonQuery();

            // show database in the gui
            Show_DB();

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
            _serial = Convert.ToInt32(label5.Text);


            string sql = @"UPDATE record " +
                      " SET name = '" + _name + "',"
                        + " type = '" + _stock_type.ToString() + "' , "
                        + " price = '" + _price.ToString() + "',"
                        + " number = '" + _number.ToString() + "' "
                        + "   where serial = " + _serial.ToString() + ";";


            DBConfig.sqlite_cmd = new SQLiteCommand(sql, DBConfig.sqlite_connect);
            DBConfig.sqlite_cmd.ExecuteNonQuery();
            Show_DB();  
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {

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
    }
}
