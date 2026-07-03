namespace TourPlanner.BL.Exceptions
{
    public class TourPlannerException : Exception
    {
        public TourPlannerException(string message) : base(message) { }
        public TourPlannerException(string message, Exception inner) : base(message, inner) { }
    }
}
