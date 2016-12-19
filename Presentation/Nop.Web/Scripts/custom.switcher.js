// changing class of a filter button
$(document).ready(function () {
	document.getElementById("filterSwitch").addEventListener("click",
		function () {
			var filters = document.getElementById("filters");
			var filterSwitch = document.getElementById("filterSwitch");
			if (filters.style.display == "none" || filters.style.display == "") {
				filters.style.display = "block";
				filterSwitch.style.backgroundColor = "#666666";
			}
			else {
				filters.style.display = "none";
				filterSwitch.style.backgroundColor = "#c68c53";
			}
		}
	);
});