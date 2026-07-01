# Sales Tax Puzzle

.NET console application that reads sale lines and prints a receipt with sales tax and totals.

## Requirements

- .NET 8 SDK

## Build

```powershell
dotnet build SalesTax.sln
```

## Test

```powershell
dotnet test SalesTax.sln
```

## Run

```powershell
dotnet run --project SalesPrompter
```

Enter sale lines in this format:

```text
<quantity> <description> at <unit price>
```

Example:

```text
1 book at 12.49
```

Submit a blank line to print the receipt.

## Notes

- The original project targeted `.NET Core 2.1`, which is end-of-life, so it was retargeted to `net8.0`.
- Tax rounding follows the supplied expected outputs: tax is rounded up per line to the next 0.05.
