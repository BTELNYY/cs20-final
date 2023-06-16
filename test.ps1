$port = 8888;
$ip = "127.0.0.1"
$clientnum = 30
#add --ping to enable debug pinging
for($i = 0; $i -lt $clientnum; $i++ )
{
    Start-Process -FilePath ".\cs20-final-client-test\bin\Debug\net6.0\cs20-final-client-test.exe" -ArgumentList $ip,$port,$i.ToString();
}