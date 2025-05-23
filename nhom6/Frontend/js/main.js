﻿/*  ---------------------------------------------------
    Template Name: Male Fashion
    Description: Male Fashion - ecommerce teplate
    Author: Colorib
    Author URI: https://www.colorib.com/
    Version: 1.0
    Created: Colorib
---------------------------------------------------------  */

'use strict';
console.log("Mainjs đã thực thi xong");
(function ($) {

    /*------------------
        Preloader
    --------------------*/
    $(window).on('load', function () {
        $(".loader").fadeOut();
        $("#preloder").delay(200).fadeOut("slow");

        /*------------------
            Gallery filter
        --------------------*/
        $('.filter__controls li').on('click', function () {
            $('.filter__controls li').removeClass('active');
            $(this).addClass('active');
        });
        if ($('.product__filter').length > 0) {
            var containerEl = document.querySelector('.product__filter');
            var mixer = mixitup(containerEl);
        }
    });

    /*------------------
        Background Set
    --------------------*/
    window.applyBackgroundImages = function () {
        $('.set-bg').each(function () {
            var bg = $(this).data('setbg');
            if (bg) {
                $(this).css('background-image', 'url(' + bg + ')');
            }
        });
    };
    applyBackgroundImages();
    //Search Switch
    $('.search-switch').on('click', function () {
        $('.search-model').fadeIn(400);
    });

    $('.search-close-switch').on('click', function () {
        $('.search-model').fadeOut(400, function () {
            $('#search-input').val('');
        });
    });

    /*------------------
		Navigation
	--------------------*/
    $(".mobile-menu").slicknav({
        prependTo: '#mobile-menu-wrap',
        allowParentLinks: true
    });

    /*------------------
        Accordin Active
    --------------------*/
    $('.collapse').on('shown.bs.collapse', function () {
        $(this).prev().addClass('active');
    });

    $('.collapse').on('hidden.bs.collapse', function () {
        $(this).prev().removeClass('active');
    });

    //Canvas Menu
    $(".canvas__open").on('click', function () {
        $(".offcanvas-menu-wrapper").addClass("active");
        $(".offcanvas-menu-overlay").addClass("active");
    });

    $(".offcanvas-menu-overlay").on('click', function () {
        $(".offcanvas-menu-wrapper").removeClass("active");
        $(".offcanvas-menu-overlay").removeClass("active");
    });

    /*-----------------------
        Hero Slider
    ------------------------*/
    $(".hero__slider").owlCarousel({
        loop: true,
        margin: 0,
        items: 1,
        dots: false,
        nav: true,
        navText: ["<span class='arrow_left'><span/>", "<span class='arrow_right'><span/>"],
        animateOut: 'fadeOut',
        animateIn: 'fadeIn',
        smartSpeed: 1200,
        autoHeight: false,
        autoplay: false
    });

    /*--------------------------
        Select
    ----------------------------*/
    $("select").niceSelect();

    /*-------------------
		Radio Btn
	--------------------- */
    $(".product__color__select label, .shop__sidebar__size label, .product__details__option__size label").on('click', function () {
        $(".product__color__select label, .shop__sidebar__size label, .product__details__option__size label").removeClass('active');
        $(this).addClass('active');
    });

    /*-------------------
		Scroll
	--------------------- */
    $(".nice-scroll").niceScroll({
        cursorcolor: "#0d0d0d",
        cursorwidth: "5px",
        background: "#e5e5e5",
        cursorborder: "",
        autohidemode: true,
        horizrailenabled: false
    });

    // Color button
    $('input[name="color"]').on('change', function () {
        $('input[name="color"]').parent().removeClass('active');
        $(this).parent().addClass('active');
        updateStockQuantity();
    });
    /*------------------
        CountDown
    --------------------*/
    // For demo preview start
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
    var yyyy = today.getFullYear();

    if(mm == 12) {
        mm = '01';
        yyyy = yyyy + 1;
    } else {
        mm = parseInt(mm) + 1;
        mm = String(mm).padStart(2, '0');
    }
    var timerdate = mm + '/' + dd + '/' + yyyy;
    // For demo preview end


    // Uncomment below and use your date //

    /* var timerdate = "2020/12/30" */

    $("#countdown").countdown(timerdate, function (event) {
        $(this).html(event.strftime("<div class='cd-item'><span>%D</span> <p>Days</p> </div>" + "<div class='cd-item'><span>%H</span> <p>Hours</p> </div>" + "<div class='cd-item'><span>%M</span> <p>Minutes</p> </div>" + "<div class='cd-item'><span>%S</span> <p>Seconds</p> </div>"));
    });

    /*------------------
		Magnific
	--------------------*/
    $('.video-popup').magnificPopup({
        type: 'iframe'
    });

    /*-------------------
		Quantity change
	--------------------- */
    var proQty = $('.pro-qty');
    proQty.prepend('<span class="fa fa-angle-up dec qtybtn"></span>');
    proQty.append('<span class="fa fa-angle-down inc qtybtn"></span>');
    proQty.on('click', '.qtybtn', function () {
        var $button = $(this);
        var oldValue = $button.parent().find('input').val();
        if ($button.hasClass('inc')) {
            var newVal = parseFloat(oldValue) + 1;
        } else {
            // Don't allow decrementing below zero
            if (oldValue > 0) {
                var newVal = parseFloat(oldValue) - 1;
            } else {
                newVal = 0;
            }
        }
        $button.parent().find('input').val(newVal);
    });

    var proQty = $('.pro-qty-2');
    proQty.prepend('<span class="fa fa-angle-left dec qtybtn"></span>');
    proQty.append('<span class="fa fa-angle-right inc qtybtn"></span>');
    proQty.on('click', '.qtybtn', function () {
        var $button = $(this);
        var oldValue = $button.parent().find('input').val();
        if ($button.hasClass('inc')) {
            var newVal = parseFloat(oldValue) + 1;
        } else {
            // Don't allow decrementing below zero
            if (oldValue > 0) {
                var newVal = parseFloat(oldValue) - 1;
            } else {
                newVal = 0;
            }
        }
        $button.parent().find('input').val(newVal);
        // active change event
        $button.parent().find('input').trigger('change');
    });

    /*------------------
        Achieve Counter
    --------------------*/
    $('.cn_num').each(function () {
        $(this).prop('Counter', 0).animate({
            Counter: $(this).text()
        }, {
            duration: 4000,
            easing: 'swing',
            step: function (now) {
                $(this).text(Math.ceil(now));
            }
        });
    });
})(jQuery);

//Red underline in nav
var currentPath = window.location.pathname.toLowerCase();
var navLinks = document.querySelectorAll(".header__menu ul li");

navLinks.forEach((link) => {
    var linkPath = link.querySelector("a").getAttribute("href").toLowerCase();

    if (currentPath === linkPath) {
        link.classList.add("active");
    }
});

//Format price
window.formatPrices = function () {
    let unitPrice = document.querySelectorAll('.unit-price');
    console.log("Hàm formatPrices đã được thực thi");
    unitPrice.forEach(p => {
        let price = parseInt(p.innerText.replace(/\D/g, '')); // Change data type to int
        if (!isNaN(price)) {
            p.innerText = price.toLocaleString('vi-VN') + " VND";
        }
    });
}
window.showToast = function (message, type = 'success') {
    const toast = document.getElementById('cart-toast');
    const toastHeader = document.getElementById('toast-header');
    const toastBody = document.getElementById('toast-body');
    const toastTitle = document.getElementById('toast-title');

    // Reset
    toastHeader.className = 'toast-header text-white';

    switch (type) {
        case 'success':
            toastHeader.classList.add('bg-success');
            toastTitle.textContent = 'Thành công';
            break;
        case 'error':
            toastHeader.classList.add('bg-danger');
            toastTitle.textContent = 'Lỗi';
            break;
    }

    toastBody.innerHTML = message;

    // Khởi tạo và show toast bằng jQuery của Bootstrap 4
    $('#cart-toast').toast('dispose'); // Đảm bảo xóa instance cũ nếu có
    $('#cart-toast').toast({ delay: 10000 });
    $('#cart-toast').toast('show');

    console.log('Toast đã được hiển thị!');
}
$(document).ready(function () {
    var cart = document.getElementById("cart");

    fetch('/Login/CheckLogin', {
        method: 'POST'
    })
        .then(res => res.json())
        .then(data => {
            if (data.isLoggedIn) {
                // Hiện số lượng giỏ hàng
                $.get('/AddToCart/GetCartCount', function (res) {
                    if (res.success) {
                        document.getElementById("cart-count").textContent = res.cartCount;
                    }
                });
            } else {
                cart.replaceWith(cart.cloneNode(true));
                cart = document.getElementById("cart");
                cart.addEventListener('click', function (e) { 
                    e.preventDefault();
                    $('#loginRequiredModal').modal('show'); //do sử dụng Bootstrap 4.41 đây là hàm gọi modal 
                });
            }
        });
    document.getElementById("goToLoginBtn").addEventListener("click", function () {
        window.location.href = "/Login/Login"; //chuyển hướng đến trang login do Tuyền làm
    });
});

