using System.Windows;
using System.Windows.Input;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace DatabaseFinalProject
{
    /// <summary>
    /// Interaction logic for CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        public CreateAccount()
        {
            InitializeComponent();
        }

        private void create_acc_btn_press(object sender, RoutedEventArgs e)
        {
            //if blank field
            if (set_username.Text == "" || set_password.Password == "" || set_firstname.Text == "" || set_lastname.Text == "" || set_major.Text == "")
                MessageBox.Show("*****ERROR***** \nPlease fill out all fields");
            else
            {


                string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=2&fname=" + set_firstname.Text + "&lname=" + set_lastname.Text + "&uname=" + set_username.Text;
                url += "&pass=" + set_password.Password + "&major=" + set_major.Text + "&phone_number=" + set_phone.Text + "&email=" + "&address=" + set_address.Text;

                using (var wc = new WebClient())
                {
                    var output= wc.DownloadString(url);

                    if (output == "account exists")
                        MessageBox.Show("This Username has already been claimed");
                    else if (output != "Complete")
                        MessageBox.Show("*****ERROR***** \nTechincal issue. Failed to create account.");
                    else
                    {
                        new MainWindow().Show();
                        this.Close();
                    }
                }                
            }
        }

        //Credit to: http://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
        private void ValidateNumber(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //if cancel btn clicked
        private void cancel_acc_creation(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void set_major_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> majors = new List<string>();
            majors.Add("");
            //bring in all departments to populate dropdown box
            using (var wc = new WebClient())
            {
                string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=9";

                var json = wc.DownloadString(url);

                if (json == "[]") { }//something bad happened
                else
                {
                    dynamic result = JArray.Parse(json);

                    foreach (var tuple in result)
                    {
                        string curr = tuple.Major;
                        majors.Add(curr);
                    }
                }
            }

            //

            //assign values to combobox
            var combo = sender as ComboBox;
            combo.ItemsSource = majors;
            combo.SelectedIndex = 0;
        }
    }
}
