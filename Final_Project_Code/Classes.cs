using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DatabaseFinalProject
{
    public class Student
    {
        #region Student Properties
        private int id;
        private string f_name;
        private string l_name;

        private string username;

        private string major;
        private string address;
        private long phone_number;
        private string email;
        private bool isChecked;

        private List<Classes> registered;
        
        #endregion

        public Student(int iden, string u_name, string first_n, string last_n, string maj, long phone, string em, string addres)
        {
            id = iden;
            username = u_name;
            f_name = first_n;
            l_name = last_n;
            major = maj;
            phone_number = phone;
            email = em;
            address = addres;
        }

        public Student(int Iden, string first_n, string last_n)
        {
            f_name = first_n;
            l_name = last_n;
            isChecked = false;
            id = Iden;
        }

        #region Getters/Setters

        public int ID
        { get { return id; } }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string F_name
        {
            get { return f_name; }
            set { f_name = value; }
        }

        public string L_name
        {
            get { return l_name; }
            set { l_name = value; }
        }

        public string Major
        {
            get { return major; }
            set { major = value; }
        }

        public long Phone_Number
        {
            get { return phone_number; }
            set { phone_number = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public List<Classes> Registered
        {
            get { return registered; }
            set { registered = value; }
        }

        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }
        #endregion

        public int update_credits()
        {
            return 0;
        }

    }

    public class Classes
    {
        #region Class Properties
        private string department;
        private int class_number;
        private int section;
        private string name;
        private int credits;
        private string pfname;
        private string plname;
        private int size;

        private bool isChecked;
        #endregion

        public Classes(string dept, int num, int sect, string nam, int cred, string p_fname, string p_lname, int sze)
        {
            department = dept;
            class_number = num;
            section = sect;
            name = nam;
            credits = cred;
            pfname = p_fname;
            plname = p_lname;
            size = sze;
            isChecked = false;
        }

        #region Getters/Setters
        public string Department
        { get { return department; } }

        public int Class_number
        { get { return class_number; } }

        public int Section
        { get { return section; } }

        public string Name
        { get { return name; } }

        public int Credits
        { get { return credits; } }

        public string P_fname
        { get { return pfname; } }

        public string P_lname
        { get { return plname; } }

        public int Size
        { get { return size; } }

        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }
        #endregion
    }

    public class Professor
    {
        #region Professor Properties
        private int id;
        private string fname;
        private string lname;
        private string uname;
        private string email;
        private long phone_num;
        private string startDate;
        private string office_hr;
        private List<Student> advisees;

        #endregion

        //constructor
        public Professor(int iden, string first_name, string last_name, string username, string em, long phone, string SD, string Office) {
            id = iden;
            fname = first_name;
            lname = last_name;
            uname = username;
            email = em;
            phone_num = phone;
            startDate = SD;
            office_hr = Office;
        }


        #region Getter/Setter
        public int ID
        { get { return id; } }

        public string Fname
        { get { return fname; } }

        public string Lname
        { get { return lname; } }

        public string Uname
        {
            get { return uname; }
            set { uname = value; }
        }

        public string Email
        {
            get { return email; }
        }

        public long Phone_Num
        {
            get { return phone_num; }
        }

        public string StartDate
        {
            get { return startDate; }
        }

        public string Office_hr
        {
            get { return office_hr; }
        }


        public List<Student> Advisees
        {
            get { return advisees; }
            set { advisees = value; }
        }

        #endregion

    }

    public class Registrar
    {
        private static Student curr_stud = null;
        private static Professor curr_professor = null;
        private static Registrar instance = null;
        private List<Classes> curr_search = null;
        private List<Student> selected_students = null;

        public static Registrar get_shared_instance()
        {
            if (instance == null) //if no registrar has been created, make one
                instance = new Registrar();

            return instance; //always return created registrar
        }

        //Constructor. Nothing needs to be done at the creation
        private Registrar(){}

        public Student Curr_Stud
        {
            get { return curr_stud; }
            set { curr_stud = value; }
        }

        public List<Classes> Curr_search
        {
            get { return curr_search; }
            set { curr_search = value; }
        }

        public List<Student> Selected_students
        {
            get { return selected_students; }
            set { selected_students = value; }
        }

        public Professor Curr_Prof
        {
            get { return curr_professor; }
            set { curr_professor = value; }
        }

        public bool login(string user, string pass, string type)
        {
            using (var wc = new WebClient())
            {
                if (type == "Student")
                {
                    //Run login query
                    var json = wc.DownloadString("http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=1&uname=" + user + "&pass=" + pass + "&utype=" + type);

                    //Test results if account exists
                    if (json == "[]")
                        return false;
                    else
                    {
                        //Decode the JSON to create the student object
                        dynamic result = JArray.Parse(json);

                        //If we don't do this then for some reason it breaks when we try to create a new student
                        int id = result[0].id;
                        string fname = result[0].First_Name;
                        string lname = result[0].Last_Name;
                        string major = result[0].Major;

                        //Deal with possible null values
                        long phone = (result[0].Phone_Number == null) ? 0 : result[0].Phone_Number;
                        String email = (result[0].Email == null) ? "" : result[0].Email;
                        String address = (result[0].Address == null) ? "" : result[0].Address;


                        curr_stud = new Student(id, user, fname, lname, major, phone, email, address);
                        return true;
                    }
                }
                else
                {
                    var json = wc.DownloadString("http://cs1/whitnetacess/runSQLMSSQL.php?switchcontrol=10&uname=" + user + "&pass=" + pass);

                    if (json == "[]")
                        return false;
                    else
                    {
                        dynamic result = JArray.Parse(json);
                        int id = result[0].ID;
                        string fname = result[0].First_Name;
                        string lname = result[0].Last_Name;
                        string email = (result[0].Email == null) ? "None" : result[0].Email;
                        long phone = (result[0].Phone_Number == null) ? 0 : result[0].Phone_Number;
                        string start = (result[0].Start_Date == null) ? "None" : result[0].Start_Date;
                        string Off = (result[0].Office_Hour == null) ? "None" : result[0].Office_Hour;

                        curr_professor = new Professor(id, fname, lname, user, email, phone, start, Off);
                        return true;
                    }
                }

            }
        }


        public void import_student_file()
        {

        }


        public void export_student_file()
        {

        }
    }
}
