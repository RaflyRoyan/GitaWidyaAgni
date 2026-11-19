// Area Anggota › Presensi. URL dari window.PresensiSayaUrl (Views/AreaAnggota/PresensiSaya/Index.cshtml); helper global di js/site.js (AppUI).
// Modal check-in / check-out: (sesi mode QR: scan QR dulu) → foto selfie dari kamera langsung (cadangan: pilih dari galeri) → lokasi otomatis → simpan.
window.PresensiSaya = (function () {
    const URL_ = window.PresensiSayaUrl;
    const MAX_WIDTH = 1024;
    let sesi = null;      // data sesi dari tombol (data-*)
    let token = null;     // hasil scan / ketik kode QR (sesi mode QR)
    let photo = null;     // Blob / File foto selfie
    let pos = null;       // { lat, lng, acc }
    let stream = null;
    let qrScanner = null;
    let facing = 'user';
    let active = false;   // modal sedang terbuka (stream yang datang terlambat langsung dihentikan)

    function el(id) { return document.getElementById(id); }
    function pad(n) { return (n < 10 ? '0' : '') + n; }

    $(function () {
        $('.ps-seg button').on('click', function () {
            const panel = $(this).data('panel');
            $('.ps-seg button').removeClass('is-active').attr('aria-selected', 'false');
            $(this).addClass('is-active').attr('aria-selected', 'true');
            $('.ps-panel').prop('hidden', true);
            $('#panel-' + panel).prop('hidden', false);
        });
        tick();
        setInterval(tick, 1000);
        $('#modalAbsen').on('hidden.bs.modal', cleanup);
    });

    // Lama sesi berjalan sejak check-in (jj:mm:dd).
    function tick() {
        $('[data-sejak]').each(function () {
            const s = Math.max(0, Math.floor((Date.now() - new Date($(this).data('sejak')).getTime()) / 1000));
            $(this).text(Math.floor(s / 3600) + ':' + pad(Math.floor(s % 3600 / 60)) + ':' + pad(s % 60));
        });
    }

    /* ===== Modal ===== */

    function open(btn) {
        const d = btn.dataset;
        sesi = {
            id: Number(d.id), nama: d.nama || '', jam: d.jam || '', lokasi: d.lokasi || '',
            qr: d.qr === '1', keluar: d.keluar === '1',
            lat: d.lat ? parseFloat(d.lat) : null, lng: d.lng ? parseFloat(d.lng) : null, radius: d.radius ? Number(d.radius) : null
        };
        token = null;
        photo = null;
        pos = null;
        active = true;
        facing = 'user';

        const aksi = sesi.keluar ? 'Check-Out' : 'Check-In';
        $('#modalAbsenTitle').text(aksi + ' · ' + sesi.nama);
        $('#absNama').text(sesi.nama);
        $('#absInfo').text([sesi.jam, sesi.lokasi].filter(Boolean).join(' · '));
        const now = new Date();
        $('#absJam').empty().append($('<i class="ri-time-line">'), document.createTextNode(' ' + pad(now.getHours()) + ':' + pad(now.getMinutes())));
        $('#absSubmit span').text('Simpan ' + aksi);
        $('#absQrManual').val('');
        $('#absCamTitle').text(sesi.qr ? '2. Foto selfie' : 'Foto selfie');

        bootstrap.Modal.getOrCreateInstance(el('modalAbsen')).show();
        locate();
        if (sesi.qr) {
            $('#absQrStep').prop('hidden', false);
            $('#absCamStep').prop('hidden', true);
            startQr();
        } else {
            $('#absQrStep').prop('hidden', true);
            $('#absCamStep').prop('hidden', false);
            startCamera();
        }
        update();
    }

    function cleanup() {
        active = false;
        stopStream();
        stopQr();
    }

    function update() {
        if (!sesi) return;
        const needLoc = sesi.lat !== null && sesi.radius;
        const ready = !!photo && (!sesi.qr || !!token) && (!!pos || !needLoc);
        $('#absSubmit').prop('disabled', !ready);
    }

    /* ===== QR (sesi mode Wajib Scan QR) ===== */

    function startQr() {
        if (typeof Html5Qrcode === 'undefined') return;
        qrScanner = new Html5Qrcode('absQrReader');
        qrScanner.start({ facingMode: 'environment' }, { fps: 10, qrbox: 220 }, function (text) {
            gotToken(text);
        }).catch(function () {
            qrScanner = null;
            AppUI.alertWarning('Kamera tidak bisa dibuka untuk scan. Ketik kode sesi di bawahnya.');
        });
    }
    function stopQr() {
        if (!qrScanner) return Promise.resolve();
        const s = qrScanner;
        qrScanner = null;
        return s.stop().catch(function () { }).then(function () { try { s.clear(); } catch (e) { /* sudah bersih */ } });
    }
    function gotToken(text) {
        token = $.trim(String(text || ''));
        if (!token) return;
        stopQr().then(function () {
            if (!active) return;
            $('#absQrStep').prop('hidden', true);
            $('#absCamStep').prop('hidden', false);
            startCamera();
            update();
        });
    }
    function useCode() {
        const v = $.trim($('#absQrManual').val() || '');
        if (!v) {
            AppUI.alertWarning('Scan QR sesi atau ketik kodenya.');
            return;
        }
        gotToken(v);
    }

    /* ===== Kamera ===== */

    function stopStream() {
        if (stream) stream.getTracks().forEach(function (t) { t.stop(); });
        stream = null;
    }
    function showCamera() {
        $('#absVideo').prop('hidden', false).css('transform', facing === 'user' ? 'scaleX(-1)' : 'none');
        $('#absPreview, #absFallback').prop('hidden', true);
        $('#absCamBar').prop('hidden', false);
        $('#absSnap').prop('hidden', false);
        $('#absRetake').prop('hidden', true);
    }
    function fallback() {
        stopStream();
        $('#absVideo, #absPreview, #absCamBar').prop('hidden', true);
        $('#absFallback').prop('hidden', false);
    }
    function startCamera() {
        photo = null;
        stopStream();
        showCamera();
        update();
        const md = navigator.mediaDevices;
        if (!md || !md.getUserMedia) {
            fallback();
            return;
        }
        if (md.enumerateDevices) {
            md.enumerateDevices().then(function (ds) {
                $('#absSwitch').prop('hidden', ds.filter(function (x) { return x.kind === 'videoinput'; }).length < 2);
            }).catch(function () { });
        }
        md.getUserMedia({ video: { facingMode: facing }, audio: false })
            .catch(function () { return md.getUserMedia({ video: true, audio: false }); })
            .then(function (s) {
                if (!active) {
                    s.getTracks().forEach(function (t) { t.stop(); });
                    return;
                }
                stream = s;
                el('absVideo').srcObject = s;
            })
            .catch(fallback);
    }
    function switchCamera() {
        facing = facing === 'user' ? 'environment' : 'user';
        startCamera();
    }
    // Rata-rata kecerahan sangat rendah = kamera hitam / tertutup.
    function isBlack(ctx, w, h) {
        const data = ctx.getImageData(0, 0, w, h).data;
        let sum = 0, n = 0;
        for (let i = 0; i < data.length; i += 80) {
            sum += 0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2];
            n++;
        }
        return n > 0 && sum / n < 10;
    }
    function snap() {
        const video = el('absVideo');
        const canvas = el('absCanvas');
        const vw = video.videoWidth || 640, vh = video.videoHeight || 480;
        const w = Math.min(vw, MAX_WIDTH), h = Math.round(vh * w / vw);
        canvas.width = w;
        canvas.height = h;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(video, 0, 0, w, h);
        if (!stream || isBlack(ctx, w, h)) {
            AppUI.alertWarning('Foto terdeteksi gelap / kamera belum siap. Coba lagi, atau pilih foto dari galeri.');
            return;
        }
        canvas.toBlob(function (b) { if (b) setPhoto(b); }, 'image/jpeg', 0.8);
    }
    function setPhoto(file) {
        photo = file;
        stopStream();
        el('absPreview').src = URL.createObjectURL(file);
        $('#absPreview').prop('hidden', false);
        $('#absVideo, #absFallback, #absSnap, #absSwitch').prop('hidden', true);
        $('#absCamBar, #absRetake').prop('hidden', false);
        update();
    }
    function pickFile(input) {
        const f = input.files && input.files[0];
        input.value = '';
        if (!f) return;
        if (!/^image\//.test(f.type)) {
            AppUI.alertWarning('Pilih berkas gambar (JPG / PNG).');
            return;
        }
        setPhoto(f);
    }

    /* ===== Lokasi ===== */

    function distance(lat1, lon1, lat2, lon2) {
        const r = Math.PI / 180, R = 6371000;
        const dLat = (lat2 - lat1) * r, dLon = (lon2 - lon1) * r;
        const a = Math.sin(dLat / 2) ** 2 + Math.cos(lat1 * r) * Math.cos(lat2 * r) * Math.sin(dLon / 2) ** 2;
        return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    }
    function setLoc(css, icon, text) {
        $('#absLoc').attr('class', 'ps-loc ' + css).empty().append($('<i>').addClass(icon), $('<span>').text(text));
    }
    function locate() {
        pos = null;
        $('#absCoord, #absLocRetry').prop('hidden', true);
        setLoc('', 'ri-loader-4-line', 'Mengambil lokasi...');
        update();
        if (!navigator.geolocation) {
            setLoc('err', 'ri-map-pin-line', 'Perangkat tidak mendukung lokasi.');
            return;
        }
        navigator.geolocation.getCurrentPosition(function (p) {
            if (!active) return;
            pos = { lat: p.coords.latitude, lng: p.coords.longitude, acc: p.coords.accuracy };
            $('#absLat').text(pos.lat.toFixed(6));
            $('#absLng').text(pos.lng.toFixed(6));
            $('#absCoord, #absLocRetry').prop('hidden', false);
            if (sesi.lat !== null && sesi.radius) {
                const d = Math.round(distance(sesi.lat, sesi.lng, pos.lat, pos.lng));
                if (d <= sesi.radius) setLoc('ok', 'ri-map-pin-2-fill', 'Dalam area presensi (±' + d + ' m dari titik, maks ' + sesi.radius + ' m).');
                else setLoc('warn', 'ri-error-warning-line', 'Di luar area: ' + d + ' m dari titik (maks ' + sesi.radius + ' m). Mendekat lalu ambil ulang lokasi.');
            } else {
                setLoc('ok', 'ri-map-pin-2-fill', 'Lokasi didapat (akurasi ±' + Math.round(pos.acc) + ' m).');
            }
            update();
        }, function () {
            if (!active) return;
            setLoc('err', 'ri-map-pin-line', 'Lokasi tidak diizinkan. Aktifkan GPS dan izinkan akses lokasi, lalu ambil ulang.');
            $('#absLocRetry').prop('hidden', false);
            update();
        }, { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 });
    }

    /* ===== Simpan ===== */

    function submit() {
        if (!sesi || !photo) return;
        const fd = new FormData();
        fd.append('keluar', sesi.keluar ? 'true' : 'false');
        if (token) fd.append('token', token);
        else fd.append('sesi', String(sesi.id));
        fd.append('foto', photo instanceof File ? photo : new File([photo], 'selfie.jpg', { type: 'image/jpeg' }));
        if (pos) {
            fd.append('lat', pos.lat.toFixed(7));
            fd.append('lng', pos.lng.toFixed(7));
        }
        const btn = $('#absSubmit').prop('disabled', true);
        AppUI.postForm(URL_.kirim, fd).done(function (r) {
            bootstrap.Modal.getOrCreateInstance(el('modalAbsen')).hide();
            AppUI.alertSuccess(r.Message + (r.Data ? ' Status: ' + r.Data + '.' : '')).then(function () { window.location.reload(); });
        }).always(function () { update(); btn.blur(); });
    }

    return { open: open, useCode: useCode, startCamera: startCamera, switchCamera: switchCamera, snap: snap, pickFile: pickFile, locate: locate, submit: submit };
})();
