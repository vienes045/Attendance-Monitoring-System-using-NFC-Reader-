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
using System.Windows.Media.Animation;


namespace ReadWriteRFID
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

    
       
        

        

        private void btnAttendance_Click(object sender, RoutedEventArgs e)
        {
            AddEvent aizen3 = new AddEvent();
            aizen3.Show();
            this.Close();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            MainWindow register = new MainWindow();
            register.Show();
            this.Close();
        }

     

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Animation.DoubleAnimation doubleAnimation = new System.Windows.Media.Animation.DoubleAnimation();
            doubleAnimation.From = this.ActualWidth;
            doubleAnimation.To = -lblName.ActualWidth;
            doubleAnimation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(20)); // provide an appropriate  duration 
            lblName.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            Report rep = new Report();
            rep.Show();
            this.Close();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
           
            Update aizen5 = new Update();
            aizen5.Show();
            this.Close();
        }

        private void btnAttendance_Click_1(object sender, RoutedEventArgs e)
        {
            Select aizen6 = new Select();
            aizen6.Show();
            this.Close();
        }

        private void btnRegister_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow register = new MainWindow();
            register.Show();
            this.Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            System.Windows.Media.Animation.DoubleAnimation doubleAnimation = new System.Windows.Media.Animation.DoubleAnimation();
            doubleAnimation.From = this.ActualWidth;
            doubleAnimation.To = -lblName.ActualWidth;
            doubleAnimation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(20)); // provide an appropriate  duration 
            lblName.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AboutUs about = new AboutUs();
            about.Show();
            this.Close();
        }
    }
}
