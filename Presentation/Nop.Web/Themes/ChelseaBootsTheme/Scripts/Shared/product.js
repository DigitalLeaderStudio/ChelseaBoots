var product = (function () {

	var loadWaiting;

	function _addToWishList(evt) {
		if (loadWaiting) {
			return;
		}

		loadWaiting = true
		var $element = $(evt.currentTarget);
		var id = $element.attr('add-to-wish');
		$element.removeAttr('add-to-wish');
		$element.attr('remove-from-wish', id);
		$element.find('span').text($element.attr('remove-text'));

		$element.find('i').attr('class', 'fa fa-heart');

		$.ajax({
			cache: false,
			url: 'WishList/AddToWishList/' + id,
			data: { productId: id },
			type: 'post',
			success: onSuccess,
			complete: onComplete,
			error: onError
		});

		return false;
	}

	function _remvoeFromWishList(evt) {
		if (loadWaiting) {
			return;
		}

		loadWaiting = true
		var $element = $(evt.currentTarget);
		var id = $element.attr('remove-from-wish');
		$element.removeAttr('remove-from-wish');
		$element.attr('add-to-wish', id)
		$element.find('span').text($element.attr('add-text'));

		$element.find('i').attr('class', 'fa fa-heart-o');

		$.ajax({
			cache: false,
			url: 'WishList/RemoveFromWishList/' + id,
			type: 'post',
			success: onSuccess,
			complete: onComplete,
			error: onError
		});

		return false;
	}

	function _refreshHeaderWishListData(html) {
		var icon = $($(AjaxCart.topwishlistselector).prev());
		var itemsCount = parseInt(html.replace(/\D+/g, ''));

		icon.attr('class', 'fa fa-heart' + (itemsCount == 0 ? '-o' : ''));

		$(AjaxCart.topwishlistselector).html(html);
	}

	function onSuccess(response) {
		_refreshHeaderWishListData(response.updatetopwishlistsectionhtml);

		if (response.message) {
			//display notification
			if (response.success == true) {
				//success
				//specify timeout for success messages
				displayBarNotification(response.message, 'success', 3500);
			}
			else {
				//no timeout for errors
				displayBarNotification(response.message, 'error', 0);

			}
			return false;
		}
		if (response.redirect) {
			location.href = response.redirect;
			return true;
		}
		return false;
	}

	function onComplete(response) {
		loadWaiting = false;
	}

	function onError(response) {
		displayBarNotification(response.statusText, 'error', 0);
		loadWaiting = false;
	}

	function _bindEvents() {

		$(document).on('click', 'a[add-to-wish]', _addToWishList);
		$(document).on('click', 'a[remove-from-wish]', _remvoeFromWishList);
	}

	function _init() {
		_bindEvents();
	}

	return {
		init: _init
	}

})();
$(document).ready(function () { product.init(); });