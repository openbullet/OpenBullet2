namespace OpenBullet2.Web.Tests.Utils;

public readonly struct Result<TValue, TError> {
    public TValue Value { get; }
    public TError Error { get; }
    public bool IsSuccess { get; }
 
    private Result(TValue v, TError e, bool success)
    {
        Value = v;
        Error = e;
        IsSuccess = success;
    }
 
    public static Result<TValue, TError?> Ok(TValue v) => new(v, default, true);

    public static Result<TValue?, TError> Err(TError e) => new(default, e, false);

    public static implicit operator Result<TValue, TError?>(TValue v) => new(v, default, true);
    public static implicit operator Result<TValue?, TError>(TError e) => new(default, e, false);
 
    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> failure) =>
        IsSuccess ? success(Value) : failure(Error);
}
