document.addEventListener("DOMContentLoaded", function () {

    function addToCart(productID, sizeID, colorID, quantity) {
        $.ajax({
            url: '/AddToCart/AddToCart',
            type: 'POST',
            data: {
                product: productID,
                size: sizeID,
                color: colorID,
                quantity: quantity
            },
            success: function (res) {
                if (res.success) {
                    document.getElementById("cart-count").textContent = res.cartCount;

                    // Hiển thị toast
                    showToast("Đã thêm vào giỏ hàng!", "success");

                    // Ẩn thông báo lỗi nếu có
                    document.getElementById("error-message").style.display = "none";
                } else {
                    document.getElementById("error-message").innerText = res.message;
                    document.getElementById("error-message").style.display = "block";
                }
            },
            error: function () {
                alert("Đã có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        });
    }

    window.addToCartBtn = function () {

        fetch('/Login/CheckLogin', {
            method: 'POST'
        })
            .then(res => res.json())
            .then(data => {
                if (data.isLoggedIn) {
                    const selectedProductID = document.querySelector('#add-to-cart-btn').getAttribute("data-product-id")
                    const selectedSizeID = document.querySelector('input[name="size"]:checked')?.value;
                    const selectedColorID = document.querySelector('input[name="color"]:checked')?.value;
                    const selectedQuantity = parseInt(document.getElementById("quantity-input").value);

                    addToCart(selectedProductID, selectedSizeID, selectedColorID, selectedQuantity);
                }
                else {
                    $('#loginRequiredModal').modal('show'); //do sử dụng Bootstrap 4.41 đây là hàm gọi modal 
                }
            });
        console.log("addToCart đã được gọi");
    }
    // Khi người dùng bấm đăng nhập
    document.getElementById("goToLoginBtn").addEventListener("click", function () {
        window.location.href = "/Login/Login"; //chuyển hướng đến trang login do Tuyền làm
    });
});