# Proposed Solution Plan

This document describes the intended solution before implementation.

## Goal

Make the sales tax application produce the expected receipts, handle bad input safely, and leave the code easier to understand.

The first implementation should focus on correctness, not extra credit.

## Non-Goals For The First Pass

- Do not redesign the entire application.
- Do not add a database or product catalog.
- Do not implement the extra-credit sale-level rounding until the required examples pass.
- Do not change the console user experience more than necessary.

## Step 1: Add Regression Tests

Before adding tests, confirm the project can run on the local machine.

Current observation:

- The installed SDK can build the solution.
- The app cannot run because it targets `.NET Core 2.1`, and the installed runtime is `.NET 8`.

Planned framework decision:

- Prefer retargeting both projects to `net8.0` so the project runs on a supported .NET version.
- If the assessment expects the original target framework to remain unchanged, document that `.NET Core 2.1` runtime is required.

Create tests for the examples in `Instructions and Tests.txt`.

Required coverage:

- Test 1: basic exempt and taxable products.
- Test 2: imported food and imported general product.
- Test 3: mixed basket.
- Test 4: quantity greater than one.
- Test 5: invalid input returns an error without crashing.
- Test 6: blank sale prints `0.00` totals.

Purpose:

- Protect against accidental changes while fixing the bugs.
- Prove the final behavior matches the assignment.

## Step 2: Fix Sale Aggregation

Update `SalesTax/Sale.cs`.

Planned changes:

- Initialize the `saleLines` list.
- If parsing fails, return `false`.
- Only update totals after a valid `SaleLine` is created.
- Ensure `ToString()` works when there are no sale lines.

Expected result:

- Invalid input is handled gracefully.
- Blank receipts show zero totals.

## Step 3: Fix Input Parsing Contract

Update `SalesTax/InputParser.cs`.

Planned parsing rules:

- Reject null or whitespace input.
- Split input while ignoring repeated spaces.
- Require at least four tokens.
- Parse the first token as a positive integer quantity.
- Require the second-last token to be `at`.
- Parse the last token as a decimal unit price.
- Use the words between quantity and `at` as the product description.
- Detect `imported` as a standalone word.
- Normalize imported descriptions so receipts print `imported` at the front.

Expected result:

- Inputs from the supplied tests parse correctly.
- Bad input returns `null` instead of throwing.
- Parser behavior matches its comment contract.
- Repeated spaces do not break otherwise valid input.

## Step 4: Fix Tax Rules

Update `SalesTax/SaleLine.cs`.

Planned tax rules:

- Start tax rate at `0`.
- Add `10` for non-exempt products.
- Add `5` for imported products.
- Do not replace base tax when import tax applies.

Expected result:

- Imported general products use 15%.
- Imported exempt products use 5%.
- Local exempt products use 0%.

## Step 5: Make Product Classification Explicit Enough For The Puzzle

Update `SalesTax/SaleLine.cs`.

Planned changes:

- Avoid broad substring matches where possible.
- Cover all expected exempt products from the supplied tests:
  - books
  - chips
  - chocolates
  - headache tablets
  - iodine tablets
- Keep the approach simple unless a real product catalog is introduced.

Expected result:

- The supplied examples classify correctly.
- Obvious false positives such as `microchip` or `notebook` are less likely.

## Step 6: Fix Money Rounding And Formatting

Update `SalesTax/SaleLine.cs`.

Planned changes:

- Keep all money calculations in `decimal`.
- Round tax per line to the next 0.05 according to the challenge examples.
- Format receipt amounts with exactly two decimals.
- Use a mandatory leading zero for amounts less than `1.00`.

Expected result:

- Tax and total values match expected output.
- Floating-point precision does not affect money calculations.
- `0.85` prints as `0.85`, not `.85`.

## Step 7: Improve Readability Carefully

Possible improvements:

- Replace simple backing fields with private-set properties or readonly fields.
- Extract tax classification into small helper methods.
- Remove unused imports.
- Keep the public API small and compatible with the current console app.

Purpose:

- Make the solution easier to review.
- Avoid unnecessary redesign.

## Step 8: Document The Implemented Changes

After coding, create or update `SOLUTION.md`.

It should include:

- Summary of fixed bugs.
- Explanation of tax and rounding behavior.
- Tests added.
- Tests run.
- Any remaining assumptions.

## Step 9: Clean Generated Artifacts

Before producing the final patch:

- Confirm `bin/` and `obj/` are ignored.
- Confirm no generated build artifacts are staged.
- Confirm only source, test, and documentation changes are included.

## Step 10: Create The Final Patch

After all fixes and tests pass, generate the submission patch:

```powershell
git format-patch HEAD~1 --stdout > Bless-developer-dotnet-puzzle-solution.patch
```

If the solution uses multiple commits, generate patches from the original imported commit:

```powershell
git format-patch 244bdcb..HEAD --stdout > Bless-developer-dotnet-puzzle-solution.patch
```

The patch file is the artifact to attach in the email reply.
