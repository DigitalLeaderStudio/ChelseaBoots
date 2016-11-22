;
var catalog = (function () {

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

			History.pushState(null, '', $(this).attr('href'));
		});

		//override href behaviours in left part
		$(document).on('change', '#filter-spec input[type=checkbox]', function (evt) {
			History.pushState(null, '', $(this).attr('href'));
		});
	}

	function _init() {

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