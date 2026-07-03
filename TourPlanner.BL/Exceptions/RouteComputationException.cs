namespace TourPlanner.BL.Exceptions
{
    public class RouteComputationException : TourPlannerException
    {
        public RouteComputationException(string message) : base(message) { }
        public RouteComputationException(string message, Exception inner) : base(message, inner) { }
    }
}
