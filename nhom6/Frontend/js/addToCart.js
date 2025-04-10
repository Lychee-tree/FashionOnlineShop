document.addEventListener("DOMContentLoaded", function () {

    function addToCart(productId) {
        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: {
                productId: productId,
                quantity: 1
            },
            success: function (response) {
                if (response.requiresLogin) {
                    window.location.href = '/Account/Login?returnUrl=' + window.location.pathname;
                } else if (response.success) {
                    alert("Đã thêm vào giỏ hàng");
                    // Hoặc cập nhật số lượng trên icon giỏ hàng
                    loadCartCount();
                } else {
                    alert("Thêm thất bại");
                }
            },
            error: function () {
                alert("Lỗi kết nối server");
            }
        });
    }

    window.addToCartBtn = function () {

        document.querySelectorAll(".add-cart").forEach(cartBtn => {
            cartBtn.addEventListener("click", function (e) {
                e.preventDefault();

                fetch('/Login/CheckLogin', {
                    method: 'POST'
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isLoggedIn) {
                        }
                        else {
                            $('#loginRequiredModal').modal('show'); //do sử dụng Bootstrap 4.41 đây là hàm gọi modal 
                        }
                    });
                console.log("addToCart đã được gọi");
            });
        });
    }
    // Khi người dùng bấm đăng nhập
    document.getElementById("goToLoginBtn").addEventListener("click", function () {
        window.location.href = "/Account/Login"; //chuyển hướng đến trang login do Tuyền làm
    });
});