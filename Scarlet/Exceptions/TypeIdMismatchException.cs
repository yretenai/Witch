namespace Scarlet.Exceptions;

public sealed class TypeIdMismatchException : Exception {
    public TypeIdMismatchException(string message, TypeId providedTypeId, TypeId expectedTypeId) : base(message) {
        ProvidedTypeId = providedTypeId;
        ExpectedTypeId = expectedTypeId;
    }

    public TypeIdMismatchException(TypeId providedTypeId, TypeId expectedTypeId) : base($"Expected {expectedTypeId}, got {providedTypeId}") {
        ProvidedTypeId = providedTypeId;
        ExpectedTypeId = expectedTypeId;
    }

    public TypeIdMismatchException(string message, Exception innerException, TypeId providedTypeId, TypeId expectedTypeId) : base(message, innerException) {
        ProvidedTypeId = providedTypeId;
        ExpectedTypeId = expectedTypeId;
    }

    public TypeIdMismatchException(string message, Exception innerException) : base(message, innerException) { }

    public TypeIdMismatchException() { }

    public TypeIdMismatchException(string message) : base(message) { }

    public TypeId ProvidedTypeId { get; }
    public TypeId ExpectedTypeId { get; }
}
