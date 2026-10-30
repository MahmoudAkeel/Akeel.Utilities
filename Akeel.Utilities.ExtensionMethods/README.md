# Akeel.Utilities.ExtensionMethods

Small extension methods for strings, collections, booleans, objects, numbers, enums, dictionaries and dates. Version 1.0.1 targets .NET Standard 2.0 and declares no package dependencies.

```text
dotnet add package Akeel.Utilities.ExtensionMethods --version 1.0.1
```

```csharp
using System;
using Akeel.Utilities.ExtensionMethods;

Console.WriteLine("Order #42".ToDigits()); // 42
Console.WriteLine("  ".IsNullOrEmpty()); // True; whitespace counts as empty
```

Other available methods include `AnyGuard` and `EmptyOrNull` for `ICollection<T>`, `IsTrue`/`IsFalse`, `IsNull`/`IsNotNull`, `Positive`/`Negative`, `ToNumber`/`ToNumberString`/`ToByte`, `TryGetValueOrDefault`, `ToStartDayTime`, `ToLastDayTime` and `ToArabicDayName`.

`ToDigits` returns 0 when the extracted digits do not parse as an `int`. `IsNullOrEmpty` uses whitespace semantics. Date methods return a new `DateTime` at the selected wall-clock time; they do not convert time zones. The enum conversion methods are intended for values with compatible numeric representations.

Source: [this directory](../Akeel.Utilities.ExtensionMethods). License: [MIT](../LICENSE). See [published versions and source provenance](../docs/package-versions.md).
