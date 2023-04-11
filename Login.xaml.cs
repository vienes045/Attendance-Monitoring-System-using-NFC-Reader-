using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.OleDb;

namespace ReadWriteRFID
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            

                OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
                OleDbCommand cmd = con.CreateCommand();
                con.Open();
                cmd = new OleDbCommand("select * from tbl_Admin where Username ='" + txtUsername.Text + "' and Password='" + passBox.Password + "'", con);
                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read()){
                if (txtUsername.Text == reader[1].ToString() && passBox.Password == reader[2].ToString())
                {
                    MessageBox.Show("Login Successful.");

                    Home win = new Home();
                    win.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Username or password is incorrect");
                    txtUsername.Text = "";
                    passBox.Clear();
                }
                }
                reader.Close();
                con.Close();
                 
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
