dotnet bootstrap-script.dll $1
if test "$?" = '1' ; then
	./delete_script.sh
fi

