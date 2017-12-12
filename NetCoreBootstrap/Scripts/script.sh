dotnet ./NetCoreBootstrap/Scripts/bootstrap-script.dll $1 $2
if test "$?" = '1'; then
	chmod +x ./NetCoreBootstrap/Scripts/delete_script.sh
	./NetCoreBootstrap/Scripts/delete_script.sh $1
fi
