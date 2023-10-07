<?php
include('../config.php');

$con = mysqli_connect($db_hostname, $db_username, $db_password, $db_database);

if(mysqli_connect_errno()){
	echo "Connection failed",
	exit();
}

$username = $_POST["name"];
$password = $_POST["password"];

$nameCheckQuery = "SELECT user_login FROM User WHERE user_login='".$username."';";

$nameCheck = mysqli_query($con,$nameCheckQuery) or die("Name check query failed");

if(mysqli_num_rows($nameCheck) > 0){
	echo "Name already exists";
	exit();
}

$passwordCrypted = password_hash($password, PASSWORD_DEFAULT);

$insertUserQuery = "INSERT INTO User(user_login, user_password) VALUES ('".$username."','".$passwordCrypted."');";

mysqli_query($con, $insertUserQuery) or die("Insert player query failed");

$getIdQuery = "SELECT user_id  FROM User WHERE user_login='".$username."';";

$getId = mysqli_query($con,$getIdQuery) or die("Get user_id query failed");

if(mysqli_num_rows($getId) != 1){
	echo "No user with this name";
	exit();
}

$getIdInfo = mysqli_fetch_assoc($getId);

echo"0\t". $getIdInfo["user_id"];

?>