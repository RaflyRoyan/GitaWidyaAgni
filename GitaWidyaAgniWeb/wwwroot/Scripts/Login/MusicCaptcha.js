// Captcha login "seret ikon musik yang sama (bentuk & arah)" — pola IconCaptcha BEGateSecuritySystemWeb.
// Gambar target & pilihan dibuat server (Login/Captcha) dengan ID acak; ikon yang dijatuhkan diverifikasi server
// (Login/CaptchaVerifikasi) yang mengembalikan token sekali pakai untuk form login (#CaptchaToken).
// Keyboard: Tab ke ikon, Enter / Spasi untuk memilih. URL dari data-url-* di #musicCaptcha.
window.MusicCaptcha = (function () {
    var box = document.getElementById('musicCaptcha');
    var target = document.getElementById('mcTarget');
    var targetImg = document.getElementById('mcTargetImg');
    var wadah = document.getElementById('mcPilihan');
    var pesan = document.getElementById('mcPesan');
    var tokenInput = document.getElementById('CaptchaToken');
    var urlCaptcha = box.dataset.urlCaptcha;
    var urlVerifikasi = box.dataset.urlVerifikasi;

    var puzzleId = null, drag = null, terverifikasi = false, sibuk = false;
    var memuat = false, muatLagi = false, timerKedaluwarsa = null;
    var UMUR_PUZZLE_MS = 4.5 * 60 * 1000;  // server: 5 menit
    var UMUR_TOKEN_MS = 9.5 * 60 * 1000;   // server: 10 menit

    function tulis(teks, kelas) {
        pesan.textContent = teks || '';
        pesan.className = 'mc-pesan' + (kelas ? ' ' + kelas : '');
    }

    function goyang() {
        box.classList.remove('mc-gagal'); void box.offsetWidth; box.classList.add('mc-gagal');
    }

    function setKedaluwarsa(ms, pesanKedaluwarsa) {
        clearTimeout(timerKedaluwarsa);
        timerKedaluwarsa = setTimeout(function () {
            muat();
            if (pesanKedaluwarsa) tulis(pesanKedaluwarsa, 'error');
        }, ms);
    }

    function muat() {
        if (memuat || sibuk) { muatLagi = true; return; }
        memuat = true; muatLagi = false;
        clearTimeout(timerKedaluwarsa);
        terverifikasi = false; drag = null; puzzleId = null;
        tokenInput.value = '';
        box.classList.remove('mc-selesai', 'mc-gagal');
        target.classList.remove('valid', 'invalid', 'mc-hover');
        targetImg.removeAttribute('src');
        wadah.innerHTML = '<span class="mc-loading">Memuat captcha…</span>';
        tulis('');

        fetch(urlCaptcha, { cache: 'no-store', credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                if (res.Status !== 1 || !res.Data) { wadah.innerHTML = ''; tulis(res.Message || 'Captcha gagal dimuat.', 'error'); return; }
                puzzleId = res.Data.PuzzleId;
                targetImg.src = res.Data.Target;
                wadah.innerHTML = '';
                res.Data.Pilihan.forEach(function (p, i) {
                    var el = document.createElement('div');
                    el.className = 'mc-ikon';
                    el.tabIndex = 0;
                    el.dataset.id = p.Id;
                    el.setAttribute('role', 'button');
                    el.setAttribute('aria-label', 'Ikon pilihan ' + (i + 1) + ' (Enter untuk memilih)');
                    var img = document.createElement('img');
                    img.alt = '';
                    img.draggable = false;
                    img.src = p.Gambar;
                    el.appendChild(img);
                    el.addEventListener('pointerdown', mulai);
                    el.addEventListener('keydown', function (e) {
                        if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); kirim(el, 1000); }
                    });
                    wadah.appendChild(el);
                });
                setKedaluwarsa(UMUR_PUZZLE_MS);
            })
            .catch(function () { wadah.innerHTML = ''; tulis('Captcha gagal dimuat, klik ↻ untuk mencoba lagi.', 'error'); })
            .finally(selesaiRequest);
    }

    function selesaiRequest() {
        memuat = false; sibuk = false;
        if (muatLagi) muat();
    }

    function diAtasTarget(x, y) {
        var r = target.getBoundingClientRect();
        return x >= r.left && x <= r.right && y >= r.top && y <= r.bottom;
    }

    function mulai(e) {
        if (terverifikasi || sibuk || drag) return;
        var el = e.currentTarget;
        drag = { el: el, awalX: e.clientX, awalY: e.clientY, waktu: Date.now(), id: e.pointerId };
        el.setPointerCapture(e.pointerId);
        el.classList.add('mc-drag');
        el.addEventListener('pointermove', gerak);
        el.addEventListener('pointerup', selesai);
        el.addEventListener('pointercancel', batal);
        e.preventDefault();
    }

    function gerak(e) {
        if (!drag || e.pointerId !== drag.id) return;
        drag.el.style.transform = 'translate(' + (e.clientX - drag.awalX) + 'px,' + (e.clientY - drag.awalY) + 'px) scale(1.1)';
        target.classList.toggle('mc-hover', diAtasTarget(e.clientX, e.clientY));
    }

    function lepas() {
        var d = drag;
        drag = null;
        d.el.removeEventListener('pointermove', gerak);
        d.el.removeEventListener('pointerup', selesai);
        d.el.removeEventListener('pointercancel', batal);
        d.el.classList.remove('mc-drag');
        target.classList.remove('mc-hover');
        return d;
    }

    function batal() { if (drag) lepas().el.style.transform = ''; }

    function selesai(e) {
        if (!drag || e.pointerId !== drag.id) return;
        var kena = diAtasTarget(e.clientX, e.clientY);
        var d = lepas();
        d.el.style.transform = '';
        if (kena) kirim(d.el, Date.now() - d.waktu);
    }

    function kirim(el, durasi) {
        if (terverifikasi || sibuk || memuat || !puzzleId) return;
        sibuk = true;
        clearTimeout(timerKedaluwarsa);
        var body = new URLSearchParams();
        body.append('puzzleId', puzzleId);
        body.append('pilihan', el.dataset.id);
        body.append('durasi', durasi);
        var af = document.querySelector("input[name='__RequestVerificationToken']");
        if (af) body.append('__RequestVerificationToken', af.value);

        fetch(urlVerifikasi, { method: 'POST', body: body, credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                if (res.Status === 1 && res.Data && res.Data.Token) {
                    terverifikasi = true;
                    tokenInput.value = res.Data.Token;
                    target.classList.add('valid');
                    box.classList.add('mc-selesai');
                    el.style.visibility = 'hidden';
                    tulis('Terverifikasi', 'success');
                    setKedaluwarsa(UMUR_TOKEN_MS, 'Verifikasi kedaluwarsa, seret ikon yang sama lagi.');
                    return;
                }
                // Salah: puzzle sudah hangus di server, muat yang baru.
                target.classList.add('invalid');
                goyang();
                tulis(res.Message || 'Bentuk atau arah ikon belum tepat, coba lagi.', 'error');
                setTimeout(muat, 800);
            })
            .catch(function () { tulis('Koneksi gagal, coba lagi.', 'error'); setTimeout(muat, 800); })
            .finally(selesaiRequest);
    }

    document.getElementById('mcRefresh').addEventListener('click', muat);
    muat();

    return {
        terverifikasi: function () { return terverifikasi && !!tokenInput.value; },
        tandaiBelum: function () {
            goyang();
            tulis('Seret ikon yang sama ke kotak terlebih dahulu.', 'error');
            var first = wadah.querySelector('.mc-ikon');
            if (first) first.focus();
        }
    };
})();
