(function () {

    // ---------- live search in the table ----------
    const searchInput = document.getElementById('productSearch');
    const table = document.getElementById('productsTable');

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

    // ---------- anti-forgery token (from @Html.AntiForgeryToken() in the view) ----------
    function getToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    // ---------- delete ----------
    document.querySelectorAll('.btn-delete').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const id = this.dataset.id;
            const row = this.closest('tr');
            const title = row.querySelector('.row-title')?.innerText?.trim() || 'this product';

            if (!confirm(`Delete "${title}"? This cannot be undone.`)) {
                return;
            }

            const token = getToken();
            if (!token) {
                alert('Security token missing. Please refresh the page and try again.');
                return;
            }

            btn.disabled = true;
            row.style.opacity = '0.5';

            fetch(`/Admin/Product/DeleteProduct/${id}`, {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'RequestVerificationToken': token,   // checked by [ValidateAntiForgeryToken]
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(function (res) {
                    if (res.status === 401 || res.status === 403) {
                        throw new Error('You are not allowed to delete products. Please log in again.');
                    }
                    if (!res.ok) {
                        throw new Error('Delete failed (HTTP ' + res.status + ').');
                    }
                    return res.json();
                })
                .then(function (data) {
                    if (data.success) {
                        // fade out, then remove the row
                        row.style.transition = 'opacity .3s';
                        row.style.opacity = '0';
                        setTimeout(function () { row.remove(); }, 300);
                    } else {
                        alert(data.message || 'Could not delete this product.');
                        btn.disabled = false;
                        row.style.opacity = '';
                    }
                })
                .catch(function (err) {
                    alert(err.message || 'Something went wrong. Please try again.');
                    btn.disabled = false;
                    row.style.opacity = '';
                });
        });
    });
})();