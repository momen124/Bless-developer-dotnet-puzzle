# Problem Overview

## Context

This repository contains a .NET Core console application for calculating sales tax and printing a receipt for a group of purchased items.

The candidate task is to review the existing code, identify defects, fix as many issues as possible, document the work, and return an updated Git patch file.

## Business Rules

The application should read sale lines in this format:

```text
<quantity> <description> at <unit price>
```

Example:

```text
1 book at 12.49
```

For each sale line, the program should:

1. Parse the quantity, item description, and unit price.
2. Determine whether the item is imported.
3. Determine whether the item is exempt from base sales tax.
4. Calculate tax for the line.
5. Add the tax to the line total.
6. Print a receipt containing each line, total sales tax, and final total.

## Tax Rules

The required tax rules are:

- Books, medical items, and food are exempt from the base sales tax.
- General products have 10% GST.
- Imported products have an extra 5% import tax.
- Imported general products therefore have 15% total tax.
- Sales tax is rounded per line to the nearest 5 cents.

## Expected Program Behavior

The supplied examples define the acceptance criteria:

- Valid sale lines should produce the expected receipt output.
- Invalid input should be handled gracefully.
- Blank input should produce an empty receipt with `0.00` totals.

## Important Files

- `SalesPrompter/Program.cs`: console input loop and user interaction.
- `SalesTax/InputParser.cs`: parses raw input into sale lines.
- `SalesTax/Sale.cs`: stores sale lines and totals.
- `SalesTax/SaleLine.cs`: calculates tax and line totals.
- `Instructions and Tests.txt`: original challenge description and expected outputs.

## Deliverable

The final submission should include:

- Fixed source code.
- Documentation explaining the changes.
- A Git patch file containing the solution.
