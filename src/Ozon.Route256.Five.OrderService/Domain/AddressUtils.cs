using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Domain;

public static class AddressUtils
{
    /// <summary>
    /// Расстояние между 2-мя географическими координатами
    /// </summary>
    /// <param name="baseCoordinates"></param>
    /// <param name="targetCoordinates"></param>
    /// <returns></returns>
    public static double DistanceToKm(this Coordinates baseCoordinates, Coordinates targetCoordinates)
    {
        var baseRad = Math.PI * baseCoordinates.Latitude / 180;
        var targetRad = Math.PI * targetCoordinates.Latitude / 180;
        var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
        var thetaRad = Math.PI * theta / 180;

        double dist =
            Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
            Math.Cos(targetRad) * Math.Cos(thetaRad);
        dist = Math.Acos(dist);

        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515 * 1.609344;

        return dist;
    }
}
