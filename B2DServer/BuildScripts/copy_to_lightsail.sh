echo Starting Copy to AWS Lightsail
echo
echo Copy dependencies:
echo
scp -r -i  /c/Users/Steinar/Keys/b2d-test-server-key.pem ../builds/x86_64/B2DServer_D* ubuntu@13.51.157.167:/home/ubuntu/b2d-server/
echo
echo
echo Copy UnityPlayer.so:
echo
scp -i /c/Users/Steinar/Keys/b2d-test-server-key.pem ../builds/x86_64/Unity* ubuntu@13.51.157.167:/home/ubuntu/b2d-server/
echo
echo
echo Copy Executable:
echo
scp -i /c/Users/Steinar/Keys/b2d-test-server-key.pem ../builds/x86_64/B2DServer.x86_64 ubuntu@13.51.157.167:/home/ubuntu/b2d-server/
echo
echo
echo Copy Run Script:
echo
scp -i /c/Users/Steinar/Keys/b2d-test-server-key.pem run_linux_server.sh ubuntu@13.51.157.167:/home/ubuntu/b2d-server/
echo
echo Copy Complete