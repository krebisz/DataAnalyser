namespace DataVisualiser.Core.Validation.Parity;

public enum ParityMode
{
    Diagnostic, // return failures but do not throw
    Strict      // throw on first failure
}
