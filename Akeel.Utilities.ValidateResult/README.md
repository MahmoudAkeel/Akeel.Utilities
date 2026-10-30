# Akeel.Utilities.ValidateResult

Represent a successful value or a failure with an error code and message. Version 1.0.3 targets .NET Standard 2.0 and declares no package dependencies.

```text
dotnet add package Akeel.Utilities.ValidateResult --version 1.0.3
```

```csharp
using System;
using Akeel.Utilities.ValidateResult;

ValidateResult<int> ParsePositive(string text)
{
    if (!int.TryParse(text, out var number) || number <= 0)
        return ValidateResult<int>.WithError(400, "Enter a positive integer");

    return ValidateResult<int>.WithSuccess(number);
}

var result = ParsePositive("-2");
Console.WriteLine($"{result.IsFailure}: {result.Error.Message}");
// True: Enter a positive integer
```

`ValidateResult<T>` provides `Data`, `WithSuccess` and `WithError` overloads. The base `ValidateResult` provides `IsSuccess`, `IsFailure`, `Error` and `ValidateRange` overloads; `ErrorDto.Factory` creates an error from a code or enum. `ToSuccessResult` and `ToErrorResult` are convenience extensions.

`ValidateRange` returns the first failing result's error; it does not aggregate errors. Check `IsSuccess` before using `Data`, and check `IsFailure` before using `Error`.

Source: [this directory](../Akeel.Utilities.ValidateResult). License: [MIT](../LICENSE). See [published versions and source provenance](../docs/package-versions.md).
