

namespace Blog.Foundation.Extensions
{
    public static class DecimalExtension
    {
        public static decimal PercentageOf(this decimal number, int percent)
        {
            return (decimal)(number * percent / 100);
        }
    }
}
