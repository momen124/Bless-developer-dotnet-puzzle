# Solution Summary

## What Changed

- Fixed the `Sale` crash by initializing its line collection and ignoring invalid parsed lines.
- Fixed input parsing for multi-word descriptions, repeated spaces, the `at` separator, invariant-culture prices, and case-insensitive `imported`.
- Fixed imported tax so imported general goods pay 15% instead of 5%.
- Reworked tax rounding to use `decimal` math and round up to the next 0.05, matching the supplied examples.
- Fixed receipt formatting for values below 1.00 and blank receipts.
- Added xUnit coverage for the six supplied scenarios, parser edge cases, tax rounding, and constructor validation.
- Retargeted projects from `netcoreapp2.1` to `net8.0` because .NET Core 2.1 is end-of-life.

## Assumptions And Trade-Offs

- The instructions say "nearest 5 cents", but the expected receipts match rounding up to the next 0.05, so the implementation follows the examples.
- Tax is calculated on the total line value: `unit price x quantity`.
- Product classification uses simple case-insensitive substring matching for the supplied kata products. This is enough for books, chips, chocolates, and tablets, but a production system would use explicit product categories instead of keyword matching.
- The word `imported` is normalized into the printed product name so the receipt matches the expected output.

## Verification

Run:

```powershell
dotnet test SalesTax.sln
dotnet build SalesTax.sln
```

Expected result:

- All tests pass.
- Build succeeds.
