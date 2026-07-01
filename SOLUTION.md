# Solution Summary

## Bugs Fixed

### Critical
1. **NullReferenceException in Sale.Add** — `saleLines` field was declared but never initialized. Fixed by initializing as `new List<SaleLine>()`.
2. **NullReferenceException on invalid input** — `Sale.Add` called `saleLines.Add(saleLine)` without checking if `InputParser.ProcessInput` returned null. Fixed by returning `false` when parser returns null.
3. **Parser rejected multi-word descriptions** — InputParser checked `wordCount > 4` and returned null, rejecting any description longer than one word. Fixed by requiring at least 4 tokens (qty, desc, "at", price) but allowing more.
4. **String.Join out-of-range** — `string.Join(" ", words, 1, wordCount)` tried to read past array end for 4-word inputs. Fixed by joining tokens between index 1 and wordCount-2 (excluding "at" and price).
5. **Import tax overwrote base tax** — `taxRate = 5` replaced the base rate instead of adding. Fixed with `taxRate += 5`.

### High
6. **Double precision in money calculations** — `CalculateTax` converted to `double` for rounding. Replaced with `decimal` math using `Math.Ceiling`.
7. **Leading zero missing on amounts < 1.00** — Format `#,###.00` produced `.85` instead of `0.85`. Changed to `0.00`.
8. **Chocolates not recognized as exempt food** — Added `"chocolate"` to exempt keywords.
9. **Parser didn't validate `at` separator** — Added check that `words[^2]` equals `"at"`.

### Medium
10. **Culture-sensitive decimal parsing** — Used `CultureInfo.InvariantCulture` for `decimal.TryParse`.
11. **Repeated spaces broke parsing** — Used `StringSplitOptions.RemoveEmptyEntries`.
12. **Negative/zero quantity or price accepted** — Rejected values ≤ 0.
13. **Case-sensitive `imported` detection** — Used case-insensitive comparison.
14. **Project targeted unsupported netcoreapp2.1** — Retargeted to `net8.0`.

## Tax and Rounding Behavior

- Tax is calculated on the **total line value** (unit price × quantity), not per unit.
- Tax is rounded **up** to the next 0.05 using `Math.Ceiling(rawTax / 0.05m) * 0.05m`.
- Books, food (chocolate, chips), and medical items (tablets) are exempt from base 10% GST.
- Imported items incur an additional 5% import tax.
- Imported exempt items pay only 5% import tax.
- Imported general items pay 15% total (10% GST + 5% import).

## Tests Added

| Test | Description | Status |
|------|-------------|--------|
| Test 1 | Basic exempt (book, chips) and taxable (music CD) | Pass |
| Test 2 | Imported exempt (chips) and imported general (transformer) | Pass |
| Test 3 | Mixed basket with imported oil, perfume, medical, chocolates | Pass |
| Test 4 | Multiple quantities with imports and exempt items | Pass |
| Test 5 | Invalid input returns false without crashing | Pass |
| Test 6 | Blank sale prints 0.00 totals | Pass |

## Framework

Retargeted from `netcoreapp2.1` to `net8.0`. Tests use xUnit.
