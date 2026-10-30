/* =========================================================
   Checkout page: bKash box, delivery charge by district,
   double-click protection
   ========================================================= */
(function () {
    'use strict';

    const form = document.getElementById('ckForm');
    if (!form) return;

    const subtotal = parseFloat(form.dataset.subtotal) || 0;
    const inside = parseFloat(form.dataset.inside) || 0;
    const outside = parseFloat(form.dataset.outside) || 0;

    const district = document.getElementById('District');
    const deliveryEl = document.getElementById('ckDelivery');
    const totalEl = document.getElementById('ckTotal');
    const bkashAmt = document.getElementById('ckBkashAmt');
    const bkashBox = document.getElementById('ckBkash');
    const submit = document.getElementById('ckSubmit');

    // ---------- delivery charge follows the district ----------
    function recalc() {
        const isDhaka = ((district && district.value) || '').trim().toLowerCase() === 'dhaka';
        const delivery = isDhaka ? inside : outside;
        const total = subtotal + delivery;

        if (deliveryEl) deliveryEl.textContent = delivery > 0 ? PP.money(delivery) : 'Free';
        if (totalEl) totalEl.textContent = PP.money(total);
        if (bkashAmt) bkashAmt.textContent = PP.money(total);
    }

    if (district) district.addEventListener('change', recalc);
    recalc();

    // ---------- show the bKash fields only for bKash ----------
    function togglePay() {
        const picked = form.querySelector('input[name="PaymentMethod"]:checked');
        if (bkashBox) bkashBox.hidden = !(picked && picked.value === 'bkash');
    }

    form.querySelectorAll('input[name="PaymentMethod"]').forEach((r) => r.addEventListener('change', togglePay));
    togglePay();

    // ---------- do not submit twice ----------
    form.addEventListener('submit', function () {
        // let jquery-validate stop an invalid form first
        if (window.jQuery && jQuery.fn.valid && !jQuery(form).valid()) return;

        if (submit) {
            submit.disabled = true;
            submit.textContent = 'Placing order...';
        }
    });

    // coming back with the browser's back button
    window.addEventListener('pageshow', function (e) {
        if (e.persisted && submit) {
            submit.disabled = false;
            submit.textContent = 'Place Order';
        }
    });
})();