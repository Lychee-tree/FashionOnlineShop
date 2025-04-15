document.addEventListener("DOMContentLoaded", function () {
    // Xóa sản phẩm khỏi giỏ hàng
    $('.fa-close').click(function () {
        var pvId = $(this).data('pv-id'); // Lấy pvid
        var orderId = $(this).data('order-id'); // Lấy orderid
        var row = $(this).closest('tr');
        console.log("Remove đã được gọi")
        $.ajax({
            url: '/AddToCart/RemoveItem',
            type: 'POST',
            data: {
                pvId: pvId,
                orderId: orderId
            },
            success: function (response) {
                if (response.success) {
                    // Ẩn dòng đó với hiệu ứng mượt
                    row.fadeOut(300, function () {
                        console.log ("Đang ẩn dòng")
                        row.remove();

                        // Cập nhật tổng giỏ hàng
                        $('#total-price').text(response.cartTotalFormatted);
                        formatPrices();
                    });
                }
            },
            error: function () {
                alert('Có lỗi xảy ra khi xóa sản phẩm.');
            }
        });
    });

    // Cập nhật số lượng sản phẩm trong giỏ hàng
    $(document).on('change', '.quantity input', function () {
        var row = $(this).closest('tr');
        var quantity = parseInt($(this).val());

        console.log("Update đã được gọi")

        if (quantity <= 0 || isNaN(quantity)) {
            showToast('Số lượng phải lớn hơn 0','error');
            return;
        }

        // Lấy pvId và orderId
        var closeBtn = row.find('.fa-close');
        var pvId = closeBtn.data('pv-id');
        var orderId = closeBtn.data('order-id');

        $.ajax({
            url: '/AddToCart/UpdateQuantity',
            type: 'POST',
            data: {
                pvId: pvId,
                orderId: orderId,
                quantity: quantity
            },
            success: function (response) {
                if (response.success) {
                    // Cập nhật giá đơn vị và tổng
                    row.find('.cart__price').text(response.totalUnitPriceFormatted);

                    // Cập nhật tổng tiền giỏ hàng
                    $('#total-price').text(response.cartTotalFormatted);

                    // Gọi hàm định dạng nếu cần
                    formatPrices();
                } else {
                    showToast(response.message,'error');
                    // Nếu lỗi thì gán lại số lượng cũ
                    row.find('input').val(response.currentQuantity);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra khi cập nhật số lượng.');
            }
        });
    });

    $('#proceed-to-checkout').click(function (e) {
        e.preventDefault();
        console.log("Thanh toán được gọi")
        var orderId = $(this).data('order-id');
        $.ajax({
            url: '/Checkout/CheckCartStock',
            type: 'POST',
            data: { orderId: orderId },
            success: function (response) {
                if (response.success) {
                    // Chuyển tới trang thanh toán kèm orderId
                    window.location.href = '/Checkout/CheckOut?orderId=' + response.orderId;
                } else {
                    const formattedItems = response.outOfStockItems.map(item => `• ${item}`).join('<br>');

                    const message = `
                            <div style="font-size: 14px;">
                                <strong>Một số mẫu mã trong giỏ hàng đã hết hàng:</strong><br>
                                ${formattedItems}
                                <br><strong>Vui lòng xóa sản phẩm trước khi thanh toán.</strong>
                            </div>
                        `;

                    showToast(message, 'error');
                }
            },
            error: function () {
                alert('Có lỗi khi kiểm tra tồn kho. Vui lòng thử lại.');
            }
        });
    });
});