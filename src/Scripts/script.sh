dotnet ./src/Scripts/bootstrap-script.dll $1 $2
if test "$?" = '1'; then
	chmod +x ./src/Scripts/delete_script.sh
	./src/Scripts/delete_script.sh $1
fi
