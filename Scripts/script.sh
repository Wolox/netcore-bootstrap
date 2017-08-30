dotnet bootstrap-script.dll $1
if test "$?" = '1' ; then
	chmod +x ./netcore-bootstrap/Scripts/delete_script.sh
	./netcore-bootstrap/Scripts/delete_script.sh
fi

