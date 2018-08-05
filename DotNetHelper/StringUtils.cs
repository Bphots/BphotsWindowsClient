namespace DotNetHelper
{
    /// <summary>
    ///     Mostly extensions for common string manipulation.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        ///     Remove any combination of newlines. (System.Environment.NewLine might not always work)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveNewLines(this string input)
        {
            return input == null ? null : input.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }
    }
}