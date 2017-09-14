// Write your Javascript code.
$(function() 
{
	$(".user-list").on('change', function(){
		$(".user-roles").load('ViewRoles?userId=' + $(this).val());
	});
});
