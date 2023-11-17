using System;
using System.Windows.Forms;

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
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {

            

            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
