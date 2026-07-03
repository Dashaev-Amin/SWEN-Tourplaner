namespace TourPlanner.BL.Exceptions
{
    public class TourNotFoundException : TourPlannerException
    {
        public TourNotFoundException(Guid tourId)
            : base($"Tour mit ID '{tourId}' wurde nicht gefunden.") { }
    }
}
