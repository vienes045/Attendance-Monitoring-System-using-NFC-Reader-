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
    /// Interaction logic for AboutUs.xaml
    /// </summary>
    public partial class AboutUs : Window
    {
        public AboutUs()
        {
            InitializeComponent();
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

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Home aizen7 = new Home();
            aizen7.Show();
            this.Close();
        }
    }
}
