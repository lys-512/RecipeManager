# Design

Complete this document for **Part B**.

## 1. Saved recipe collection

Name the C# collection you selected for the saved/favourite feature and explain why it suits add, remove, duplicate prevention and membership checks.

## 2. Integration

In a short paragraph, explain how the new Part B features use the existing Part A `RecipeManager` and recipe catalogue.

## 3. Basic complexity

| Operation | Structure | Expected complexity | Reason |
| --- | --- | --- | --- |
| Lookup recipe by ID | Dictionary | Average O(1) | Hash-based key lookup. |
| Traverse cooking plan | LinkedList | O(n) | Each planned recipe may need to be visited. |
| Complete next instruction | Queue | O(1) | The item at the front is removed. |
| LINQ title/ingredient search | Recipe collection | O(n) | Each recipe may need to be inspected. |
| Check whether a recipe is saved | Your chosen collection | | Explain how your collection performs membership checks. |
