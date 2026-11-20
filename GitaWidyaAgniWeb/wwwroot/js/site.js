/*
 * Helper UI global (pola BEGateSecuritySystemWeb, warna GWA). Dimuat di _Layout setelah jQuery, Bootstrap, DevExtreme, SweetAlert2.
 * Dipakai script halaman di wwwroot/Scripts/<Area>/<Feature>.js, yang berbentuk modul:
 *   window.<Feature> = (function () { ...; return { add, edit, save, ... }; })();
 * URL halaman dioper dari Index.cshtml lewat window.<Feature>Url = { list, add, edit, simpanBaru, update, ... }.
 */
window.AppUI = (function () {
    const COLOR_PRIMARY = '#9a6b00';
    const COLOR_DANGER = '#c8202a';

    function token() {
        return $('input[name="__RequestVerificationToken"]').first().val() || $('meta[name="csrf-token"]').attr('content');
    }

    // Semua request jQuery non-GET membawa token antiforgery; sesi habis (401) → halaman login, ditolak (403) → pesan.
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            if (!/^(GET|HEAD|OPTIONS)$/i.test(settings.type)) xhr.setRequestHeader('RequestVerificationToken', token());
        }
    });
    $(document).ajaxError(function (event, xhr) {
        if (xhr.status === 401) {
            window.location.href = '/Login?returnUrl=' + encodeURIComponent(location.pathname + location.search);
        } else if (xhr.status === 403) {
            alertWarning('Anda tidak memiliki akses ke fitur ini.');
        }
    });

    function showLoading() { $('#loading1').show(); }
    function hideLoading() { $('#loading1').hide(); }

    function alertError(message) {
        Swal.fire({ icon: 'error', title: 'Error', text: message || 'Terjadi kesalahan saat memuat data.', confirmButtonColor: COLOR_PRIMARY });
    }

    function alertWarning(message) {
        Swal.fire({ icon: 'warning', title: 'Peringatan', text: message, confirmButtonColor: COLOR_PRIMARY });
    }

    function alertSuccess(message) {
        return Swal.fire({ icon: 'success', title: 'Berhasil', text: message, timer: 2500, showConfirmButton: false });
    }

    function handleResponse(resp, fallback) {
        if (resp && resp.Status === 1) return resp;
        alertWarning((resp && resp.Message) || fallback);
        return $.Deferred().reject(resp).promise();
    }

    function handleFail(xhr) {
        if (xhr && (xhr.status === 401 || xhr.status === 403)) return;
        alertError('Terjadi kesalahan saat mengirim data.');
    }

    /**
     * POST JSON ke action controller ([FromBody]). Resolve dengan Response bila Status=Success (1);
     * Status=Error (0) → alert warning berisi Message, lalu reject.
     */
    function postJson(url, data) {
        return $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data || {}),
            dataType: 'json',
            beforeSend: function (xhr) { xhr.setRequestHeader('RequestVerificationToken', token()); showLoading(); },
            complete: hideLoading
        }).then(function (resp) { return handleResponse(resp, 'Gagal memproses data.'); }, handleFail);
    }

    /** POST FormData (upload berkas, [FromForm] + IFormFile). Sama dengan postJson. */
    function postForm(url, formData) {
        return $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            beforeSend: function (xhr) { xhr.setRequestHeader('RequestVerificationToken', token()); showLoading(); },
            complete: hideLoading
        }).then(function (resp) { return handleResponse(resp, 'Gagal memproses data.'); }, handleFail);
    }

    /** Object → FormData (null/undefined dilewati, Date → ISO, File/Blob apa adanya). */
    function toFormData(obj) {
        const fd = new FormData();
        Object.keys(obj || {}).forEach(function (key) {
            const v = obj[key];
            if (v === null || v === undefined) return;
            if (v instanceof Date) fd.append(key, toIsoDate(v, true));
            else if (Array.isArray(v)) v.forEach(function (x) { fd.append(key, x); });
            else fd.append(key, v);
        });
        return fd;
    }

    /** GET JSON Response (mis. data untuk dirender di client). Resolve dengan resp.Data bila Status=Success. */
    function getJson(url, data) {
        return $.ajax({
            url: url,
            type: 'GET',
            data: data,
            dataType: 'json',
            beforeSend: showLoading,
            complete: hideLoading
        }).then(function (resp) {
            if (resp && resp.Status === 1) return resp.Data;
            alertWarning((resp && resp.Message) || 'Gagal memuat data.');
            return $.Deferred().reject(resp).promise();
        }, function (xhr) { if (xhr.status !== 401 && xhr.status !== 403) alertError(); });
    }

    /**
     * Dialog alasan wajib (Tolak / Batal / Revisi).
     * Return Promise<string|null>: alasan yang diisi, atau null bila dibatalkan.
     */
    function promptReason(title, confirmText, confirmCss) {
        return Swal.fire({
            title: title,
            input: 'textarea',
            inputLabel: 'Alasan',
            inputPlaceholder: 'Tuliskan alasan...',
            showCancelButton: true,
            confirmButtonText: confirmText || 'Ya',
            cancelButtonText: 'Batal',
            customClass: { confirmButton: 'btn ' + (confirmCss || 'btn-danger') + ' me-2', cancelButton: 'btn btn-light' },
            buttonsStyling: false,
            inputValidator: function (value) {
                if (!value || !value.trim()) return 'Alasan harus diisi!';
            }
        }).then(function (result) {
            return result.isConfirmed ? result.value.trim() : null;
        });
    }

    /**
     * Dialog satu isian (opts: { title, text, input: 'date'|'number'|'text'|'textarea'|'select', options ({ nilai: teks } untuk select),
     * placeholder, label, value, required, confirmText, confirmCss }).
     * Return Promise<string|null>: nilai yang diisi ('' bila kosong dan tidak wajib), atau null bila dibatalkan.
     */
    function promptInput(opts) {
        // SweetAlert2 11.3 belum mengenal input 'date' / 'time' / 'datetime-local' (isian tidak tampil, validasi wajib tetap gagal):
        // pakai input 'text' lalu ubah type-nya saat dialog terbuka.
        const nativeType = ['date', 'time', 'datetime-local', 'month'].indexOf(opts.input) >= 0 ? opts.input : null;
        const value = opts.value === undefined || opts.value === null ? '' : opts.value;
        return Swal.fire({
            title: opts.title,
            text: opts.text,
            input: nativeType ? 'text' : (opts.input || 'text'),
            didOpen: nativeType ? function () {
                const input = Swal.getInput();
                if (input) {
                    input.type = nativeType;
                    input.value = value;
                }
            } : undefined,
            inputOptions: opts.options,
            inputPlaceholder: opts.placeholder,
            inputLabel: opts.label,
            inputValue: value,
            showCancelButton: true,
            confirmButtonText: opts.confirmText || 'Simpan',
            cancelButtonText: 'Batal',
            customClass: { confirmButton: 'btn ' + (opts.confirmCss || 'btn-success') + ' me-2', cancelButton: 'btn btn-light' },
            buttonsStyling: false,
            inputValidator: function (value) {
                if (opts.required && (!value || !String(value).trim())) return (opts.label || 'Isian') + ' harus diisi!';
            }
        }).then(function (result) {
            return result.isConfirmed ? String(result.value || '').trim() : null;
        });
    }

    /**
     * Dialog beberapa isian sekaligus (ganti rantai promptInput). opts: { title, text, confirmText, confirmCss, fields: [...] }.
     * Field: { name, label, type: 'text'|'number'|'textarea'|'select'|'date'|'daterange', value, required, placeholder, options, help }.
     * - select: options = { nilai: teks } | Map | [[nilai, teks], ...] (urutan dipertahankan untuk Map / array).
     * - daterange: names = ['dari', 'sampai'], value = [dari, sampai] (yyyy-MM-dd); sampai tidak boleh sebelum dari.
     * Return Promise<object|null>: { name: nilai } (string, '' bila kosong), atau null bila dibatalkan.
     */
    function promptForm(opts) {
        let seq = 0;
        const form = document.createElement('div');
        form.className = 'gwa-prompt-form text-start';
        if (opts.text) {
            $('<p class="text-muted small mb-3">').text(opts.text).appendTo(form);
        }
        const pairs = function (options) {
            if (!options) return [];
            if (options instanceof Map) return Array.from(options.entries());
            if (Array.isArray(options)) return options;
            return Object.entries(options);
        };
        const control = function (type, value, field) {
            let el;
            if (type === 'select') {
                el = document.createElement('select');
                el.className = 'form-select';
                pairs(field.options).forEach(function (p) {
                    const o = document.createElement('option');
                    o.value = String(p[0]);
                    o.textContent = p[1];
                    el.appendChild(o);
                });
            } else if (type === 'textarea') {
                el = document.createElement('textarea');
                el.className = 'form-control';
                el.rows = 3;
            } else {
                el = document.createElement('input');
                el.className = 'form-control';
                el.type = type || 'text';
            }
            el.id = 'gwaPf' + (++seq);
            if (field.placeholder) el.placeholder = field.placeholder;
            el.value = value === undefined || value === null ? '' : String(value);
            return el;
        };
        (opts.fields || []).forEach(function (f) {
            const group = $('<div class="mb-3">').appendTo(form);
            if (f.type === 'daterange') {
                const values = f.value || [];
                const from = control('date', values[0], f);
                const to = control('date', values[1], f);
                from.dataset.name = f.names[0];
                to.dataset.name = f.names[1];
                from.setAttribute('aria-label', (f.label || 'Tanggal') + ' dari');
                to.setAttribute('aria-label', (f.label || 'Tanggal') + ' sampai');
                $('<label class="form-label">').attr('for', from.id).text(f.label || 'Rentang tanggal').appendTo(group);
                $('<div class="input-group">')
                    .append(from).append($('<span class="input-group-text">').text('s.d.')).append(to)
                    .appendTo(group);
            } else {
                const el = control(f.type, f.value, f);
                el.dataset.name = f.name;
                $('<label class="form-label">').attr('for', el.id).text(f.label || f.name).appendTo(group);
                group.append(el);
            }
            if (f.help) {
                $('<div class="form-text">').text(f.help).appendTo(group);
            }
        });

        return Swal.fire({
            title: opts.title,
            html: form,
            showCancelButton: true,
            focusConfirm: false,
            confirmButtonText: opts.confirmText || 'Simpan',
            cancelButtonText: 'Batal',
            customClass: { confirmButton: 'btn ' + (opts.confirmCss || 'btn-success') + ' me-2', cancelButton: 'btn btn-light' },
            buttonsStyling: false,
            didOpen: function () {
                const first = form.querySelector('input, select, textarea');
                if (first) first.focus();
            },
            preConfirm: function () {
                const result = {};
                form.querySelectorAll('[data-name]').forEach(function (el) { result[el.dataset.name] = String(el.value || '').trim(); });
                for (const f of opts.fields || []) {
                    if (f.type === 'daterange') {
                        const a = result[f.names[0]], b = result[f.names[1]];
                        if (f.required && (!a || !b)) return Swal.showValidationMessage((f.label || 'Rentang tanggal') + ' harus diisi!');
                        if (a && b && b < a) return Swal.showValidationMessage('Tanggal akhir harus sesudah tanggal awal.');
                    } else if (f.required && !result[f.name]) {
                        return Swal.showValidationMessage((f.label || f.name) + ' harus diisi!');
                    }
                }
                return result;
            }
        }).then(function (r) {
            return r.isConfirmed ? r.value : null;
        });
    }

    /** promptInput lalu POST JSON { ...data, [field]: nilai }; sukses → notifikasi. Batal → promise ditolak. */
    function inputPost(opts, url, data, field) {
        return promptInput(opts).then(function (value) {
            if (value === null) return $.Deferred().reject().promise();
            const body = $.extend({}, data);
            body[field] = value === '' ? null : value;
            return postJson(url, body).done(function (resp) { alertSuccess(resp.Message); });
        });
    }

    /** Dialog konfirmasi Ya/Batal. Return Promise<boolean>. */
    function confirm(title, text, confirmText, confirmCss) {
        return Swal.fire({
            icon: 'question',
            title: title,
            text: text,
            showCancelButton: true,
            confirmButtonText: confirmText || 'Ya, lanjutkan',
            cancelButtonText: 'Batal',
            customClass: { confirmButton: 'btn ' + (confirmCss || 'btn-primary') + ' me-2', cancelButton: 'btn btn-light' },
            buttonsStyling: false
        }).then(function (result) { return result.isConfirmed; });
    }

    /** Konfirmasi lalu POST JSON; sukses → notifikasi. Return promise Response (reject bila batal / gagal). */
    function confirmPost(title, text, url, data, confirmText, confirmCss) {
        return confirm(title, text, confirmText, confirmCss).then(function (ok) {
            if (!ok) return $.Deferred().reject().promise();
            return postJson(url, data).done(function (resp) { alertSuccess(resp.Message); });
        });
    }

    /** Minta alasan lalu POST JSON { ...data, [field]: alasan }; sukses → notifikasi. */
    function reasonPost(title, url, data, field, confirmText, confirmCss) {
        return promptReason(title, confirmText, confirmCss).then(function (reason) {
            if (reason === null) return $.Deferred().reject().promise();
            const body = $.extend({}, data);
            body[field || 'Keterangan'] = reason;
            return postJson(url, body).done(function (resp) { alertSuccess(resp.Message); });
        });
    }

    /** Date → "yyyy-MM-dd" (atau "yyyy-MM-ddTHH:mm:ss" bila withTime), waktu lokal. */
    function toIsoDate(d, withTime) {
        if (!d) return null;
        if (!(d instanceof Date)) d = new Date(d);
        const pad = function (n) { return String(n).padStart(2, '0'); };
        const date = d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate());
        return withTime ? date + 'T' + pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds()) : date;
    }

    /** formData dxForm -> object query string (Date diformat yyyy-MM-dd, null -> ""). */
    function toQuery(formData) {
        const result = {};
        Object.keys(formData || {}).forEach(function (key) {
            const value = formData[key];
            if (value instanceof Date) {
                result[key] = DevExpress.localization.formatDate(value, 'yyyy-MM-dd');
            } else {
                result[key] = value === null || value === undefined ? '' : value;
            }
        });
        return result;
    }

    /** Validasi dxForm; return formData siap kirim, atau null bila tidak valid. */
    function getFormQuery(formSelector) {
        const form = $(formSelector).dxForm('instance');
        if (!form.validate().isValid) {
            return null;
        }
        return toQuery(form.option('formData'));
    }

    /** GET partial view lalu render ke target (script inline partial, mis. widget DevExtreme, ikut dieksekusi). */
    function loadPartial(target, url, data) {
        return $.ajax({
            url: url,
            type: 'GET',
            data: data,
            beforeSend: showLoading,
            complete: hideLoading
        }).done(function (html) {
            if (html && html.Status === 0) {
                alertWarning(html.Message);
                return;
            }
            $(target).html(html);
        }).fail(function (xhr) {
            if (xhr.status !== 401 && xhr.status !== 403) alertError();
        });
    }

    /**
     * Muat partial view ke modal bersama di _Modal.cshtml.
     * modalId: 'staticBackdrop' (kecil), 'staticBackdropLarge' (xl), 'staticBackdropFullScreen'.
     * Opsi size 'lg' melebarkan staticBackdrop ke modal-lg selama modal terbuka.
     */
    function openModal(modalId, title, url, data, size) {
        const $modal = $('#' + modalId);
        return loadPartial($modal.find('.modal-body'), url, data).done(function (html) {
            if (html && html.Status === 0) return;
            if (size) {
                const css = 'modal-' + size;
                $modal.find('.modal-dialog').addClass(css);
                $modal.one('hidden.bs.modal', function () { $modal.find('.modal-dialog').removeClass(css); });
            }
            $modal.find('.modal-title').text(title);
            bootstrap.Modal.getOrCreateInstance($modal[0]).show();
        });
    }

    function closeModal(modalId) {
        const el = document.getElementById(modalId || 'staticBackdrop');
        const modal = el && bootstrap.Modal.getInstance(el);
        if (modal) modal.hide();
    }

    /* ===== DevExtreme DataGrid ===== */

    /** Cell template kolom "#": nomor urut lintas halaman. */
    function rowNumberTemplate(container, options) {
        const grid = options.component;
        container.text(grid.pageIndex() * grid.pageSize() + options.rowIndex + 1);
    }

    /** Cell template kolom boolean: ikon Ya (hijau) / Tidak (merah). */
    function yesNoTemplate(container, options) {
        $('<i>')
            .addClass(options.value ? 'ri-checkbox-circle-fill text-success fs-18' : 'ri-close-circle-fill text-danger fs-18')
            .attr('title', options.value ? 'Ya' : 'Tidak')
            .appendTo(container);
    }

    /**
     * Buat cell template badge status. colorMap: { 'Nilai': 'success' | 'warning' | 'danger' | 'info' | ... }
     * Nilai yang tidak ada di map memakai defaultColor (default 'info').
     */
    function badgeTemplate(colorMap, defaultColor) {
        return function (container, options) {
            if (!options.value) return;
            const color = (colorMap && colorMap[options.value]) || defaultColor || 'info';
            $('<span>').addClass('badge fs-12 badge-soft-' + color).text(options.value).appendTo(container);
        };
    }

    /** Cell template kode (font monospace). */
    function codeTemplate(container, options) {
        if (options.value === null || options.value === undefined || options.value === '') {
            $('<span class="text-muted">').text('-').appendTo(container);
            return;
        }
        $('<span class="gwa-code">').text(options.value).appendTo(container);
    }

    /** Cell template kolom jumlah: badge angka, abu-abu bila 0. */
    function countBadgeTemplate(container, options) {
        $('<span>').addClass('badge rounded-pill ' + (options.value ? 'badge-soft-primary' : 'badge-soft-secondary'))
            .text(options.value || 0)
            .appendTo(container);
    }

    /** Cell template tanggal + pembuat (InsDate / InsBy) dalam satu sel. */
    function createdTemplate(container, options) {
        const d = options.data;
        $('<span class="gwa-created">')
            .append($('<span>').text(d.InsDate ? new Date(d.InsDate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' }) : '-'))
            .append($('<small>').text(d.InsBy || '-'))
            .appendTo(container);
    }

    /** Buat tombol aksi Bootstrap kecil (btn-outline-*) untuk cell template grid. */
    function actionButton(options) {
        return $('<button type="button">')
            .addClass('btn btn-sm btn-icon me-1 ' + options.css)
            .attr('title', options.title)
            .attr('aria-label', options.title)
            .append($('<i>').addClass(options.icon).attr('aria-hidden', 'true'))
            .on('click', options.onClick);
    }

    /** onRowPrepared: baris dengan Aktif=false diberi latar merah muda. */
    function inactiveRowPrepared(e) {
        if (e.rowType === 'data' && e.data.Aktif === false) e.rowElement.addClass('bg-soft-danger');
    }

    /**
     * Semua baris DataGrid client-side sebagai array baru (grid atau selector).
     * Dibaca dari store (bukan getVisibleRows) supaya tidak terpotong paging.
     */
    function gridRows(gridOrSelector) {
        const grid = typeof gridOrSelector === 'string' ? $(gridOrSelector).dxDataGrid('instance') : gridOrSelector;
        const ds = grid.option('dataSource');
        if (Array.isArray(ds)) return ds.slice();
        const store = grid.getDataSource() && grid.getDataSource().store();
        return store && typeof store.createQuery === 'function' ? store.createQuery().toArray() : [];
    }

    /** Hapus 1 baris dari DataGrid client-side (list detail sebelum disimpan), setelah konfirmasi. */
    function removeGridRow(gridSelector, row, label) {
        return confirm('Hapus data?', label, 'Hapus', 'btn-danger').then(function (ok) {
            if (!ok) return false;
            const grid = $(gridSelector).dxDataGrid('instance');
            grid.option('dataSource', gridRows(grid).filter(function (r) { return r !== row; }));
            return true;
        });
    }

    /** Nilai widget DevExtreme di dalam elemen #id (TextBox, SelectBox, DateBox, ...). */
    function dxValue(id) {
        const el = document.getElementById(id);
        if (!el) return null;
        const names = ['dxTextBox', 'dxTextArea', 'dxSelectBox', 'dxNumberBox', 'dxDateBox', 'dxCheckBox', 'dxTagBox', 'dxSwitch', 'dxHtmlEditor'];
        for (let i = 0; i < names.length; i++) {
            const w = DevExpress.ui[names[i]] && DevExpress.ui[names[i]].getInstance(el);
            if (w) return w.option('value');
        }
        return null;
    }

    /** Berkas pertama dxFileUploader di #id (null bila kosong). */
    function dxFile(id) {
        const el = document.getElementById(id);
        const up = el && DevExpress.ui.dxFileUploader.getInstance(el);
        const files = up ? up.option('value') : [];
        return files && files.length ? files[0] : null;
    }

    /** Format rupiah tanpa desimal. */
    function rupiah(n) {
        return 'Rp ' + Number(n || 0).toLocaleString('id-ID', { maximumFractionDigits: 0 });
    }

    /* ===== Simpan form modal (POST JSON) ===== */

    /** POST JSON; sukses → tutup modal (default #staticBackdrop) + notifikasi. */
    function postAndClose(url, data, modalId) {
        return postJson(url, data).done(function (resp) {
            closeModal(modalId || 'staticBackdrop');
            alertSuccess(resp.Message);
        });
    }

    /** Validasi dxForm lalu POST formData (+ extra) dengan postAndClose. */
    function submitForm(formSelector, url, extra, modalId) {
        const form = $(formSelector).dxForm('instance');
        if (!form.validate().isValid) return $.Deferred().reject().promise();
        return postAndClose(url, $.extend({}, form.option('formData'), extra || {}), modalId);
    }

    /*
     * Widget DevExtreme yang dirender di dalam tab/collapse/modal tersembunyi punya ukuran 0.
     * Saat tab atau modal tampil, hitung ulang ukuran DataGrid / TreeList di dalamnya.
     */
    function remeasure(scope) {
        $(scope).find('.dx-datagrid, .dx-treelist').closest('.dx-widget').each(function () {
            const w = DevExpress.ui.dxDataGrid.getInstance(this) || DevExpress.ui.dxTreeList.getInstance(this);
            if (w) w.updateDimensions();
        });
    }
    $(document).on('shown.bs.tab', function (e) {
        remeasure($(e.target).attr('href') || $(e.target).data('bs-target'));
    });
    $(document).on('shown.bs.modal', function (e) {
        remeasure(e.target);
    });

    /** Halaman viewer DevExpress Reports (Views/Shared/_ReportViewer) di iframe modal layar penuh. */
    function openReport(title, src) {
        const $modal = $('#staticBackdropFullScreen');
        const $body = $modal.find('.modal-body').removeClass('p-4').addClass('p-0');
        $body.empty().append($('<iframe>', { src: src, title: title, style: 'border:0;width:100%;height:calc(100vh - 62px);display:block' }));
        $modal.find('.modal-title').text(title);
        $modal.one('hidden.bs.modal', function () { $body.empty().removeClass('p-0').addClass('p-4'); });
        bootstrap.Modal.getOrCreateInstance($modal[0]).show();
    }

    return {
        openReport: openReport,
        COLOR_PRIMARY: COLOR_PRIMARY,
        COLOR_DANGER: COLOR_DANGER,
        showLoading: showLoading,
        hideLoading: hideLoading,
        alertError: alertError,
        alertWarning: alertWarning,
        alertSuccess: alertSuccess,
        postJson: postJson,
        postForm: postForm,
        toFormData: toFormData,
        getJson: getJson,
        postAndClose: postAndClose,
        submitForm: submitForm,
        confirm: confirm,
        confirmPost: confirmPost,
        promptReason: promptReason,
        reasonPost: reasonPost,
        promptInput: promptInput,
        promptForm: promptForm,
        inputPost: inputPost,
        toIsoDate: toIsoDate,
        toQuery: toQuery,
        getFormQuery: getFormQuery,
        loadPartial: loadPartial,
        openModal: openModal,
        closeModal: closeModal,
        rowNumberTemplate: rowNumberTemplate,
        yesNoTemplate: yesNoTemplate,
        badgeTemplate: badgeTemplate,
        codeTemplate: codeTemplate,
        countBadgeTemplate: countBadgeTemplate,
        createdTemplate: createdTemplate,
        actionButton: actionButton,
        inactiveRowPrepared: inactiveRowPrepared,
        gridRows: gridRows,
        removeGridRow: removeGridRow,
        dxValue: dxValue,
        dxFile: dxFile,
        rupiah: rupiah
    };
})();
