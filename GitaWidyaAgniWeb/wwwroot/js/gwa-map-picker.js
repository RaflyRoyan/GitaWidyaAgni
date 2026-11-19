// Peta pemilih titik lokasi + radius untuk dxForm yang punya field Latitude / Longitude / RadiusMeter (Leaflet + OpenStreetMap).
// Klik peta = taruh titik; lingkaran = radius; tombol "Lokasi saya" (GPS) dan "Lokasi kegiatan" (koordinat t_activity).
// Dipakai lewat Views/Shared/_PetaLokasi.cshtml: GwaMapPicker.attach({ formId, mapId, koordinat: { activityId: [lat, lng] } }).
window.GwaMapPicker = (function () {
    const DEFAULT_CENTER = [-7.3333, 112.7884]; // UPN "Veteran" Jawa Timur
    const DEFAULT_RADIUS = 200;
    const COLOR_POINT = '#9a6b00', COLOR_FILL = '#e8b52a', COLOR_RADIUS = '#c8202a';

    function attach(o) {
        let tries = 0;
        (function wait() {
            const form = $('#' + o.formId).data('dxForm');
            if (!form || typeof L === 'undefined' || !document.getElementById(o.mapId)) {
                if (tries++ < 60) setTimeout(wait, 50);
                return;
            }
            init(form, o);
        })();
    }

    function activityPoint(o, activityId) {
        const k = o.koordinat && activityId ? o.koordinat[activityId] : null;
        return k && k.length === 2 ? [Number(k[0]), Number(k[1])] : null;
    }

    function init(form, o) {
        const el = document.getElementById(o.mapId);
        const $wrap = $(el).closest('.gwa-map');
        const data = form.option('formData') || {};
        const has = data.Latitude != null && data.Longitude != null;
        const start = has ? [Number(data.Latitude), Number(data.Longitude)] : (activityPoint(o, data.ActivityId) || DEFAULT_CENTER);
        const map = L.map(el, { scrollWheelZoom: false }).setView(start, has ? 17 : 15);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: '&copy; OpenStreetMap' }).addTo(map);
        let marker = null, circle = null;

        function draw() {
            const d = form.option('formData') || {};
            if (d.Latitude == null || d.Longitude == null || d.Latitude === '' || d.Longitude === '') {
                if (marker) {
                    map.removeLayer(marker);
                    map.removeLayer(circle);
                    marker = circle = null;
                }
                return;
            }
            const ll = [Number(d.Latitude), Number(d.Longitude)];
            const r = Number(d.RadiusMeter) || 0;
            if (!marker) {
                circle = L.circle(ll, { radius: r, color: COLOR_RADIUS, weight: 1, fillOpacity: 0.12 }).addTo(map);
                marker = L.circleMarker(ll, { radius: 8, color: COLOR_POINT, fillColor: COLOR_FILL, fillOpacity: 1, weight: 2 }).addTo(map);
            } else {
                marker.setLatLng(ll);
                circle.setLatLng(ll).setRadius(r);
            }
        }
        function fit() {
            map.invalidateSize();
            if (circle && circle.getRadius() > 0) map.fitBounds(circle.getBounds(), { maxZoom: 18, padding: [20, 20] });
            else if (marker) map.setView(marker.getLatLng(), 17);
        }
        function set(lat, lng, pan) {
            form.updateData('Latitude', Number(lat.toFixed(7)));
            form.updateData('Longitude', Number(lng.toFixed(7)));
            if (!form.option('formData').RadiusMeter) form.updateData('RadiusMeter', DEFAULT_RADIUS);
            draw();
            if (pan) fit();
        }

        map.on('click', function (e) { set(e.latlng.lat, e.latlng.lng, false); });
        form.on('fieldDataChanged', function (e) {
            if (e.dataField === 'Latitude' || e.dataField === 'Longitude' || e.dataField === 'RadiusMeter') draw();
            if (e.dataField === 'ActivityId') {
                const k = activityPoint(o, e.value);
                const d = form.option('formData');
                if (k && (d.Latitude == null || d.Longitude == null)) set(k[0], k[1], true);
                else if (k) map.setView(k, map.getZoom());
            }
        });
        $wrap.find('[data-peta="saya"]').on('click', function () {
            if (!navigator.geolocation) {
                AppUI.alertWarning('Perangkat tidak mendukung lokasi.');
                return;
            }
            const btn = $(this).prop('disabled', true);
            navigator.geolocation.getCurrentPosition(function (p) {
                btn.prop('disabled', false);
                set(p.coords.latitude, p.coords.longitude, true);
            }, function () {
                btn.prop('disabled', false);
                AppUI.alertWarning('Lokasi tidak diizinkan. Klik titik di peta secara manual.');
            }, { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 });
        });
        $wrap.find('[data-peta="kegiatan"]').on('click', function () {
            const k = activityPoint(o, form.option('formData').ActivityId);
            if (!k) {
                AppUI.alertWarning('Kegiatan ini belum punya koordinat. Klik titik di peta atau pakai lokasi Anda.');
                return;
            }
            set(k[0], k[1], true);
        });

        draw();
        // Peta di dalam modal: ukur ulang setelah modal tampil.
        setTimeout(fit, 250);
        $(el).closest('.modal').one('shown.bs.modal', fit);
    }

    return { attach: attach };
})();
