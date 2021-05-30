namespace DummyCompiler.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Gets whether a string is a number, no matter what type it is.
        /// </summary>
        /// <param name="token">String to parse.</param>
        /// <param name="number">Parsed number, if any.</param>
        public static bool IsNumber(this string token, out object number)
        {
            number = null;
            
            if (long.TryParse(token, out var nonFloating))
            {
                number = nonFloating;
            }
            else if (double.TryParse(token, out var floating))
            {
                number = floating;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}