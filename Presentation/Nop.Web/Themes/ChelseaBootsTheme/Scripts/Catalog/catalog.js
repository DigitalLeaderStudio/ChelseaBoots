;
var catalog = (function () {
	var shopName = '';

	function _load(url) {
		$.ajax({
			type: "GET",
			url: url,
			success: function (response) {
				$('#catalog-container').html(response);
			},
			error: function (error) {
				alert('error on the page')
			}
		});
	}

	function _bindEvents() {

		//override href behaviours in main part
		$(document).on('click', '#catalog-container a:not(#products-container a)', function (evt) {
			evt.preventDefault();
			History.pushState(null, shopName + $(this).text(), $(this).attr('href'));
		});

		//override href behaviours in left part
		$(document).on('change', '#filter-spec input[type=checkbox]', function (evt) {
			History.pushState(null, document.title, $(this).attr('href'));
		});
	}

	function _init() {
		shopName = document.title.substr(0, document.title.indexOf('.')) + '. ';

		_bindEvents();

		History.Adapter.bind(window, 'statechange', function () {
			var state = History.getState();
			_load(state.url);
		});
	}

	return {
		init: _init
	}
})();

$(function () { catalog.init(); });