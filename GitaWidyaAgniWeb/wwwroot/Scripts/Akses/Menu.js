// Akses › Menu. URL dari window.MenuUrl (Views/Akses/Menu/Index.cshtml); helper global di js/site.js (AppUI).
window.Menu = (function () {
    const MODAL = 'staticBackdrop';
    const MODAL_LARGE = 'staticBackdropLarge';
    let detailItems = [];

    $(function () {
        refreshController();
        refreshHeader();
        refreshSub();
    });

    function refreshController() { AppUI.loadPartial('#targetController', MenuUrl.listController); }
    function refreshHeader() { AppUI.loadPartial('#targetHeader', MenuUrl.listHeader); }
    function refreshSub() { AppUI.loadPartial('#targetSub', MenuUrl.listSub); }

    /* ===== Controller ===== */

    function addController() {
        AppUI.openModal(MODAL, 'Tambah Controller', MenuUrl.addController, null, 'lg');
    }
    function editController(encKey) {
        AppUI.openModal(MODAL, 'Ubah Controller', MenuUrl.editController, { encKey: encKey }, 'lg');
    }
    function saveController() {
        AppUI.submitForm('#frmController', MenuUrl.simpanController, null, MODAL).done(refreshController);
    }
    function updateController() {
        AppUI.submitForm('#frmController', MenuUrl.updateController, null, MODAL).done(refreshController);
    }
    function deleteController(row) {
        AppUI.confirmPost('Nonaktifkan controller?', 'Controller ' + row.Name + ' dan hak aksesnya tidak berlaku lagi.', MenuUrl.hapusController, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refreshController);
    }

    function actions(row) {
        AppUI.openModal(MODAL_LARGE, 'Daftar Aksi — ' + row.Name, MenuUrl.aksi, { encKey: row.EncDecrKey });
    }
    function saveActions() {
        const grid = $('#tblAksi').dxDataGrid('instance');
        grid.saveEditData().then(function () {
            const rows = AppUI.gridRows(grid).map(function (r) {
                return { Action: $.trim(r.Action || ''), NeedAuth: r.NeedAuth, Description: r.Description || null };
            }).filter(function (x) { return x.Action; });
            if (!rows.length) {
                AppUI.alertWarning('Minimal satu aksi (aksi halaman utama wajib ada).');
                return;
            }
            AppUI.postAndClose(MenuUrl.simpanAksi, { Key: $('#menuAksi').data('key'), Rows: rows }, MODAL_LARGE).done(refreshController);
        });
    }

    /* ===== Header menu ===== */

    function addHeader() {
        AppUI.openModal(MODAL, 'Tambah Header Menu', MenuUrl.addHeader, null, 'lg');
    }
    function editHeader(encKey) {
        AppUI.openModal(MODAL, 'Ubah Header Menu', MenuUrl.editHeader, { encKey: encKey }, 'lg');
    }
    function saveHeader() {
        AppUI.submitForm('#frmHeader', MenuUrl.simpanHeader, null, MODAL).done(refreshHeader);
    }
    function updateHeader() {
        AppUI.submitForm('#frmHeader', MenuUrl.updateHeader, null, MODAL).done(refreshHeader);
    }
    function deleteHeader(row) {
        AppUI.confirmPost('Nonaktifkan header?', 'Header ' + row.Name + ' tidak tampil lagi di sidebar.', MenuUrl.hapusHeader, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(refreshHeader);
    }

    // Sub menu dalam grup: grid berseleksi + seret untuk urutan.
    function detail(row) {
        AppUI.openModal(MODAL_LARGE, 'Sub Menu — ' + row.Name, MenuUrl.detail, { encKey: row.EncDecrKey });
    }
    function initDetail(items) {
        detailItems = items || [];
        $('#tblDetail').dxDataGrid({
            dataSource: detailItems,
            keyExpr: 'Id',
            elementAttr: { class: 'gwa-list-grid' },
            showBorders: false,
            showRowLines: true,
            showColumnLines: true,
            height: 420,
            scrolling: { mode: 'standard' },
            paging: { enabled: false },
            selection: { mode: 'multiple', showCheckBoxesMode: 'always', allowSelectAll: false },
            selectedRowKeys: detailItems.filter(function (x) { return x.Dipilih; }).map(function (x) { return x.Id; }),
            rowDragging: {
                allowReordering: true,
                showDragIcons: true,
                onReorder: function (e) {
                    const visible = e.component.getVisibleRows();
                    const toIndex = detailItems.findIndex(function (x) { return x.Id === visible[e.toIndex].data.Id; });
                    const fromIndex = detailItems.findIndex(function (x) { return x.Id === e.itemData.Id; });
                    detailItems.splice(fromIndex, 1);
                    detailItems.splice(toIndex, 0, e.itemData);
                    e.component.refresh();
                }
            },
            columns: [
                { dataField: 'Nama', caption: 'Sub Menu', cellTemplate: function (c, o) { iconText(c, o.data.Ikon, o.value); } },
                { dataField: 'Controller', caption: 'Controller', cellTemplate: AppUI.codeTemplate }
            ]
        });
    }
    function saveDetail() {
        const chosen = $('#tblDetail').dxDataGrid('instance').getSelectedRowKeys();
        const ids = detailItems.map(function (x) { return x.Id; }).filter(function (id) { return chosen.indexOf(id) >= 0; });
        AppUI.postAndClose(MenuUrl.simpanDetail, { Key: $('#menuDetail').data('key'), SubMenuIds: ids }, MODAL_LARGE).done(function () {
            refreshHeader();
            refreshSub();
        });
    }

    /* ===== Sub menu ===== */

    function addSub() {
        AppUI.openModal(MODAL, 'Tambah Sub Menu', MenuUrl.addSub, null, 'lg');
    }
    function editSub(encKey) {
        AppUI.openModal(MODAL, 'Ubah Sub Menu', MenuUrl.editSub, { encKey: encKey }, 'lg');
    }
    function saveSub() {
        AppUI.submitForm('#frmSub', MenuUrl.simpanSub, null, MODAL).done(refreshSub);
    }
    function updateSub() {
        AppUI.submitForm('#frmSub', MenuUrl.updateSub, null, MODAL).done(function () { refreshSub(); refreshHeader(); });
    }
    function deleteSub(row) {
        AppUI.confirmPost('Nonaktifkan sub menu?', 'Sub menu ' + row.Name + ' tidak tampil lagi di sidebar.', MenuUrl.hapusSub, { Key: row.EncDecrKey }, 'Nonaktifkan', 'btn-danger')
            .done(function () { refreshSub(); refreshHeader(); });
    }

    /* ===== Grid (teks selalu lewat .text()) ===== */

    function iconText(container, icon, text) {
        if (icon) $('<i>').addClass(icon + ' me-1').attr('aria-hidden', 'true').appendTo(container);
        $('<span>').text(text || '').appendTo(container);
    }
    function nameIconTemplate(container, options) {
        iconText(container, options.data.Icon, options.value);
    }

    function controllerActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-list-settings-line', title: 'Daftar aksi', onClick: function () { actions(row); } }));
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah controller', onClick: function () { editController(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan controller', onClick: function () { deleteController(row); } }));
        }
    }

    function headerActionTemplate(container, options) {
        const row = options.data;
        if (!row.Controller) {
            container.append(AppUI.actionButton({ css: 'btn-outline-success', icon: 'ri-list-ordered', title: 'Atur sub menu grup', onClick: function () { detail(row); } }));
        }
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah header', onClick: function () { editHeader(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan header', onClick: function () { deleteHeader(row); } }));
        }
    }

    function subActionTemplate(container, options) {
        const row = options.data;
        container.append(AppUI.actionButton({ css: 'btn-outline-warning', icon: 'ri-edit-2-line', title: 'Ubah sub menu', onClick: function () { editSub(row.EncDecrKey); } }));
        if (row.Aktif) {
            container.append(AppUI.actionButton({ css: 'btn-outline-danger', icon: 'ri-forbid-line', title: 'Nonaktifkan sub menu', onClick: function () { deleteSub(row); } }));
        }
    }

    return {
        addController: addController, saveController: saveController, updateController: updateController, saveActions: saveActions,
        addHeader: addHeader, saveHeader: saveHeader, updateHeader: updateHeader, initDetail: initDetail, saveDetail: saveDetail,
        addSub: addSub, saveSub: saveSub, updateSub: updateSub,
        nameIconTemplate: nameIconTemplate,
        controllerActionTemplate: controllerActionTemplate, headerActionTemplate: headerActionTemplate, subActionTemplate: subActionTemplate
    };
})();
