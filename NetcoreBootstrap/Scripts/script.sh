dotnet ./NetcoreBootstrap/Scripts/bootstrap-script.dll $1 $2
if test "$?" = '1'; then
	chmod +x ./NetcoreBootstrap/Scripts/delete_script.sh
	./NetcoreBootstrap/Scripts/delete_script.sh $1
fi
