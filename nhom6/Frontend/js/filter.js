document.addEventListener("DOMContentLoaded", function () {
    var selectedCategory = 0;
    var selectedPriceRange = 0;
    var selectedColor = 0;
    var selectedSize = 0;
    var selectedSort = 0;
    console.log("filter.js đã được tải và thực thi");

    function attachClickEvent(elements, type) {
        elements.forEach(link => {
            link.addEventListener("click", function (event) {
                event.preventDefault();

                if (type === "category") {
                    selectedCategory = this.getAttribute("data-id");
                } else if (type === "price") {
                    selectedPriceRange = this.getAttribute("data-id");
                } 

                filterProducts();
            });
        });
    }

    function attachRadioEvent(elements, type) {
        elements.forEach(radio => {
            radio.addEventListener("change", function () {
                if (type === "size") {
                    selectedSize = this.value;
                } else if (type === "color") {
                    selectedColor = this.value;
                }
                filterProducts();
            });
        });
    }

    attachClickEvent(document.querySelectorAll(".category-link"), "category");
    attachClickEvent(document.querySelectorAll(".price-link"), "price");
    attachRadioEvent(document.querySelectorAll("input[name='sizeFilter']"), "size");
    attachRadioEvent(document.querySelectorAll("input[name='colorFilter']"), "color");
    // Khởi tạo nice-select tại vì có sử dụng j-query nice-select
    $('select').niceSelect();

    // Gắn sự kiện change vào select gốc
    $('.sort').on('change', function () {
        selectedSort = $(this).val();
        console.log("Giá trị đã chọn:", selectedSort);

        // Thực hiện logic sắp xếp sản phẩm
        filterProducts(selectedSort);
    });
    console.log("SizeID:", selectedSize);
    function filterProducts() {
        fetch(`/Product/FilterProducts?categoryId=${selectedCategory}&price=${selectedPriceRange}&color=${selectedColor}&size=${selectedSize}&sort=${selectedSort}`)
            .then(response => response.json())
            .then(data => {
                var productContainer = document.getElementById("product-list");
                if (!productContainer) {
                    console.error("Không tìm thấy phần tử #product-list!");
                    return;
                }
                productContainer.innerHTML = "";

                data.forEach(product => {
                    var productHTML = `
                    <div class="col-lg-4 col-md-6 col-sm-6">
                        <div class="product__item">
                            <div class="product__item__pic set-bg" data-setbg="/Content/Image/${product.ImagePath}">
                            </div>
                            <div class="product__item__text">
                                <h6 >${product.ProductName}</h6>
                                <a href="/Product/ProductDetails/${product.ProductID}" class="add-cart">Xem chi tiết</a>
                                <h5 class="unit-price">${product.UnitPrice}</h5>
                            </div>
                        </div>
                    </div>`;
                    productContainer.insertAdjacentHTML("beforeend", productHTML);
                });
                console.log("Đã in hết các phần tử")
                formatPrices();
                //addToCartBtn();
                requestAnimationFrame(() => {
                    applyBackgroundImages();
                });
            })
            .catch(error => console.error("Lỗi:", error));
    }

    // Gọi filterProducts() sau khi DOM đã sẵn sàng
    filterProducts();
});
