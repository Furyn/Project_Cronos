<?php
include('../config.php');

$con = mysqli_connect($db_hostname, $db_username, $db_password, $db_database);

if(mysqli_connect_errno()){
	echo "
    <response>
        <error>1</error>
        <message>Connection failed</message>
    </response>";
    die();
}

$username = $_POST["name"];
$password = $_POST["password"];

$nameCheckQuery = "SELECT * FROM User WHERE user_login='".$username."';";

$nameCheck = mysqli_query($con,$nameCheckQuery) or die("Name check query failed");

if(mysqli_num_rows($nameCheck) != 1){
	echo "
    <response>
        <error>1</error>
        <message>No user with this name</message>
    </response>";
    die();
}

$loginInfo = mysqli_fetch_assoc($nameCheck);

if(!password_verify($password, $loginInfo["user_password"])){
    echo "
    <response>
        <error>1</error>
        <message>Incorrect password</message>
    </response>";
    die();
}

echo "
<response>
    <error>0</error>
    <user_id>".$loginInfo["user_id"]."</user_id>
</response>";

?>