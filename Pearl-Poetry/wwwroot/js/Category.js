(function () {
    const modal = document.getElementById('catModal');
    const form = document.getElementById('catForm');
    const title = document.getElementById('modalTitle');
    const idInput = document.getElementById('catId');
    const nameInput = document.getElementById('catName');
    const descInput = document.getElementById('catDesc');
    const activeInput = document.getElementById('catActive');
    const imageInput = document.getElementById('catImage');
    const preview = document.getElementById('catPreview');
    const fileName = document.getElementById('fileName');
    const errorBox = document.getElementById('catError');
    const saveBtn = document.getElementById('saveBtn');

    
    const SAVE_URL = '/Admin/Category/SaveCategory';
    const DELETE_URL = '/Admin/Category/DeleteCategory/';

    const emptyPreview = '<i class="fa-regular fa-image"></i>';

    function openModal() { modal.classList.add('open'); }
    function closeModal() { modal.classList.remove('open'); }

    function resetForm() {
        form.reset();
        idInput.value = '0';
        activeInput.checked = true;
        preview.innerHTML = emptyPreview;
        fileName.textContent = 'No file chosen';
        errorBox.textContent = '';
    }

    document.getElementById('openAddModal').addEventListener('click', function () {
        resetForm();
        title.textContent = 'Add Category';
        openModal();
        nameInput.focus();
    });

    document.getElementById('closeModal').addEventListener('click', closeModal);
    document.getElementById('cancelModal').addEventListener('click', closeModal);

    modal.addEventListener('click', function (e) {
        if (e.target === modal) closeModal();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeModal();
    });

    // ---------- Edit ----------
    document.querySelectorAll('.cat-edit').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const row = this.closest('tr');
            resetForm();

            title.textContent = 'Edit Category';
            idInput.value = row.dataset.id;
            nameInput.value = row.dataset.name;
            descInput.value = row.dataset.description || '';
            activeInput.checked = row.dataset.active === 'true';
            preview.innerHTML = '<img src="' + row.dataset.image + '" alt="">';
            fileName.textContent = 'Current image (choose new to replace)';

            openModal();
        });
    });

   
    imageInput.addEventListener('change', function () {
        const file = this.files[0];
        if (!file) return;

        fileName.textContent = file.name;

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.innerHTML = '<img src="' + e.target.result + '" alt="">';
        };
        reader.readAsDataURL(file);
    });


    form.addEventListener('submit', function (e) {
        e.preventDefault();
        errorBox.textContent = '';

        if (!nameInput.value.trim()) {
            errorBox.textContent = 'Name is required.';
            return;
        }

        const data = new FormData();
        data.append('id', idInput.value);
        data.append('name', nameInput.value.trim());
        data.append('description', descInput.value.trim());
        data.append('isActive', activeInput.checked);
        if (imageInput.files[0]) {
            data.append('image', imageInput.files[0]);
        }

        saveBtn.disabled = true;
        saveBtn.textContent = 'Saving...';

        fetch(SAVE_URL, { method: 'POST', body: data })
            .then(function (res) {
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.json();
            })
            .then(function (result) {
                if (result.success) {
                    location.reload();
                } else {
                    errorBox.textContent = result.message || 'Could not save.';
                    saveBtn.disabled = false;
                    saveBtn.textContent = 'Save';
                }
            })
            .catch(function (err) {
                console.error(err);
                errorBox.textContent = 'Error: ' + err.message;
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save';
            });
    });

  
    document.querySelectorAll('.cat-delete').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const row = this.closest('tr');
            const id = row.dataset.id;
            const name = row.dataset.name;

            if (!confirm('Delete "' + name + '"? This cannot be undone.')) return;

            btn.disabled = true;

            fetch(DELETE_URL + id, { method: 'POST' })
                .then(function (res) {
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    return res.json();
                })
                .then(function (result) {
                    if (result.success) {
                        row.remove();
                    } else {
                        alert(result.message || 'Could not delete.');
                        btn.disabled = false;
                    }
                })
                .catch(function (err) {
                    console.error(err);
                    alert('Error: ' + err.message);
                    btn.disabled = false;
                });
        });
    });

  
    const search = document.getElementById('catSearch');
    const table = document.getElementById('catTable');

    if (search && table) {
        search.addEventListener('input', function () {
            const term = this.value.trim().toLowerCase();
            table.querySelectorAll('tbody tr').forEach(function (row) {
                row.style.display = row.innerText.toLowerCase().includes(term) ? '' : 'none';
            });
        });
    }
})();