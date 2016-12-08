/*
** nopCommerce ajax cart implementation
*/


var AjaxCart = {
	loadWaiting: false,
	usepopupnotifications: false,
	topcartselector: '',
	topwishlistselector: '',
	flyoutcartselector: '',

	init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector) {
		this.loadWaiting = false;
		this.usepopupnotifications = usepopupnotifications;
		this.topcartselector = topcartselector;
		this.topwishlistselector = topwishlistselector;
		this.flyoutcartselector = flyoutcartselector;
	},

	setLoadWaiting: function (display) {
		displayAjaxLoading(display);
		this.loadWaiting = display;
	},

	//add a product to the cart/wishlist from the catalog pages
	addproducttocart_catalog: function (element, urladd) {
		if (this.loadWaiting != false) {
		    return;
		}
		this.setLoadWaiting(true);

		$.ajax({
		    cache: false,
		    url: urladd,
		    type: 'post',
		    success: this.success_process,
		    complete: this.resetLoadWaiting,
		    error: this.ajaxFailure
		});

		var cart = $(this.topcartselector).parents('a').first();
		var itemToDrag = $(element).parents('div[data-productid]');

		this.animateAdding(itemToDrag, cart);
	},

	animateAdding: function(itemToDrag, endPointItem){
		if (itemToDrag.length > 0) {
			var itemClone = itemToDrag.clone()
				.offset({
					top: itemToDrag.offset().top,
					left: itemToDrag.offset().left
				})
                .css({
                	'opacity': '0.5',
                	'position': 'absolute',
                	'height': '150px',
                	'width': '150px',
                	'z-index': '100'
                })
                .appendTo($('body'))
                .animate({
                	'top': endPointItem.offset().top + 10,
                	'left': endPointItem.offset().left + 10,
                	'width': 75,
                	'height': 75
                }, 1000, 'easeInOutExpo');

			setTimeout(function () {
				endPointItem.effect("shake", {
					distance: 25,
					times: 2
				}, 400);
			}, 1500);

			itemClone.animate({
				'width': 0,
				'height': 0
			}, function () {
				$(this).detach()
			});
		}
	},

	//add a product to the cart/wishlist from the product details page
	addproducttocart_details: function (urladd, formselector) {
		if (this.loadWaiting != false) {
			return;
		}
		this.setLoadWaiting(true);

		$.ajax({
			cache: false,
			url: urladd,
			data: $(formselector).serialize(),
			type: 'post',
			success: this.success_process,
			complete: this.resetLoadWaiting,
			error: this.ajaxFailure
		});
	},

	//add a product to compare list
	addproducttocomparelist: function (urladd) {
		if (this.loadWaiting != false) {
			return;
		}
		this.setLoadWaiting(true);

		$.ajax({
			cache: false,
			url: urladd,
			type: 'post',
			success: this.success_process,
			complete: this.resetLoadWaiting,
			error: this.ajaxFailure
		});
	},

	success_process: function (response) {
		if (response.updatetopcartsectionhtml) {
			$(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
		}
		if (response.updatetopwishlistsectionhtml) {
			$(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
		}
		if (response.updateflyoutcartsectionhtml) {
			$(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
		}
		if (response.message) {
			//display notification
			if (response.success == true) {
				//success
				if (AjaxCart.usepopupnotifications == true) {
					displayPopupNotification(response.message, 'success', true);
				}
				else {
					//specify timeout for success messages
					displayBarNotification(response.message, 'success', 3500);
				}
			}
			else {
				//error
				if (AjaxCart.usepopupnotifications == true) {
					displayPopupNotification(response.message, 'error', true);
				}
				else {
					//no timeout for errors
					displayBarNotification(response.message, 'error', 0);
				}
			}
			return false;
		}
		if (response.redirect) {
			location.href = response.redirect;
			return true;
		}
		return false;
	},

	resetLoadWaiting: function () {
		AjaxCart.setLoadWaiting(false);
	},

	ajaxFailure: function () {
		displayBarNotification(response.statusText, 'error', 0);
	}
};