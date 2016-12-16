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

		var that = this;
		$.ajax({
			cache: false,
			url: urladd,
			type: 'post',
			success: function (response) {
				that.success(response, function (defer) {
					var cart = $(that.topcartselector).parents('a').first();
					var itemToDrag = $(element).parents('div[data-productid]');

					return that.animateAdding(itemToDrag, cart, defer);
				});
			},
			complete: that.resetLoadWaiting,
			error: that.ajaxFailure
		});
	},

	removeFromCart: function (element, removeUrl) {
		if (this.loadWaiting != false) {
			return;
		}
		this.setLoadWaiting(true);

		var that = this;

		var defferRemoveFromCart = $.Deferred();

		$.ajax({
			cache: false,
			url: removeUrl,
			type: 'post',
			success: function (response) {
				that.success(response, function (defer) {
					$(element).parents('[product-id]').effect('drop', function () { $(this).detach(); });

					setTimeout(function () {
						defer.resolve();
						defferRemoveFromCart.resolve();
					}, 400);

					return defer.promise();
				});
			},
			complete: that.resetLoadWaiting,
			error: that.ajaxFailure
		});

		return defferRemoveFromCart.promise();
	},

	//add a product to the cart/wishlist from the product details page
	addproducttocart_details: function (urladd, formselector) {
		if (this.loadWaiting != false) {
			return;
		}
		this.setLoadWaiting(true);

		var that = this;
		$.ajax({
			cache: false,
			url: urladd,
			data: $(formselector).serialize(),
			type: 'post',
			success: function (response) {

				that.success(response, function (defer) {
					var cart = $(that.topcartselector).parents('a').first();
					var itemToDrag = $('.product-essential div.gallery > .picture');

					return that.animateAdding(itemToDrag, cart, defer);
				});
			},
			complete: this.resetLoadWaiting,
			error: this.ajaxFailure
		});
	},

	animateAdding: function (itemToDrag, endPointItem, defer) {
		defer = defer ? defer : $.Deferred();
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
				defer.resolve();
			}, 1500);

			itemClone.animate({
				'width': 0,
				'height': 0
			}, function () {
				$(this).detach()
			});
		}

		return defer.promise();
	},

	animateRemoving: function (itemToDrag, endPointItem, defer) {
		defer = defer ? defer : $.Deferred();
		if (itemToDrag.length > 0) {
			var itemClone = itemToDrag.clone()
				.offset({
					top: endPointItem.offset().top,
					left: endPointItem.offset().left
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
                	'top': itemToDrag.offset().top + 10,
                	'left': itemToDrag.offset().left + 10,
                	'width': 75,
                	'height': 75
                }, 1000, 'easeInOutExpo');

			setTimeout(function () {
				endPointItem.effect("shake", {
					distance: 25,
					times: 2
				}, 400);
				defer.resolve();
			}, 1500);

			itemClone.animate({
				'width': 0,
				'height': 0
			}, function () {
				$(this).detach()
			});
		}

		return defer.promise();
	},

	updateHtml: function (response) {
		if (response.updatetopcartsectionhtml)
			$(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
		if (response.updatetopwishlistsectionhtml)
			$(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
		if (response.updateflyoutcartsectionhtml)
			$(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
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

	success: function (response, animation) {
		var result = true;
		if (response.message) {
			if (response.success == true) {
				if (AjaxCart.usepopupnotifications == true)
					displayPopupNotification(response.message, 'success', true);
				else
					displayBarNotification(response.message, 'success', 3500);
			}
			else {
				if (AjaxCart.usepopupnotifications == true)
					displayPopupNotification(response.message, 'error', true);
				else
					displayBarNotification(response.message, 'error', 0);
			}
			result = response.success;
		}

		if (result == true && animation) {
			var defer = $.Deferred();

			animation(defer).done(function () { AjaxCart.updateHtml(response); });
		}
		else
			AjaxCart.updateHtml(response);

		if (response.redirect) {
			location.href = response.redirect;
			result = true;
		}

		return result;
	},

	resetLoadWaiting: function () {
		AjaxCart.setLoadWaiting(false);
	},

	ajaxFailure: function () {
		displayBarNotification(response.statusText, 'error', 0);
	}
};