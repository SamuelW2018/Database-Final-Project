<?php



    //$SQL = $_REQUEST["SQL"];
	
	$switchcontrol = $_REQUEST["switchcontrol"];
	
	//connect to Database
    $connopts = array('Database'=>'WhitNet', 'TrustServerCertificate'=>1);
    $conn = sqlsrv_connect("cs1", $connopts) or die("cannot connect :".sqlsrv_errors());
	
	//Run Query
	switch($switchcontrol){
		case 1: #LOG IN
			$uname = $_REQUEST["uname"];
			$pass = $_REQUEST["pass"];
			$utype = $_REQUEST["utype"];
			
			//create query
			$SQL = "Select id, First_Name, Last_Name, Major, Phone_Number, Email, [Address] From " .$utype. " Where Username LIKE '";
			$SQL .= $uname. "' AND Password LIKE '" .$pass. "'";
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			$rows = array();
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
	
			print json_encode($rows);
			break;
			
		case 2: #Create Account
			$uname = $_REQUEST["uname"];
			$pass = $_REQUEST["pass"];
			$fname = $_REQUEST["fname"];
			$lname = $_REQUEST["lname"];
			$major = $_REQUEST["major"];
			$phone_number = $_REQUEST["phone_number"];
			$email = $_REQUEST["email"];
			$address = $_REQUEST["address"];
			
			//Check if username is not taken
			$userSQL = "Select cast(count(1) as bit) as result From Student Where Username like '" . $uname . "'";
			
			$re = sqlsrv_query($conn, $userSQL)
				or die("Failed to execute SQL (".$userSQL.") : ".print_r(sqlsrv_errors()));
				
			$output = sqlsrv_fetch_array($re, SQLSRV_FETCH_ASSOC);
			
			if($output['result'] == 1)
			{
				echo "account exists";
			}
			else
			{
				//create query
				$SQL = "INSERT INTO Student(First_Name, Last_Name, [Username], [Password], [Major], Phone_Number, [Email], [Address]) ";
				$SQL .= "Values('".$fname. "', '" .$lname. "', '" .$uname. "', '" .$pass. "', '" .$major. "'";
				
				//Sets values to null if not filled in
				if(empty($phone_number))
					$SQL .= ", NULL ";
				else
					$SQL .= ", " . ($phone_number);
				
				if(empty($email))
					$SQL .= ", NULL ";
				else
					$SQL .= ", '" . $email . "'";
				
				if(empty($address))
					$SQL .= ", NULL)";
				else
					$SQL .= ", '" . $address . "')";
				
				$res = sqlsrv_query($conn, $SQL)
					or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
				
				echo "Complete";
			}
			
			break;
		
		case 3: #Search for specific classes
			$dept = $_REQUEST["dept"];
			$num = $_REQUEST["num"];
			$name = $_REQUEST["name"];
			
			//Lets us know if we need to append an AND to Where clause
			$first_addition = true;
			
			//create query
			$SQL = "Select C.Course_Number, C.Department, C.Section_Number, P.First_Name, P.Last_Name, C.Credits, C.Course_Name, C.Size ";
			$SQL .= "From Courses as C ";
			$SQL .= "LEFT OUTER JOIN rTeaching as T ON C.Course_Number = T.Class_Number AND C.Department = T.Department AND C.Section_Number = T.Section_Number ";
			$SQL .= "LEFT OUTER JOIN Professor as P ON T.Professor_ID = P.ID ";
			
			//add search properties to query if not null
			if(!empty($dept))
			{
				$SQL .= "Where C.Department = '" .$dept."' ";
				$first_addition = false;
			}
			
			if(!empty($num) And $first_addition == true)
			{
				$SQL .= "Where C.Course_Number = " .$num. " ";
				$first_addition = false;
			}
			elseif (!empty($num) And $first_addition == false)
				$SQL .= "AND C.Course_Number = " .$num. " ";
				
			if(!empty($name) And $first_addition == true)
			{
				$SQL .= "Where C.Course_Name LIKE '%" .$name. "%' ";
				$first_addition = false;
			}
			elseif(!empty($name) And $first_addition == false)
				$SQL .= "AND C.Course_Name LIKE '%" .$name. "%' ";
			
				
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			$rows = array();
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
			

	
			print json_encode($rows);	
				
			break;
			
		case 4: #Register for course
			$id = $_REQUEST["id"];
			$dept = $_REQUEST["dept"];
			$class_num = $_REQUEST["class_num"];
			$sect = $_REQUEST["sect"];
			$credits = $_REQUEST["credits"];
			
			#Check if registered for the course already
			$checkRegSQL = "Select cast(count(1) as bit) as result From rRegistration Where Student_ID = " . ($id) . " AND Department = '" . $dept . "' AND Class_Number = " . ($class_num);
			
			$re = sqlsrv_query($conn, $checkRegSQL)
				or die("Failed to execute SQL (".$CheckRegSQL.") : ".print_r(sqlsrv_errors()));
				
			$isReg = sqlsrv_fetch_array($re, SQLSRV_FETCH_ASSOC);
			
			if($isReg['result'] == 0) #if not registered for the course
			{
				$CredLimitSQL = "Select sum(C.Credits) as creditcount From Courses as C INNER JOIN rRegistration as R on R.Class_Number = C.Course_Number AND R.Section_Number = C.Section_Number AND R.Department = C.Department Where R.Student_ID = " . ($id);
				
				$rere = sqlsrv_query($conn, $CredLimitSQL)
					or die("Failed to execute SQL (".$CredLimitSQL.") : ".print_r(sqlsrv_errors()));
				
				$credRow = sqlsrv_fetch_array($rere, SQLSRV_FETCH_ASSOC);
				
				if($credRow['creditcount'] + $credits < 17) #if you haven't taken too many credits
				{
					$SQL = "INSERT INTO rRegistration (Department, Class_Number, Section_Number, Student_ID) ";
					$SQL .= "Values('" . $dept . "', " . ($class_num) . ", " . ($sect) . ", " . ($id) . ")";
					
					$res = sqlsrv_query($conn, $SQL)
						or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
					
					echo "Complete";
				}
				else
					echo "Credit limit reached";
			}
			else
				echo "Already Registered";
			
			break;
			
		case 5: #Search for registered courses
		
			$id = $_REQUEST["id"];
			
			$SQL = "Select C.Department, C.Course_Number, C.Section_Number, C.Course_Name, C.Credits, P.First_Name, P.Last_Name ";
			$SQL .= "From Courses as C INNER JOIN rRegistration as R on R.Class_Number = C.Course_Number AND R.Section_Number = C.Section_Number AND R.Department = C.Department ";
			$SQL .= "LEFT OUTER JOIN rTeaching as T ON C.Course_Number = T.Class_Number AND C.Department = T.Department AND C.Section_Number = T.Section_Number ";
			$SQL .= "LEFT OUTER JOIN Professor as P ON T.Professor_ID = P.ID Where R.Student_ID = " . $id;
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
			print json_encode($rows);
			
			break;
			
		Case 6: #Drop registered class
			$id = $_REQUEST["id"];
			$dept = $_REQUEST["dept"];
			$class_num = $_REQUEST["class_num"];
			$sect = $_REQUEST["sect"];
			
			$SQL = "Delete From rRegistration Where Student_ID = " . ($id) . " AND Class_Number = " . ($class_num) . " AND Section_Number = " . ($sect) . " AND Department = '" . $dept . "'";
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
				
			echo "Complete";
			break;
			
		Case 7: #Update Student Info
			$id = $_REQUEST["id"];
			$user = $_REQUEST["user"];
			$pass = $_REQUEST["pass"];
			$major = $_REQUEST["major"];
			$address = $_REQUEST["address"];
			$phone = $_REQUEST["phone"];

			$SQL = "UPDATE Student Set";
			
			$first = true;
			
			if(!empty($user) And $first == true)
			{
				$SQL .= " Username = '" . $user . "'";
				$first = false;
			}
			if(!empty($pass) And $first == true)
			{
				$SQL .= " Password = '" . $pass . "'";
				$first = false;
			}
			elseif(!empty($pass) And $first == false)
				$SQL .= ", Password = '" . $pass . "'";
			if(!empty($major) And $first == true)
			{
				$SQL .= " Major = '" . $major . "'";
				$first = false;
			}
			elseif(!empty($major) And $first == false)
				$SQL .= ", Major = '" . $major . "'";
			if(!empty($address) And $first == true)
			{
				$SQL .= " Address = '" . $address . "'";
				$first = false;
			}
			elseif(!empty($address) And $first == false)
				$SQL .= ", Address = '" . $address . "'";
			if(!empty($phone) And $first == true)
				$SQL .= " Phone_Number = " . $phone;
			elseif(!empty($phone) And $first == false)
				$SQL .= ", Phone_Number = " . $phone;
				
			$SQL .= " Where ID = " . $id;
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
				
			echo "Complete";
			break;
			
		Case 8: #get all departments
			$SQL = "Select Department From Courses Group by Department";
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
			print json_encode($rows);
			break;
			
			
		Case 9: #get all majors
			$SQL = "Select * From Majors";
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
			print json_encode($rows);
			
			break;
		
		Case 10: # Professor Login
			$uname = $_REQUEST["uname"];
			$pass = $_REQUEST["pass"];
			
			//create query
			$SQL = "Select ID, First_Name, Last_Name, Email, Phone_Number, Start_Date From Professor Where Username LIKE '";
			$SQL .= $uname. "' AND Password LIKE '" .$pass. "'";
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			$rows = array();
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
	
			print json_encode($rows);
			break;
		
		Case 11: # Get advisees
			$aid = $_REQUEST["aid"];
			$SQL = "Select S.First_Name, S.Last_Name, S.id From Student AS S INNER JOIN rAdvising AS A ON A.Student_ID = S.id ";
			$SQL .= "Where A.Advisor_ID = " . $aid;
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			$rows = array();
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
	
			print json_encode($rows);
			break;
		
		Case 12:
			$sid = $_REQUEST["sid"];
			$SQL = "Delete From rAdvising Where Student_ID = " . $sid;
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			
			echo "Complete";
			break;
			
		case 13: #import student information for advisor
			$id = $_REQUEST["id"];
			
			$SQL = "Select First_Name, Last_Name, Major, Phone_Number, Address, Email From Student Where id = " . ($id);
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
			$rows = array();
			while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
				$rows[] = $row;
			}
	
			print json_encode($rows);
			
			break;
			
		Case 14: # Update advisor information
		
			$id = $_REQUEST["id"];
			$user = $_REQUEST["user"];
			$pass = $_REQUEST["pass"];
			
			$SQL = "UPDATE Professor SET ";
			
			$first = true;
			
			if(!empty($user) And $first == true)
			{
				$SQL .= " Username = '" . $user . "'";
				$first = false;
			}
			if(!empty($pass) And $first == true)
			{
				$SQL .= " Password = '" . $pass . "'";
				$first = false;
			}
			elseif(!empty($pass) And $first == false)
				$SQL .= ", Password = '" . $pass . "'";
				
			$SQL .= " WHERE ID = " . ($id);
			
			$res = sqlsrv_query($conn, $SQL)
				or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));
				
			echo "Complete";
			break;
			
		
		default:
			echo "Error in switch statement input";
	}
 


    //echo "executing '" . $SQL . "'...<P>";
    /*$res = sqlsrv_query($conn, $SQL)
        or die("Failed to execute SQL (".$SQL.") : ".print_r(sqlsrv_errors()));

    
    $firstrow = 1;

	$rows = array();
    while ($row = sqlsrv_fetch_array($res, SQLSRV_FETCH_ASSOC)) {
        $rows[] = $row;
    }
	
    print json_encode($rows);*/

    sqlsrv_close($conn)
?>
