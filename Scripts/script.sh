dotnet bootstrap-script.dll $1
if test "$?" = '1' ; then
	chmod +x delete_script.sh
	./delete_script.sh
fi

