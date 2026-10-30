(function () {

  
    const searchInput = document.getElementById('couponSearchLive');
    const table = document.getElementById('couponsTable');

    if (searchInput && table) {
        const rows = table.querySelectorAll('tbody tr');

        searchInput.addEventListener('input', function () {
            const term = this.value.trim().toLowerCase();

            rows.forEach(function (row) {
                const text = row.innerText.toLowerCase();
                row.style.display = text.includes(term) ? '' : 'none';
            });
        });
    }

   
    document.querySelectorAll('.btn-deactivate-coupon').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            const code = this.dataset.code || 'this coupon';
            if (!confirm(`Deactivate "${code}"? Customers won't be able to use it anymore.`)) {
                e.preventDefault();
            }
        });
    });

    // ---- Create/Edit form: only show "Max Discount Amount" for percentage coupons ----
    const discountType = document.getElementById('DiscountType');
    const maxDiscountField = document.getElementById('maxDiscountField');
    const maxDiscountInput = document.getElementById('MaxDiscountAmount');

    function toggleMaxDiscountField() {
        if (!discountType || !maxDiscountField) return;

        const isPercentage = discountType.value === 'percentage';
        maxDiscountField.dataset.hidden = isPercentage ? 'false' : 'true';

        if (!isPercentage && maxDiscountInput) {
            maxDiscountInput.value = '';
        }
    }

    if (discountType && maxDiscountField) {
        toggleMaxDiscountField();
        discountType.addEventListener('change', toggleMaxDiscountField);
    }

    
    const form = document.getElementById('couponForm');
    const startDate = document.getElementById('StartDate');
    const expiryDate = document.getElementById('ExpiryDate');

    if (form && startDate && expiryDate) {
        form.addEventListener('submit', function (e) {
            if (startDate.value && expiryDate.value && expiryDate.value < startDate.value) {
                e.preventDefault();
                alert('Expiry date cannot be before the start date.');
            }
        });
    }
    // ---- Create/Edit form: "Who can use this coupon?" ----
    const audienceRadios = document.querySelectorAll('input[name="Audience"]');

    function toggleAudience() {
        const checked = document.querySelector('input[name="Audience"]:checked');
        const value = checked ? checked.value : 'everyone';

        document.querySelectorAll('[data-audience-field]').forEach(function (field) {
            field.dataset.hidden = field.dataset.audienceField === value ? 'false' : 'true';
        });

        document.querySelectorAll('.coupon-audience-option').forEach(function (option) {
            const input = option.querySelector('input');
            option.classList.toggle('is-selected', !!(input && input.checked));
        });
    }

    if (audienceRadios.length) {
        toggleAudience();
        audienceRadios.forEach(function (r) {
            r.addEventListener('change', toggleAudience);
        });
    }

})();
