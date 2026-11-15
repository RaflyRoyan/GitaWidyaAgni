using GWAShared.Results;
using System;

namespace GWALib.Lib.Transaksi.Presensi
{
    /// <summary>
    /// Validasi titik lokasi + radius (geofence) untuk jadwal / sesi presensi dan pengecekan posisi check-in / check-out.
    /// </summary>
    internal static class GeoFence
    {
        private const double EARTH_RADIUS_METER = 6371000;

        // Koordinat harus berpasangan dan dalam rentang; radius hanya bila koordinat ada dan harus > 0.
        internal static void ValidateArea(decimal? latitude, decimal? longitude, int? radiusMeter)
        {
            if ((latitude == null) != (longitude == null) || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Koordinat Lokasi"));
            }
            if (radiusMeter != null && (radiusMeter <= 0 || latitude == null))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Radius Lokasi"));
            }
        }
        // Tanpa titik + radius = lokasi tidak dicek. Dengan geofence, posisi wajib dikirim dan harus di dalam radius.
        internal static void ValidatePosition(decimal? areaLatitude, decimal? areaLongitude, int? radiusMeter, decimal? latitude, decimal? longitude)
        {
            if (areaLatitude == null || areaLongitude == null || radiusMeter == null)
            {
                return;
            }
            if (latitude == null || longitude == null)
            {
                throw new ArgumentException(ExceptionMessage.ATTENDANCE_LOCATION_REQUIRED);
            }
            var distance = Distance((double)areaLatitude, (double)areaLongitude, (double)latitude, (double)longitude);
            if (distance > radiusMeter.Value)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ATTENDANCE_OUT_OF_RANGE, Math.Round(distance), radiusMeter.Value));
            }
        }
        // Jarak haversine dalam meter.
        internal static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadian(lat2 - lat1);
            var dLon = ToRadian(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return EARTH_RADIUS_METER * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
        private static double ToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
