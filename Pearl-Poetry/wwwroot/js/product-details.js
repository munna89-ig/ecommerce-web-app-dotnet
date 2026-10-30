/* =========================================================
   Product page: color / size variants, price, stock,
   quantity, add to cart, buy now.
   Colors / sizes that are null in the database are never shown.
   ========================================================= */
(function () {
    'use strict';

    const dataEl = document.getElementById('pdData');
    if (!dataEl) return;

    const data = JSON.parse(dataEl.textContent);
    const variants = data.variants || [];
    const hasVariants = variants.length > 0;

    const $ = (id) => document.getElementById(id);

    const priceNow = $('pdPriceNow');
    const priceOld = $('pdPriceOld');
    const priceSave = $('pdPriceSave');
    const stockEl = $('pdStock');
    const qtyInput = $('pdQty');
    const addBtn = $('pdAdd');
    const buyBtn = $('pdBuy');
    const colorBox = $('pdColors');
    const sizeBox = $('pdSizes');
    const colorLabel = $('pdColorLabel');
    const sizeLabel = $('pdSizeLabel');
    const buyForm = $('pdBuyForm');

    const unique = (arr) => [...new Set(arr)];

    const colors = unique(variants.map(v => v.color));
    const sizes = unique(variants.map(v => v.size));

    // a group is shown only if at least one variant really has a value
    const showColor = !!colorBox && colors.some(c => c !== null);
    const showSize = !!sizeBox && sizes.some(s => s !== null);

    const state = { color: null, size: null };

    // start on the first variant that is in stock
    if (hasVariants) {
        const first = variants.find(v => v.stock > 0) || variants[0];
        state.color = first.color;
        state.size = first.size;
    }

    function findVariant() {
        if (!hasVariants) return null;
        return variants.find(v =>
            (!showColor || v.color === state.color) &&
            (!showSize || v.size === state.size)) || null;
    }

    // ---------- chips ----------
    function renderGroup(box, key, values, otherKey) {
        box.innerHTML = '';

        values.forEach(value => {
            // greyed out only if NO variant with this value is in stock
            const available = variants.some(v => v[key] === value && v.stock > 0);

            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'chip' + (state[key] === value ? ' is-selected' : '') + (available ? '' : ' is-unavailable');
            btn.textContent = value === null ? 'Standard' : value;
            btn.setAttribute('aria-pressed', state[key] === value);
            btn.addEventListener('click', () => choose(key, otherKey, value));
            box.appendChild(btn);
        });
    }

    function choose(key, otherKey, value) {
        state[key] = value;

        const otherShown = key === 'color' ? showSize : showColor;

        if (otherShown) {
            // keep the other choice if that combination exists, else jump to a valid one
            const exists = variants.some(v => v[key] === value && v[otherKey] === state[otherKey] && v.stock > 0);
            if (!exists) {
                const alt = variants.find(v => v[key] === value && v.stock > 0)
                    || variants.find(v => v[key] === value);
                if (alt) state[otherKey] = alt[otherKey];
            }
        }
        render();
    }

    // ---------- price / stock / buttons ----------
    function render() {
        const v = findVariant();

        if (showColor) {
            renderGroup(colorBox, 'color', colors, 'size');
            if (colorLabel) colorLabel.textContent = state.color === null ? 'Standard' : state.color;
        }
        if (showSize) {
            renderGroup(sizeBox, 'size', sizes, 'color');
            if (sizeLabel) sizeLabel.textContent = state.size === null ? 'Standard' : state.size;
        }

        const price = v ? v.price : data.basePrice;
        const discount = v ? v.discount : data.baseDiscount;
        const stock = v ? v.stock : (hasVariants ? 0 : data.baseStock);

        const selling = (discount && discount > 0 && discount < price) ? discount : price;

        priceNow.textContent = PP.money(selling);

        if (selling < price) {
            priceOld.textContent = PP.money(price);
            priceOld.hidden = false;
            priceSave.textContent = Math.round((price - selling) / price * 100) + '% OFF';
            priceSave.hidden = false;
        } else {
            priceOld.hidden = true;
            priceSave.hidden = true;
        }

        // stock text
        stockEl.className = 'pd-stock';
        if (hasVariants && !v) {
            stockEl.textContent = 'This combination is not available';
            stockEl.classList.add('is-out');
        } else if (stock <= 0) {
            stockEl.textContent = 'Out of stock';
            stockEl.classList.add('is-out');
        } else if (stock <= 5) {
            stockEl.textContent = 'Only ' + stock + ' left';
            stockEl.classList.add('is-low');
        } else {
            stockEl.textContent = 'In stock';
            stockEl.classList.add('is-in');
        }

        // quantity limits
        qtyInput.max = Math.max(stock, 1);
        let q = parseInt(qtyInput.value, 10) || 1;
        q = Math.min(Math.max(q, 1), Math.max(stock, 1));
        qtyInput.value = q;

        const canBuy = stock > 0 && (!hasVariants || !!v);
        addBtn.disabled = !canBuy;
        buyBtn.disabled = !canBuy;
    }

    // ---------- quantity stepper ----------
    function changeQty(delta) {
        const max = parseInt(qtyInput.max, 10) || 1;
        let q = (parseInt(qtyInput.value, 10) || 1) + delta;
        q = Math.min(Math.max(q, 1), max);
        qtyInput.value = q;
    }

    $('pdQtyMinus').addEventListener('click', () => changeQty(-1));
    $('pdQtyPlus').addEventListener('click', () => changeQty(1));
    qtyInput.addEventListener('change', () => changeQty(0));

    // ---------- add to cart ----------
    addBtn.addEventListener('click', async function () {
        const v = findVariant();
        const payload = {
            productId: data.productId,
            quantity: parseInt(qtyInput.value, 10) || 1
        };
        if (v) payload.variantId = v.id;

        addBtn.disabled = true;
        try {
            const r = await PP.post('/Cart/Add', payload);
            if (r.success) {
                PP.setCartCount(r.count);
                PP.toast(r.message || 'Added to your cart.');
            } else {
                PP.toast(r.message || 'Could not add to cart.', 'error');
            }
        } catch (e) {
            PP.toast('Something went wrong. Please try again.', 'error');
        } finally {
            render();
        }
    });

    // ---------- buy now (normal form post to /Checkout/BuyNow) ----------
    buyForm.addEventListener('submit', function (e) {
        const v = findVariant();

        if (buyBtn.disabled || (hasVariants && !v)) {
            e.preventDefault();
            return;
        }

        buyForm.querySelector('[name="variantId"]').value = v ? v.id : '';
        buyForm.querySelector('[name="quantity"]').value = parseInt(qtyInput.value, 10) || 1;
    });

    // ---------- gallery ----------
    const mainImg = $('pdMainImage');
    document.querySelectorAll('[data-thumb]').forEach(t => {
        t.addEventListener('click', function () {
            mainImg.src = this.dataset.thumb;
            document.querySelectorAll('[data-thumb]').forEach(x => x.classList.remove('is-active'));
            this.classList.add('is-active');
        });
    });

    render();
})();