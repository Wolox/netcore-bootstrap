dotnet ./Scripts/bootstrap-script.dll $1
if test "$?" = '1' ; then
	chmod +x ./Scripts/delete_script.sh
	./Scripts/delete_script.sh
fi

