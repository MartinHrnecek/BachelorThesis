//===========================================================================
// File:        Result.cs
// Project:     ClassLibrary
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Generic Result type encapsulating either a successful
//              value or an error message, used as a lightweight
//              alternative to exception-based error propagation.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace ClassLibrary;

/// <summary>
/// Represents the outcome of an operation as either a successful value
/// of type <typeparamref name="T"/> or an error message.
/// </summary>
/// <typeparam name="T">The type of the wrapped success value.</typeparam>
public class Result<T>
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets the wrapped value if the operation succeeded; otherwise <c>default</c>.</summary>
    public T? Value { get; }

    /// <summary>Gets the error message if the operation failed; otherwise <c>null</c>.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Creates a successful result wrapping the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing <paramref name="value"/>.</returns>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>Creates a failed result with the specified error message.</summary>
    /// <param name="error">A description of the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying <paramref name="error"/>.</returns>
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Projects the wrapped value to a new type using the given mapping
    /// function, propagating failures and converting thrown exceptions
    /// into failed results.
    /// </summary>
    /// <typeparam name="TNew">The target type of the projection.</typeparam>
    /// <param name="mapper">The function applied to the wrapped value on success.</param>
    /// <returns>
    /// A new <see cref="Result{TNew}"/> containing the mapped value on
    /// success, or a failed result if the original was a failure or if
    /// <paramref name="mapper"/> threw an exception.
    /// </returns>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (!IsSuccess) return Result<TNew>.Failure(Error!);
        try
        {
            return Result<TNew>.Success(mapper(Value!));
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex.Message);
        }
    }

    /// <summary>Returns a string representation indicating success or failure.</summary>
    /// <returns>A human-readable representation of the result.</returns>
    public override string ToString()
        => IsSuccess ? $"Success({Value})" : $"Failure({Error})";
}