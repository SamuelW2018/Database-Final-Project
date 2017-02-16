using System;
using System.Collections;
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
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace DatabaseFinalProject
{
    /// <summary>
    /// Interaction logic for AdviseeInfo.xaml
    /// </summary>
    public partial class AdviseeInfo : Window
    {
        private Student advisee;
        public List<Classes> stud_courses = null;


        public AdviseeInfo(Student advising)
        {
            InitializeComponent();

            string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=13&id=" + advising.ID;

            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                dynamic result = JArray.Parse(json);

                string fname = result[0].First_Name;
                string lname = result[0].Last_Name;
                string major = result[0].Major;
                long phone = (result[0].Phone_Number == null) ? 0 : result[0].Phone_Number;
                string address = (result[0].Address == null) ? "None Listed" : result[0].Address;
                string email = (result[0].Email == null) ? "None Listed" : result[0].Email;

                advisee = new Student(advising.ID, "", fname, lname, major, phone, email, address);
            }

            update_course_list();


            stud_name.Text = advisee.F_name + " " + advisee.L_name;
            stud_phone.Text = string.Format("{0}", advisee.Phone_Number);
        }

        void update_course_list()
        {
            stud_courses = new List<Classes>();
            int credits = 0;
            string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=5&id=" + advisee.ID;
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                if (json != "null")
                {
                    dynamic result = JArray.Parse(json);

                    foreach (dynamic tuple in result)
                    {
                        string dept = tuple.Department;
                        int cnum = tuple.Course_Number;
                        int sect = tuple.Section_Number;
                        string cname = tuple.Course_Name;
                        int cred = tuple.Credits;
                        string pfname = (tuple.First_Name == null) ? "Whitworth" : tuple.First_Name;
                        string plname = (tuple.Last_Name == null) ? "Staff" : tuple.Last_Name;
                        credits += cred;

                        stud_courses.Add(new Classes(dept, cnum, sect, cname, cred, pfname, plname, 0));

                    }
                }
            }
            num_credits.Text = string.Format("{0}", credits);
            student_courses_view.ItemsSource = stud_courses;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(student_courses_view.ItemsSource);

            //Sort first by 'Class_type' then by 'Class_num'
            view.SortDescriptions.Add(new SortDescription("Department", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Class_number", ListSortDirection.Ascending));
        }

        private void drop_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach(Classes course in stud_courses)
            {
                if (course.IsChecked == true)
                {
                    string url = "http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=6&id=" + advisee.ID;
                    url += "&dept=" + course.Department + "&class_num=" + course.Class_number + "&sect=" + course.Section;

                    using (var wc = new WebClient())
                    {
                        var result = wc.DownloadString(url);

                        if (result != "Complete")
                            MessageBox.Show("*****ERROR***** \nFailed to drop: " + course.Name);
                    }
                }
            }
            update_course_list();
        }
    }
}
