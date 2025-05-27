namespace MooBot.Managers.Enums
{
    public enum BooruRating
    {
        G,
        S,
        Safe,
        Questionable,
        Q,
        Explicit,
        E
    }

    public static class BooruRatingExtensions
    {
        public static bool IsSFW(this BooruRating booruRating)
        {
            return booruRating == BooruRating.G || booruRating == BooruRating.S || booruRating == BooruRating.Safe;
        }

        public static bool IsNSFW(this BooruRating booruRating)
        {
            return booruRating == BooruRating.Q || booruRating == BooruRating.Questionable || booruRating == BooruRating.E || booruRating == BooruRating.Explicit;
        }
    }
}
