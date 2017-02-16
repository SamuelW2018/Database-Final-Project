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
using System.Net;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;

namespace DatabaseFinalProject
{
    /// <summary>
    /// Interaction logic for AdvisorsMainPage.xaml
    /// </summary>
    public partial class AdvisorsMainPage : Window
    {
        public AdvisorsMainPage()
        {
            InitializeComponent();
            cancel_bttn.IsHitTestVisible = false;
            confirm_bttn.IsHitTestVisible = false;

    }

        private void dropBttn_Click(object sender, RoutedEventArgs e)
        {
            
            //Swap button visibility and .... clickability?
            dropBttn.Background = Brushes.White;
            dropBttn.Foreground = Brushes.White;
            dropBttn.BorderBrush = Brushes.White;
            dropBttn.IsHitTestVisible = false;

            confirm_bttn.BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            confirm_bttn.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            confirm_bttn.Foreground = Brushes.Black;
            confirm_bttn.IsHitTestVisible = true;

            cancel_bttn.BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            cancel_bttn.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            cancel_bttn.Foreground = Brushes.Black;
            cancel_bttn.IsHitTestVisible = true;

            confirm_text.Foreground = Brushes.Black;
        }

        private void confirm_deny_Click(object sender, RoutedEventArgs e)
        {
            Button bttn = (Button)sender;


            if(bttn.Name == "confirm_bttn")
            {
                foreach (Student stu in Registrar.get_shared_instance().Selected_students)
                {
                    if (stu.IsChecked)
                    {
                        stu.IsChecked = false;
                        string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=12&sid=" + stu.ID;

                        using (var wc = new WebClient())
                        {
                            var output = wc.DownloadString(url);

                            if (output != "Complete")
                                MessageBox.Show("*****ERROR***** \nFailed to drop student.");
                        }
                    }
                }

                //refresh list
                tabControl.SelectedIndex = 1;
                tabControl.SelectedIndex = 0;
            }

            //reset buttons
            dropBttn.BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            dropBttn.Background = new SolidColorBrush(Color.FromRgb(226, 77, 77));
            dropBttn.Foreground = Brushes.Black;
            dropBttn.IsHitTestVisible = true;

            confirm_bttn.BorderBrush = Brushes.White;
            confirm_bttn.Background = Brushes.White;
            confirm_bttn.Foreground = Brushes.White;
            confirm_bttn.IsHitTestVisible = false;

            cancel_bttn.BorderBrush = Brushes.White;
            cancel_bttn.Background = Brushes.White;
            cancel_bttn.Foreground = Brushes.White;
            cancel_bttn.IsHitTestVisible = false;

            confirm_text.Foreground = Brushes.White;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl item = (TabControl)sender;
            TabItem selected = item.SelectedItem as TabItem;
            if(selected.Name == "advisees")
            {
                string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=11&aid=" + Registrar.get_shared_instance().Curr_Prof.ID;
                List<Student> advise = new List<Student>();

                using (var wc = new WebClient())
                {
                    var json = wc.DownloadString(url);

                    dynamic par = JArray.Parse(json);
                    foreach (dynamic tuple in par)
                    {
                        string f_name = tuple.First_Name;
                        string l_name = tuple.Last_Name;
                        int id = tuple.id;
                        advise.Add(new Student(id, f_name, l_name));
                    }


                    Registrar.get_shared_instance().Selected_students = advise;
                }

                advisees_list.ItemsSource = advise;

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(advisees_list.ItemsSource);
                view.SortDescriptions.Add(new SortDescription("L_name", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("F_name", ListSortDirection.Ascending));
            }
            else if(selected.Name == "Personal_Info")
            {
                display_fname.Text = Registrar.get_shared_instance().Curr_Prof.Fname;
                display_email.Text = Registrar.get_shared_instance().Curr_Prof.Email;
                display_lname.Text = Registrar.get_shared_instance().Curr_Prof.Lname;
                display_phone.Text = string.Format("{0}", Registrar.get_shared_instance().Curr_Prof.Phone_Num);
                display_startD.Text = Registrar.get_shared_instance().Curr_Prof.StartDate;
            }
        }
        

        private void moreInfoBttn_Click(object sender, RoutedEventArgs e)
        {
            foreach(Student curr in Registrar.get_shared_instance().Selected_students)
            {
                if(curr.IsChecked)
                    new AdviseeInfo(curr).Show();
            }

        }

        private void update_btn_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=14&id=" + Registrar.get_shared_instance().Curr_Prof.ID + "&user=" + change_user.Text + "&pass=" + change_password.Password;

            using (var wc = new WebClient())
            {
                var output = wc.DownloadString(url);

                if (output != "Complete")
                {
                    MessageBox.Show("*****ERROR***** \n Failed to update professor information.");
                }

            }
        }
    }
}
