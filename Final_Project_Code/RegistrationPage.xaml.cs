using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace DatabaseFinalProject
{
    /// <summary>
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Window
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void class_search(object sender, RoutedEventArgs e)
        {
            //create query link and get results
            string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=3&dept=" + dept_combo.Text + "&num=" + class_num.Text + "&name=" + class_title.Text;

            //parse results into Classes object
            List<Classes> search = new List<Classes>();
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                
                dynamic par = JArray.Parse(json);
                foreach(dynamic tuple in par)
                {
                    string dep = tuple.Department;
                    int cnum = tuple.Course_Number;
                    int sec = tuple.Section_Number;
                    string name = tuple.Course_Name;
                    int cred = tuple.Credits;
                    int size = tuple.Size;
                    string p_fname = (tuple.First_Name == null) ? "Whitworth" : tuple.First_Name;
                    string p_lname = (tuple.Last_Name == null) ? "Staff" : tuple.Last_Name;
                    search.Add(new Classes(dep, cnum, sec, name, cred, p_fname, p_lname, size));
                }

                Registrar.get_shared_instance().Curr_search = search;
            }

            //Displays list
            class_list_view.ItemsSource = search;

            //Sorts list that is dislayed
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(class_list_view.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Department", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Class_number", ListSortDirection.Ascending));

            //if no results to display
            if (search.Count == 0)
                MessageBox.Show("No classes meet your search criteria");
        }

        //Detects when tabs change and updates items in selected tab
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl item = sender as TabControl;
            TabItem selected = item.SelectedItem as TabItem;

            //updates 'registered classes' tab
            if ("update_reg_class" == selected.Name)
            {
                //Generate query link to get courses we are in
                string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=5&id=" + Registrar.get_shared_instance().Curr_Stud.ID;

                //create object for each course
                List<Classes> registered = new List<Classes>();
                int num_class = 0;
                int num_cred = 0;

                using (var wc = new WebClient())
                {
                    var json = wc.DownloadString(url);

                    //If not registered for any courses do nothing
                    if(json == "[]" || json == "null" || json == null)
                    {}
                    else
                    {
                        //Decode JSON to populate registered courses

                        dynamic result = JArray.Parse(json);

                        foreach(dynamic tuple in result)
                        {
                            string dept = tuple.Department;
                            int cnum = tuple.Course_Number;
                            int sect = tuple.Section_Number;
                            string cname = tuple.Course_Name;
                            int cred = tuple.Credits;
                            string pfname = (tuple.First_Name == null) ? "Whitworth" : tuple.First_Name;
                            string plname = (tuple.Last_Name == null) ? "Staff" : tuple.Last_Name;

                            registered.Add(new Classes(dept, cnum, sect, cname, cred, pfname, plname, 0));

                            num_class++;
                            num_cred += cred;
                        }
                    }
                }

                Registrar.get_shared_instance().Curr_Stud.Registered = registered;

                //push courses into list and make that list get shown (see below) 
                registered_class_list_view.ItemsSource = registered;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(registered_class_list_view.ItemsSource);

                //Sort first by 'Class_type' then by 'Class_num'
                view.SortDescriptions.Add(new SortDescription("Department", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("Class_number", ListSortDirection.Ascending));

                credits_reg_for.Text = string.Format("{0}",num_cred);
                num_class_reg_for.Text = string.Format("{0}", num_class);
            }
            else if ("edit_acc_info_tab" == selected.Name)
            {
                //display proper information
                display_f_name.Text = Registrar.get_shared_instance().Curr_Stud.F_name;
                display_l_name.Text = Registrar.get_shared_instance().Curr_Stud.L_name;
                display_id.Text = string.Format("{0}", Registrar.get_shared_instance().Curr_Stud.ID);
                change_major.Text = Registrar.get_shared_instance().Curr_Stud.Major;
                change_phone.Text = string.Format("{0}", Registrar.get_shared_instance().Curr_Stud.Phone_Number);
                change_address.Text = Registrar.get_shared_instance().Curr_Stud.Address;
                change_username.Text = Registrar.get_shared_instance().Curr_Stud.Username;
                update_success.Text = "";
            }
        }

        //Updates 'Curr_Stud' to changed account info
        private void update_acc_btn(object sender, RoutedEventArgs e)
        {
            //push changes to database
            string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=7&id=" + Registrar.get_shared_instance().Curr_Stud.ID + "&user=" + change_username.Text;
            url += "&pass=" + change_password.Text + "&major=" + change_major.Text + "&address=" + change_address.Text + "&phone=" + change_phone.Text;

            using (var wc = new WebClient())
            {
                var output = wc.DownloadString(url);

                if(output != "Complete")
                    MessageBox.Show("*****ERROR***** \nCould not update account information");
                else
                {
                    //Keep changes in memory so we do not have to run another query
                    Registrar.get_shared_instance().Curr_Stud.Username = change_username.Text;
                    Registrar.get_shared_instance().Curr_Stud.Address = change_address.Text;
                    Registrar.get_shared_instance().Curr_Stud.Phone_Number = long.Parse(change_phone.Text);
                    Registrar.get_shared_instance().Curr_Stud.Major = change_major.Text;

                    update_success.Text = "Updated";
                }
            }

            
        }


        //Adds and drops classes from 'Curr_Stud'
        private void add_drop_btn(object sender, RoutedEventArgs e)
        {
            Button pressed = (Button)sender;
            
            
            //if button is for dropping classes
            if (pressed.Name == "drop_btn")
            {
                List<Classes> drop = Registrar.get_shared_instance().Curr_Stud.Registered;

                foreach(var course in drop)
                {
                    if(course.IsChecked == true)
                    {
                        string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=6&id=" + Registrar.get_shared_instance().Curr_Stud.ID;
                        url += "&dept=" + course.Department + "&class_num=" + course.Class_number + "&sect=" + course.Section;

                        using (var wc = new WebClient())
                        {
                            var result = wc.DownloadString(url);

                            if (result != "Complete")
                                MessageBox.Show("*****ERROR***** \nFailed to drop: " + course.Name);
                        }
                    }
                }

                //Reload page with new info
                tabControl.SelectedIndex = 1;
                tabControl.SelectedIndex = 0;
                
            }
            //if button is for adding classes
            if (pressed.Name == "add_btn")
            {   
                //Add classes to student's registered classes
                foreach(var course in Registrar.get_shared_instance().Curr_search)
                {
                    if(course.IsChecked == true) //Update to not allow them to register past 18 credits~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    {
                        course.IsChecked = false;
                        string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=4&id=" + Registrar.get_shared_instance().Curr_Stud.ID + "&dept=" + course.Department;
                        url += "&class_num=" + course.Class_number + "&sect=" + course.Section;

                        using (var wc = new WebClient())
                        {
                            var output = wc.DownloadString(url);

                            //check for successful insertion
                            if (output == "Credit limit reached")
                                MessageBox.Show("*****WARNING***** \nYou are already registered for more than 17 credits. Please see your advisor to take more.");
                            else if (output == "Already Registered")
                                MessageBox.Show("You are already registered for another section of: " + course.Name);
                            else if (output != "Complete")
                                MessageBox.Show("*****ERROR***** \nFailed to register for course: " + course.Name);
                        }
                    }
                }

                //Resets checkboxes in search tab
                Button update = seach_classes_btn;
                update.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                tabControl.SelectedIndex = 0;
            }
        }

        private void load_combobox(object sender, RoutedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            List<string> items = new List<string>();
            items.Add("");
            //bring in all departments to populate dropdown box
            using (var wc = new WebClient())
            {

                string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=";
                url += (combo.Name == "dept_combo") ? "8" : "9";

                var json = wc.DownloadString(url);

                if (json == "[]") { }//something bad happened
                else
                {
                    dynamic result = JArray.Parse(json);

                    foreach (var tuple in result)
                    {
                        string curr = (combo.Name == "dept_combo") ? tuple.Department : tuple.Major;
                        items.Add(curr);
                    }
                }
            }
            
            combo.ItemsSource = items;
            combo.Text = (combo.Name == "dept_combo") ? "" : Registrar.get_shared_instance().Curr_Stud.Major;
        }

        //Credit to: http://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
        private void ValidateNumber(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
