(function () {
    function readCart() {
        try { return JSON.parse(localStorage.getItem("cart") || "[]"); }
        catch { return []; }
    }
    function writeCart(arr) { localStorage.setItem("cart", JSON.stringify(arr)); }
    function updateCartBadge() {
        var c = readCart().length;
        var badge = document.getElementById("cart-count");
        if (badge) {
            badge.textContent = c;
            badge.classList.toggle("d-none", c === 0);
        }
    }
    document.addEventListener("click", function (e) {
        var btn = e.target.closest(".add-to-cart");
        if (!btn) return;
        var id = +btn.dataset.id;
        var cart = readCart();
        cart.push(id);
        writeCart(cart);
        updateCartBadge();
    });
    updateCartBadge();
})();
