document.addEventListener("DOMContentLoaded", function () {
    $('#apply-discount-btn').click(function (e) {
        e.preventDefault();


        var discountCode = $('.cart__discount input').val();

        if (discountCode.trim() === '') {
            showToast('Vui lòng nhập mã giảm giá.', 'error');
            return;
        }

        $.ajax({
            url: '/CheckOut/GetDiscount',
            type: 'POST',
            data: { discount: discountCode },
            success: function (response) {
                if (response.success) {
                    console.log("giá trị giảm:" + response.discount)
                    showToast('Áp dụng mã giảm giá thành công!', 'success');
                    // Cập nhật lại tổng tiền sau khi giảm giá
                    var originalTotal = parseInt($('#total-price').text().replace(/[^\d]/g, '')); // VD: chứa total chưa giảm
                    var discountedTotal = originalTotal * (1 - response.discount / 100);
                    $('#total-price').text(discountedTotal.toLocaleString('vi-VN') + ' VND');
                    //Cập nhật discountId
                    $('input[placeholder="Coupon code"]').attr('data-discount-id', response.discountId);

                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function () {
                showToast('Lỗi kết nối server!', 'error');
            }
        });
    });

    
    function isValidPhone(phone) {
        // Kiểm tra số điện thoại Việt Nam
        const phoneRegex = /^(0[2-9]|01[2|6|8|9])+([0-9]{8})$/;
        return phoneRegex.test(phone);
    }
    function isValidAddress(address) {
        return (address && address.trim().length > 0);
    }

    document.querySelector("input[name='address']").addEventListener("blur", function () {
        var address = this.value;
        let notice = document.getElementById('notice1');
        if (!isValidAddress(address)) {
            notice.textContent = "Không thể bỏ trông trường này";
        } else {
            notice.textContent = "";
        }
    });

    document.querySelector("input[name='phone-number']").addEventListener("blur", function () {
        var phone = this.value;
        let notice = document.getElementById('notice2');
        if (!isValidPhone(phone)) {
            notice.textContent = "Số điện thoại không hợp lệ";
        } else {
            notice.textContent = "";
        }
    });

    document.getElementById('order__btn').addEventListener('click', function (e) {
        e.preventDefault();

        var phone = document.querySelector("input[name='phone-number']");
        var address = document.querySelector("input[name='address']");
        var note = document.querySelector("input[name='note']");
        var discountId = document.querySelector('.cart__discount input').getAttribute('data-discount-id')
        var orderId = document.querySelector('.order__title').getAttribute('data-order-id');
        console.log("Kết quả" + isValidAddress(address.value) + isValidPhone(phone.value));
        // Dữ liệu cần gửi
        var phoneData = "";
        var addressData = "";
        var noteData = "";
        var discountData = null;

        if (isValidAddress(address.value) && isValidPhone(phone.value)) {

            if (phone.value !== phone.getAttribute('data-original')) {
                phoneData = phone.value;
            }
            if (address.value !== address.getAttribute('data-original')) {
                addressData = address.value;
            }
            if (note.value !== note.getAttribute('data-original')) {
                noteData = note.value;
            }
            if (discountId) {
                discountData = discountId;
            }

            $.ajax({
                url: '/Checkout/ConfirmOrder',
                method: 'POST',
                data: {
                    orderId: orderId,
                    address: addressData,
                    phone: phoneData,
                    note: noteData,
                    discountId: discountData
                },
                success: function (response) {
                    if (response.success) {
                        showToast('Đặt hàng thành công!', 'success');
                        setTimeout(() => {
                            window.location.href = '/Order/Confirmation?orderId=' + orderId;
                        }, 1500);
                    } else {
                        showToast(response.message || 'Có lỗi xảy ra.', 'error');
                    }
                },
                error: function () {
                    showToast('Lỗi kết nối đến server.', 'error');
                }
            });

        } else {
            showToast("Có thông tin không hợp lệ, vui lòng nhập lại", "error");
        }
    });

});