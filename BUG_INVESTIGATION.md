# Bug Investigation

This document records the issues found during review before changing the code.

## Observed Behavior Against Supplied Tests

These results were captured by running the original application under `.NET Core Runtime 2.1.30`.

Command shape used:

```powershell
@'
<test input>

'@ | dotnet SalesPrompter\bin\Debug\netcoreapp2.1\SalesPrompter.dll
```

### Supplied Test Result Matrix

| Test | Does it crash? | Does it reject input? | Does it produce wrong output? | File/line causing observed failure |
| --- | --- | --- | --- | --- |
| Test 1 | Yes | No explicit rejection before crash | No receipt produced | `SalesTax/InputParser.cs:64` |
| Test 2 | Yes | Yes, parser returns `null` because word count is greater than 4 | No receipt produced | `SalesTax/Sale.cs:23` |
| Test 3 | Yes | Yes, parser returns `null` because word count is greater than 4 | No receipt produced | `SalesTax/Sale.cs:23` |
| Test 4 | Yes | Yes, parser returns `null` because word count is greater than 4 | No receipt produced | `SalesTax/Sale.cs:23` |
| Test 5 | Yes | Parser returns `null` because quantity is invalid, but app crashes instead of showing a nice error | No nice error produced | `SalesTax/Sale.cs:23` |
| Test 6 | Yes | Not applicable; blank input ends entry loop | No blank receipt produced | `SalesTax/Sale.cs:53` |

### Detailed Test Observations

#### Test 1

Input begins with:

```text
1 book at 12.49
```

Observed result:

- Crashes immediately.
- No receipt is printed.

Stack trace points to:

```text
SalesTax.InputParser.ProcessInput(...) in SalesTax/InputParser.cs:line 64
SalesTax.Sale.Add(...) in SalesTax/Sale.cs:line 22
SalesPrompter.Program.Main(...) in SalesPrompter/Program.cs:line 18
```

Cause:

- `InputParser` calls `string.Join(" ", words, 1, wordCount)`.
- For a four-token input, this tries to join four items starting from index `1`, which goes past the array end.

#### Test 2

Input begins with:

```text
1 imported box of chips at 10.00
```

Observed result:

- Crashes immediately.
- No receipt is printed.

Stack trace points to:

```text
SalesTax.Sale.Add(...) in SalesTax/Sale.cs:line 23
SalesPrompter.Program.Main(...) in SalesPrompter/Program.cs:line 18
```

Cause:

- `InputParser` rejects input with more than four words.
- It returns `null`.
- `Sale.Add` tries to call `saleLines.Add(saleLine)` while `saleLines` is still `null`.

#### Test 3

Input begins with:

```text
1 barrell of imported oil at 27.99
```

Observed result:

- Crashes immediately.
- No receipt is printed.

Cause:

- Same as Test 2: parser rejects the multi-word description, then `Sale.Add` crashes at `SalesTax/Sale.cs:23`.

#### Test 4

Input begins with:

```text
10 imported bottles of whiskey at 27.99
```

Observed result:

- Crashes immediately.
- No receipt is printed.

Cause:

- Same as Test 2: parser rejects the multi-word description, then `Sale.Add` crashes at `SalesTax/Sale.cs:23`.

#### Test 5

Input:

```text
js s jss s
```

Observed result:

- Crashes immediately.
- The expected "nicely handled error" is not shown.

Cause:

- `InputParser` correctly fails to parse the quantity and returns `null`.
- `Sale.Add` does not handle the `null` result and crashes at `SalesTax/Sale.cs:23`.

#### Test 6

Input:

```text
<blank line>
```

Observed result:

- Crashes while trying to print the blank receipt.

Stack trace points to:

```text
SalesTax.Sale.ToString(...) in SalesTax/Sale.cs:line 53
SalesPrompter.Program.Main(...) in SalesPrompter/Program.cs:line 22
```

Cause:

- `saleLines` is never initialized.
- `ToString()` tries to iterate over `saleLines`.

### Important Note About Hidden Failures

Because the application crashes so early, several known calculation and formatting bugs are not visible in full end-to-end output yet.

After the first crash and parser bugs are fixed, the next expected failures are:

- Imported general products will be taxed at 5% instead of 15%.
- Chocolates will be treated as taxable even though the expected result treats them as food.
- Amounts below `1.00` can print without a leading zero, for example `.85` instead of `0.85`.
- Money rounding may be unstable because tax calculation uses `double`.

## Critical Runtime Bugs

### 1. `Sale.saleLines` is never initialized

File: `SalesTax/Sale.cs`

`saleLines` is declared but never assigned a `List<SaleLine>` instance.

Impact:

- The first valid call to `Sale.Add(...)` crashes with a `NullReferenceException`.
- Test 6, blank receipt, also crashes when `ToString()` tries to loop over `saleLines`.

Priority: Critical.

### 2. Invalid input is not handled safely

File: `SalesTax/Sale.cs`

`InputParser.ProcessInput(...)` can return `null`, but `Sale.Add(...)` immediately does this:

```csharp
saleLines.Add(saleLine);
totalTax += saleLine.Tax;
```

Impact:

- Invalid input causes a crash instead of returning `false`.
- Test 5 requires a nicely handled error.

Priority: Critical.

## Parsing Bugs

### 3. Parser rejects normal product descriptions

File: `SalesTax/InputParser.cs`

The code says there must be at least four words, but the condition is reversed:

```csharp
if (wordCount > 4)
    return null;
```

Impact:

- Inputs like `1 music CD at 14.99` and `1 imported box of chips at 10.00` are rejected.
- Most supplied test cases cannot be parsed.

Priority: Critical.

### 4. Product name includes too many words

File: `SalesTax/InputParser.cs`

The product name is built with:

```csharp
productName = string.Join(" ", words, 1, wordCount);
```

This includes the word `at` and the price in the product name.

Impact:

- Receipt descriptions are incorrect.
- Tax classification can be affected by unrelated words.

Priority: High.

### 4a. Product name construction can throw `ArgumentOutOfRangeException`

File: `SalesTax/InputParser.cs`

The parser uses:

```csharp
productName = string.Join(" ", words, 1, wordCount);
```

For a valid four-token input like:

```text
1 book at 12.49
```

`words.Length` is `4`, but `string.Join` starts at index `1` and tries to read `4` items. That reaches past the end of the array.

Impact:

- Even simple valid input can crash before a `SaleLine` is created.
- The method comment says parse failures return `null`, but this exception is not caught.

Priority: Critical.

### 5. Parser does not explicitly validate the `at` separator

File: `SalesTax/InputParser.cs`

The code assumes the last word is the price, but does not check that the second-last word is `at`.

Impact:

- Malformed input may be accepted accidentally.
- Error handling is inconsistent.

Priority: Medium.

### 6. Imported item detection is fragile

File: `SalesTax/InputParser.cs`

The parser checks:

```csharp
productName.Contains("imported ")
```

Impact:

- Case differences are not handled.
- `imported` at the end of a description is not detected.
- The parser mutates the product name with a broad string replacement.

Priority: Medium.

### 6a. Parser is sensitive to repeated spaces

File: `SalesTax/InputParser.cs`

The parser uses:

```csharp
input.Split(' ')
```

This keeps empty entries when the user types repeated spaces.

Impact:

- Input that visually follows the format can be rejected or parsed incorrectly.
- Example: `1  book at 12.49` produces an empty token.

Priority: Medium.

### 6b. Parser uses current culture for decimal parsing

File: `SalesTax/InputParser.cs`

The parser uses:

```csharp
decimal.Parse(...)
```

without specifying `CultureInfo`.

Impact:

- `12.49` may fail or be interpreted differently on machines whose culture uses a comma decimal separator.
- The challenge examples use dot decimal notation, so parsing should be deterministic.

Priority: Medium.

### 6c. Quantity and price validation is incomplete

File: `SalesTax/InputParser.cs`

The parser accepts any integer quantity and any decimal price if parsing succeeds.

Impact:

- `0 book at 12.49` can be accepted.
- `-1 book at 12.49` can be accepted.
- `1 book at -12.49` can be accepted.
- These values do not make sense for a sales receipt and can create negative totals.

Priority: Medium.

## Tax Calculation Bugs

### 7. Imported general products are taxed at 5% instead of 15%

File: `SalesTax/SaleLine.cs`

The code sets base tax first, but then overwrites it for imported products:

```csharp
if (isImported)
    taxRate = 5;
```

Impact:

- Imported taxable goods are under-taxed.
- Example: imported transformer should have 15% tax, not 5%.

Priority: Critical.

### 8. Tax rounding uses `double` for money

File: `SalesTax/SaleLine.cs`

`CalculateTax` converts decimal money values to `double`.

Impact:

- Floating-point precision can create incorrect rounding.
- Money calculations should stay in `decimal`.

Priority: High.

### 9. Rounding implementation is unclear

File: `SalesTax/SaleLine.cs`

The code rounds to two decimals first, then rounds up to the next 0.05.

Impact:

- The intended rule is rounding sales tax per line to the nearest 5 cents.
- The implementation should be explicit and tested against the expected outputs.

Priority: High.

### 9a. The written rounding rule is ambiguous, but the examples imply round-up

File: `SalesTax/SaleLine.cs`

The instructions say:

```text
Round sales tax per line to the nearest 5 cents.
```

However, the expected outputs match the common sales-tax-kata rule of rounding tax upward to the next 0.05.

Example:

- `14.99 * 10% = 1.499`
- Expected tax is `1.50`

Impact:

- The implementation and documentation should be explicit: use line-level rounding up to the next 0.05 to match the provided examples.

Priority: Medium.

## Classification Bugs

### 10. Exempt products are detected using broad substring checks

File: `SalesTax/SaleLine.cs`

The code checks:

```csharp
productName.Contains("book") || productName.Contains("tablet") || productName.Contains("chip")
```

Impact:

- Some expected examples work by accident.
- Other item names may be misclassified.
- The medical category is represented only by `tablet`, which is incomplete.

Priority: Medium.

### 11. Chocolates are food but not exempt

File: `SalesTax/SaleLine.cs`

The expected results treat imported chocolates as food, so they should only receive import tax.

Impact:

- Test 3 will calculate too much tax for `box of imported chocolates`.

Priority: High.

### 11a. Substring matching can create false exemptions

File: `SalesTax/SaleLine.cs`

Current logic checks:

```csharp
productName.Contains("book")
productName.Contains("tablet")
productName.Contains("chip")
```

Impact:

- `notebook` would be treated as a book.
- `microchip` would be treated as food because it contains `chip`.
- Case differences are not handled.

Priority: Medium.

### 11b. Medical and food categories are under-specified

File: `SalesTax/SaleLine.cs`

The business rule says:

```text
No Tax on books, medical items, food
```

The current code only recognizes:

- `book`
- `tablet`
- `chip`

Impact:

- `headache tablets` works by substring accident.
- `iodine tablets` works by substring accident.
- `chocolates` does not work.
- Any other food or medical item would likely be taxed incorrectly.

Priority: Medium for the puzzle, higher for production.

## Maintainability Issues

### 12. Projects target out-of-support .NET Core 2.1

Files:

- `SalesTax/SalesTax.csproj`
- `SalesPrompter/SalesPrompter.csproj`

Both projects target:

```xml
<TargetFramework>netcoreapp2.1</TargetFramework>
```

Observed result:

- `dotnet build SalesTax.sln` succeeds on the installed .NET 8 SDK.
- The build emits `NETSDK1138` because `.NET Core 2.1` is out of support.
- The build also emits vulnerability warnings for `Microsoft.NETCore.App` 2.1.0.
- `dotnet run --project SalesPrompter/SalesPrompter.csproj` fails because the .NET Core 2.1 runtime is not installed.

Impact:

- The application may not run on a modern developer machine.
- Security and support warnings reduce solution quality.

Priority: High.

Recommendation:

- Retarget both projects to a supported framework such as `net8.0`, unless the assessment explicitly requires staying on `.NET Core 2.1`.

### 12. Unused imports

Files:

- `SalesTax/InputParser.cs`
- `SalesTax/Sale.cs`

Impact:

- Small readability issue.

Priority: Low.

### 13. Console app requires an extra final Enter

File: `SalesPrompter/Program.cs`

After printing the receipt, the program calls:

```csharp
Console.ReadLine();
```

Impact:

- This is acceptable for a manual console app.
- It can make automated testing or redirected input less convenient.

Priority: Low.

### 13a. Receipt line formatting omits leading zero for values below 1.00

File: `SalesTax/SaleLine.cs`

Line output uses:

```csharp
{2:#,###.00}
```

Observed with .NET formatting:

```text
0.85 -> .85
0 -> .00
```

Expected output requires:

```text
0.85
```

Impact:

- Test 1 line `1 packet of chips: 0.85` would print incorrectly if execution reached that point.

Priority: High.

Recommendation:

- Use a format with a mandatory leading zero, for example `{0:#,##0.00}`.

### 14. Mutable backing fields add noise

File: `SalesTax/SaleLine.cs`

The class uses many private fields with simple getters. Private setters or readonly fields would make the object easier to reason about.

Priority: Low.

### 15. Tax rules are hard-coded inside the constructor

File: `SalesTax/SaleLine.cs`

Impact:

- Business rules are harder to test and maintain.
- Changes to exempt categories require editing constructor logic.

Priority: Medium.

### 16. Public method contracts are inconsistent

Files:

- `SalesTax/InputParser.cs`
- `SalesTax/Sale.cs`

The parser comment says invalid input returns `null`, but some invalid or valid-looking inputs can throw. `Sale.Add(...)` says it returns `false` for formatting failures, but it does not handle `null` from the parser.

Impact:

- Callers cannot rely on the documented behavior.
- Console error handling is bypassed by exceptions.

Priority: High.

### 17. No automated tests are present

Files:

- Solution root

There is no test project in the solution.

Impact:

- The six supplied examples are only documented manually.
- Future fixes could regress calculations or formatting.

Priority: High.

### 18. Build artifacts are generated locally after investigation

Files:

- `SalesPrompter/bin/`
- `SalesPrompter/obj/`
- `SalesTax/bin/`
- `SalesTax/obj/`

These folders were generated by `dotnet build` and are ignored by `.gitignore`.

Impact:

- They should not be included in the final patch.
- This is not a source bug, but it matters for packaging a clean solution.

Priority: Low.

### 19. Minor documentation and spelling issues

Files:

- `Instructions and Tests.txt`
- `SalesTax/SaleLine.cs`

Examples:

- `Tets 5` should be `Test 5`.
- `barrell` is likely meant to be `barrel`, though expected output uses `barrell`.
- `accesible` should be `accessible`.

Impact:

- Low functional impact.
- In expected-output tests, preserve supplied product spelling unless intentionally correcting documentation only.

Priority: Low.

## Suggested Fix Order

1. Initialize `saleLines`.
2. Make invalid input return `false` instead of crashing.
3. Fix parser structure: quantity, description, `at`, price.
4. Fix imported tax to add 5% instead of replacing the base tax.
5. Fix exempt product handling for all supplied examples.
6. Replace `double` money math with `decimal`.
7. Retarget the projects to a supported .NET version if allowed.
8. Add regression tests for the six supplied cases.
9. Add final `SOLUTION.md` after implementation.

## Investigation Commands Run

```powershell
dotnet build SalesTax.sln
```

Result:

- Build succeeded.
- 34 warnings were reported, mostly related to `.NET Core 2.1` being out of support and vulnerable.

```powershell
dotnet run --project SalesPrompter\SalesPrompter.csproj
```

Result:

- Runtime failed because `Microsoft.NETCore.App` version `2.1.0` is not installed.
- Installed runtime found: `.NET 8.0.15`.
