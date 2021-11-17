namespace Blog.Foundation.Extensions
{
    public static class NumberExtension
    {
        public static int TryParse(string input)
        {
            var defaultValue = 0;
            return int.TryParse(input, out var value) ? value : defaultValue;
        }

        public static int TryParse(this string input, int defaultValue)
        {
            return int.TryParse(input, out var value) ? value : defaultValue;
        }

    }
}