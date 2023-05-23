namespace Application.Converters
{
    public static class DateTimeConverter
    {
        public static string ToCommonViewString(this DateTime dateTime)
        {
            return $"{dateTime:dd/MM/yyyy HH:mm}";
        }
    }
}