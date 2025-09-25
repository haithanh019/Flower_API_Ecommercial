// ================== Helpers ==================
function showPageAlert(type, text) {
    const c = document.getElementById('pageAlert');
    if (c) c.innerHTML = `<div class="alert alert-${type}">${text}</div>`;
}

function findToken(form) {
    const token = form?.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

async function ajaxSubmit(form, onOk) {
    const err = form.querySelector('.js-error');
    if (err) { err.classList.add('d-none'); err.textContent = ''; }

    const fd = new FormData(form);
    const url = form.getAttribute('action') || (window.location.pathname + window.location.search);
    const res = await fetch(url, {
        method: form.getAttribute('method') || 'post',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        body: fd,
        credentials: 'same-origin'
    });

    let data = null;
    const ct = res.headers.get('content-type') || '';
    if (ct.includes('application/json')) data = await res.json();

    if (res.ok && data && data.ok) {
        if (onOk) onOk(data);
        else window.location.href = data.redirect || window.location.href;
    } else {
        const msg = (data && data.message) ? data.message : `Lỗi ${res.status}.`;
        if (err) { err.textContent = msg; err.classList.remove('d-none'); }
        else showPageAlert('danger', msg);
    }
}

// ================== Uploads ==================
// (giữ lại single cho nhu cầu khác)
async function uploadImage(form, file) {
    const fd = new FormData();
    fd.append('file', file);
    fd.append('__RequestVerificationToken', findToken(form));
    const res = await fetch(window.location.pathname + '?handler=Upload', {
        method: 'POST',
        body: fd,
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        credentials: 'same-origin'
    });
    const data = await res.json().catch(() => null);
    if (!res.ok || !data || !data.ok) throw new Error((data && data.message) || 'Upload lỗi');
    return data.url;
}

// Upload nhiều ảnh (duy nhất)
async function uploadImages(form, files, limit) {
    const fd = new FormData();
    const arr = Array.from(files);
    const pick = (typeof limit === 'number' && limit >= 0) ? arr.slice(0, limit) : arr;
    pick.forEach(f => fd.append('files', f));
    fd.append('__RequestVerificationToken', findToken(form));

    const res = await fetch(window.location.pathname + '?handler=UploadMany', {
        method: 'POST',
        body: fd,
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        credentials: 'same-origin'
    });

    const ct = res.headers.get('content-type') || '';
    const data = ct.includes('application/json') ? await res.json().catch(() => null) : null;
    if (!res.ok || !data || !data.ok) {
        const msg = (data && data.message) ? data.message : `Upload thất bại (${res.status})`;
        throw new Error(msg);
    }
    return data.urls;
}
// ================== Thumbnails state ==================
function setupThumb(container, hiddenInput, initialList) {
    let list = [];
    try { list = hiddenInput?.value ? JSON.parse(hiddenInput.value) : (initialList || []); }
    catch { list = (initialList || []); }

    function render() {
        if (!container) return;
        container.innerHTML = '';
        list.forEach((url, idx) => {
            const col = document.createElement('div');
            col.className = 'col position-relative';
            col.innerHTML = `
        <div class="ratio ratio-1x1 border rounded overflow-hidden">
          <img src="${url}" class="w-100 h-100" style="object-fit:cover" alt="image" />
        </div>
        <button type="button" class="btn btn-sm btn-light border position-absolute top-0 end-0 m-1 remove" data-idx="${idx}" title="Xoá">×</button>
      `;
            container.appendChild(col);
        });
        if (hiddenInput) hiddenInput.value = JSON.stringify(list);
    }

    container?.addEventListener('click', (e) => {
        const btn = e.target.closest('.remove');
        if (!btn) return;
        const idx = +btn.getAttribute('data-idx');
        if (!Number.isNaN(idx)) { list.splice(idx, 1); render(); }
    });

    render();

    return {
        addMany(urls) {
            const set = new Set(list.concat(urls || []));
            list = Array.from(set);
            render();
        },
        addOne(url) {
            if (!url) return;
            if (!list.includes(url)) list.push(url);
            render();
        },
        count() { return list.length; }   // <--- mới
    };
}
// ================== DnD Multiple init ==================
function initDndMulti(el) {
    const form = el.closest('form');
    const fileInput = el.querySelector('.dnd-file');
    const galleryId = el.dataset.gallery;
    const hiddenId = el.dataset.hidden;
    const initJson = el.dataset.init || '[]';
    const max = parseInt(el.dataset.max || '4', 10);  // <--- mặc định 4

    const gallery = document.getElementById(galleryId);
    const hidden = document.getElementById(hiddenId);

    let initial = [];
    try { initial = JSON.parse(initJson); } catch { initial = []; }

    const thumbs = setupThumb(gallery, hidden, initial);

    function takeAllowedFiles(files) {
        const allowed = Math.max(0, max - thumbs.count());
        if (allowed <= 0) { showPageAlert('warning', `Mỗi sản phẩm tối đa ${max} ảnh.`); return null; }
        if (files.length > allowed) {
            showPageAlert('warning', `Bạn chỉ có thể thêm thêm ${allowed}/${max} ảnh nữa.`);
        }
        return Array.from(files).slice(0, allowed);
    }

    el.addEventListener('click', () => fileInput?.click());
    el.addEventListener('dragover', (e) => { e.preventDefault(); el.classList.add('bg-light'); });
    el.addEventListener('dragleave', () => el.classList.remove('bg-light'));

    el.addEventListener('drop', async (e) => {
        e.preventDefault(); el.classList.remove('bg-light');
        const files = e.dataTransfer?.files;
        if (!files || files.length === 0) return;
        const subset = takeAllowedFiles(files);
        if (!subset) return;
        try {
            const urls = await uploadImages(form, subset, subset.length);
            thumbs.addMany(urls);
        } catch (err) { showPageAlert('danger', err.message || 'Upload lỗi'); }
    });

    fileInput?.addEventListener('change', async (e) => {
        const files = e.target.files;
        if (!files || files.length === 0) return;
        const subset = takeAllowedFiles(files);
        if (!subset) { e.target.value = ''; return; }
        try {
            const urls = await uploadImages(form, subset, subset.length);
            thumbs.addMany(urls);
        } catch (err) { showPageAlert('danger', err.message || 'Upload lỗi'); }
        finally { e.target.value = ''; }
    });
}
// ================== INIT ==================
document.addEventListener('DOMContentLoaded', function () {
    // Create modal submit
    const createForm = document.getElementById('createForm');
    if (createForm) {
        createForm.addEventListener('submit', function (e) {
            e.preventDefault();
            ajaxSubmit(createForm, () => {
                const m = bootstrap.Modal.getOrCreateInstance(document.getElementById('createModal'));
                m.hide(); window.location.reload();
            });
        });
    }

    // Edit forms submit
    document.querySelectorAll('form.edit-form').forEach(f => {
        f.addEventListener('submit', function (e) {
            e.preventDefault();
            ajaxSubmit(f, () => window.location.reload());
        });
    });

    // Confirm delete modal
    let pendingDeleteForm = null;
    document.querySelectorAll('form.delete-form .btn-delete').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const form = e.currentTarget.closest('form');
            pendingDeleteForm = form;
            const idEl = document.getElementById('delId');
            const nameEl = document.getElementById('delName');
            const errEl = document.getElementById('delError');
            const spnEl = document.getElementById('delSpinner');
            if (idEl) idEl.textContent = form?.dataset.id || '';
            if (nameEl) nameEl.textContent = e.currentTarget.dataset.name || '';
            errEl?.classList.add('d-none');
            spnEl?.classList.add('d-none');
            const modalEl = document.getElementById('confirmDeleteModal');
            if (modalEl) bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });
    });

    const confirmBtn = document.getElementById('btnConfirmDelete');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', () => {
            if (!pendingDeleteForm) return;
            const spnEl = document.getElementById('delSpinner');
            spnEl?.classList.remove('d-none');
            ajaxSubmit(pendingDeleteForm, () => {
                spnEl?.classList.add('d-none');
                const modalEl = document.getElementById('confirmDeleteModal');
                if (modalEl) bootstrap.Modal.getInstance(modalEl)?.hide();
                window.location.reload();
            });
        });
    }

    // Kích hoạt DnD MULTI cho tất cả khu vực
    document.querySelectorAll('.dnd-multi').forEach(initDndMulti);
});
