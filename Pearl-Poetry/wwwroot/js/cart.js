/* =========================================================
   Cart page: change quantity, remove item
   ========================================================= */
(function () {
    'use strict';

    const list = document.getElementById('cpList');
    if (!list) return;

    const subtotalEl = document.getElementById('cpSubtotal');
    const totalEl = document.getElementById('cpTotal');
    const countEl = document.getElementById('cpCount');

    function applySummary(r) {
        if (typeof r.subtotal === 'number') {
            if (subtotalEl) subtotalEl.textContent = PP.money(r.subtotal);
            if (totalEl) totalEl.textContent = PP.money(r.subtotal);
        }
        if (typeof r.count === 'number') {
            if (countEl) countEl.textContent = r.count;
            PP.setCartCount(r.count);
        }
    }

    async function updateQty(row, qty) {
        const input = row.querySelector('input');
        row.classList.add('is-busy');

        try {
            const r = await PP.post('/Cart/Update', {
                itemId: parseInt(row.dataset.item, 10),
                quantity: qty
            });

            if (typeof r.quantity === 'number' && r.quantity > 0) input.value = r.quantity;

            const lineEl = row.querySelector('[data-line-total]');
            if (lineEl && typeof r.lineTotal === 'number') lineEl.textContent = PP.money(r.lineTotal);

            applySummary(r);

            if (!r.success && r.message) PP.toast(r.message, 'error');
        } catch (e) {
            PP.toast('Something went wrong. Please try again.', 'error');
        } finally {
            row.classList.remove('is-busy');
        }
    }

    async function removeRow(row) {
        row.classList.add('is-busy');

        try {
            const r = await PP.post('/Cart/Remove', { itemId: parseInt(row.dataset.item, 10) });

            if (!r.success) {
                PP.toast(r.message || 'Could not remove the item.', 'error');
                row.classList.remove('is-busy');
                return;
            }

            row.remove();
            applySummary(r);

            // last item gone => show the "cart is empty" page
            if (!list.querySelector('.cp-row')) {
                window.location.reload();
                return;
            }

            PP.toast(r.message || 'Item removed.');
        } catch (e) {
            PP.toast('Something went wrong. Please try again.', 'error');
            row.classList.remove('is-busy');
        }
    }

    // wait a moment after the last click, then save once
    const timers = new WeakMap();
    function queueUpdate(row, qty) {
        clearTimeout(timers.get(row));
        timers.set(row, setTimeout(() => updateQty(row, qty), 400));
    }

    list.addEventListener('click', function (e) {
        const row = e.target.closest('.cp-row');
        if (!row) return;

        const step = e.target.closest('[data-qty]');
        if (step) {
            const input = row.querySelector('input');
            const next = Math.max(1, (parseInt(input.value, 10) || 1) + parseInt(step.dataset.qty, 10));
            input.value = next;
            queueUpdate(row, next);
            return;
        }

        if (e.target.closest('[data-remove]')) removeRow(row);
    });

    list.addEventListener('change', function (e) {
        if (e.target.tagName !== 'INPUT') return;

        const row = e.target.closest('.cp-row');
        if (!row) return;

        const qty = Math.max(1, parseInt(e.target.value, 10) || 1);
        e.target.value = qty;
        queueUpdate(row, qty);
    });
})();