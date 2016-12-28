var productAttribute = (function () {

	var doPostBack = false;

	function _dropDownChange(evt) {
		var $element = $(evt.currentTarget);
		var $dropdown = $element.parents('ul');
		var $button = $element.parents('div.btn-group').find('button:first-child');
		var newText = $element.attr('origina-text') + ': ' + $element.text();
		$button.text(newText);

		$dropdown.find('li').removeClass('selected-value');
		$element.closest('li').addClass('selected-value');

		var $hiddenAttr = $dropdown.find('input[type="hidden"]');
		if ($hiddenAttr.length > 0) {
			$hiddenAttr.val($element.attr('value'));
		}
		else {
			var hinput = "<input type='hidden' id='"
				+ $element.attr('name') + "' name = '"
				+ $element.attr('name') + "' value = '"
				+ $element.attr('attr-value') + "' />";

			$dropdown.append(hinput);
		}

		if (doPostBack) {
			var $container = $element.parents('#attributes-container');
			_doPostBack(
				$element.attr('attr-action'),
				$element.attr('name'),
				$element.attr('attr-value'),
				$container);
		}
	}

	function _doPostBack(url, name, value, $container) {
		var data = {};
		data[name] = value;
		console.log(url, value, name);
		$.ajax({
			cache: false,
			url: url,
			data: data,
			type: 'post',
			success: function (response) {
				if (response.Success === true) {
					var attrs = response.AttributeInfo.split(':');
					$container.html('<p><span>' + attrs[0] + '</span><strong>' + attrs[1] + '</strong></p>');
				}
				else {
					displayBarNotification(response.Message, 'error', 0);
				}
			}
		});
	}

	function _bindEvents() {
		$(document).on('click', '.attributes ul.dropdown-menu a[attr-value]', _dropDownChange);
	}

	function _init() {
		_bindEvents();
	}

	function _setDoPostBack(value) {
		doPostBack = value;
	}

	return {
		init: _init,
		setDoPostBack: _setDoPostBack
	}

})();
$(document).ready(function () { productAttribute.init(); });