namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A result with two different possibilities
    /// </summary>
    /// <typeparam name="TOk">The type of the Ok result</typeparam>
    public struct Result<TOk>
    {
        private readonly TOk? _okValue;

        /// <summary>
        /// Is this an Ok result?
        /// </summary>
        public bool IsOk { get; init; }

        /// <summary>
        /// Is this an Error result?
        /// </summary>
        public bool IsError
        {
            get => !IsOk;
        }

        /// <summary>
        /// The Ok value
        /// </summary>
        public TOk Ok
        {
            get => _okValue!;
        }

        /// <summary>
        /// The error value
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Constructor (inaccessible - use static creation methods)
        /// </summary>
        /// <param name="isOk">Whether this is an Ok result</param>
        /// <param name="okValue">The value of the Ok result</param>
        /// <param name="error">The error message of the Error result</param>
        private Result(bool isOk, TOk? okValue = default, string error = "")
        {
            IsOk = isOk;
            _okValue = okValue;
            Error = error;
        }

        /// <summary>
        /// Create an Ok result
        /// </summary>
        /// <param name="okValue">The value of the Ok result</param>
        /// <returns>The Ok result</returns>
        public static Result<TOk> AsOk(TOk okValue) => new Result<TOk>(true, okValue);

        /// <summary>
        /// Create an Error result
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>The Error result</returns>
        public static Result<TOk> AsError(string error) => new Result<TOk>(false) { Error = error };
    }
}
