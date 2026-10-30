/* =========================================================
   Pearl Poetry - shared shop helpers  (window.PP)
   Used by: product-details.js, cart.js, checkout.js and the
   product cards / wishlist buttons on every shop page.
   ========================================================= */
(function () {
    'use strict';

    const meta = (name) =>
        (document.querySelector('meta[name="' + name + '"]') || {}).content || '';

    const PP = {};

    // ---------- money ----------
    PP.money = (n) =>
        '৳ ' + Number(n || 0).toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });

    // ---------- JSON POST (sends the anti-forgery token) ----------
    PP.post = async function (url, payload) {
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        headers[meta('csrf-header') || 'RequestVerificationToken'] = meta('csrf-token');

        const res = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: headers,
            body: JSON.stringify(payload || {})
        });

        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
    };

    // ---------- header badges ----------
    function setBadge(selector, n) {
        document.querySelectorAll(selector).forEach((el) => {
            el.textContent = n;
            el.classList.toggle('is-empty', !n);
        });
    }
    PP.setCartCount = (n) => setBadge('[data-cart-count]', n);
    PP.setWishCount = (n) => setBadge('[data-wish-count]', n);

    // ---------- toast ----------
    PP.toast = function (message, type) {
        if (!message) return;

        let box = document.getElementById('ppToasts');
        if (!box) {
            box = document.createElement('div');
            box.id = 'ppToasts';
            box.className = 'pp-toasts';
            document.body.appendChild(box);
        }

        const el = document.createElement('div');
        el.className = 'pp-toast' + (type === 'error' ? ' is-error' : type === 'info' ? ' is-info' : '');
        el.textContent = message;
        box.appendChild(el);

        setTimeout(() => el.classList.add('is-hiding'), 2800);
        setTimeout(() => el.remove(), 3300);
    };

    window.PP = PP;

    // =====================================================
    //  Wishlist hearts  ([data-wish="productId"])
    // =====================================================
    async function toggleWish(btn) {
        if (btn.dataset.busy) return;
        btn.dataset.busy = '1';

        const id = parseInt(btn.dataset.wish, 10);

        try {
            const r = await PP.post('/Wishlist/Toggle', { productId: id });

            if (r.requiresLogin) {
                PP.toast(r.message || 'Please log in to use your wishlist.', 'info');
                setTimeout(() => { window.location.href = '/Account/Login'; }, 1200);
                return;
            }

            if (!r.success) {
                PP.toast(r.message || 'Could not update your wishlist.', 'error');
                return;
            }

            // every heart of this product on the page (card + details page ...)
            document.querySelectorAll('[data-wish="' + id + '"]').forEach((b) => {
                b.classList.toggle('is-active', r.active);
                const icon = b.querySelector('i');
                if (icon) {
                    icon.classList.toggle('fa-solid', r.active);
                    icon.classList.toggle('fa-regular', !r.active);
                }
            });

            PP.setWishCount(r.count);
            PP.toast(r.message);

            // on the wishlist page the card disappears
            if (!r.active && btn.hasAttribute('data-wish-remove')) {
                const card = btn.closest('.wl-card');
                if (card) card.remove();

                const grid = document.getElementById('wlGrid');
                if (grid && !grid.querySelector('.wl-card')) {
                    grid.classList.add('is-empty');
                    const empty = document.getElementById('wlEmpty');
                    if (empty) empty.classList.remove('is-empty');
                }
            }
        } catch (e) {
            PP.toast('Something went wrong. Please try again.', 'error');
        } finally {
            delete btn.dataset.busy;
        }
    }

    // =====================================================
    //  Quick "add to cart" on cards  ([data-add-cart="productId"])
    //  Products with colors / sizes must be chosen on the product page.
    // =====================================================
    async function quickAdd(btn) {
        if (btn.disabled) return;

        if (btn.dataset.hasVariants === 'true') {
            PP.toast('Please choose your options first.', 'info');
            const href = btn.dataset.href;
            if (href) setTimeout(() => { window.location.href = href; }, 700);
            return;
        }

        if (btn.dataset.busy) return;
        btn.dataset.busy = '1';

        try {
            const r = await PP.post('/Cart/Add', {
                productId: parseInt(btn.dataset.addCart, 10),
                quantity: 1
            });

            if (r.success) {
                PP.setCartCount(r.count);
                PP.toast(r.message || 'Added to your cart.');
            } else {
                PP.toast(r.message || 'Could not add to cart.', 'error');
            }
        } catch (e) {
            PP.toast('Something went wrong. Please try again.', 'error');
        } finally {
            delete btn.dataset.busy;
        }
    }

    document.addEventListener('click', function (e) {
        const wish = e.target.closest('[data-wish]');
        if (wish) {
            e.preventDefault();
            toggleWish(wish);
            return;
        }

        const add = e.target.closest('[data-add-cart]');
        if (add) {
            e.preventDefault();
            quickAdd(add);
        }
    });

    // =====================================================
    //  Dark mode switch - remember it between pages
    // =====================================================
    document.addEventListener('DOMContentLoaded', function () {
        const sw = document.querySelector('.switch input');
        if (!sw) return;

        try {
            sw.checked = localStorage.getItem('pp_dark') === '1';
            sw.addEventListener('change', () => {
                localStorage.setItem('pp_dark', sw.checked ? '1' : '0');
            });
        } catch (e) { /* storage blocked - ignore */ }
    });
})();