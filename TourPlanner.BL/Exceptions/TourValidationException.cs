namespace TourPlanner.BL.Exceptions
{
    public class TourValidationException : TourPlannerException
    {
        public TourValidationException(string message) : base(message) { }
        public TourValidationException(string message, Exception inner) : base(message, inner) { }
    }
}
