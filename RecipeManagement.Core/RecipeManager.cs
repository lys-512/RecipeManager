using System;
using System.Collections.Generic;

namespace RecipeManagement.Core;

/// <summary>
/// Implements the recipe manager using five nominated Part A collections:
/// Dictionary&lt;int, Recipe&gt;, List&lt;string&gt;, LinkedList&lt;int&gt;,
/// Stack&lt;int&gt; and Queue&lt;string&gt;.
/// </summary>
public sealed class RecipeManager : IRecipeManager
{
    // ── Part A 的五个指定集合（全部用 private readonly，外部不能直接碰） ──

    // 食谱数据库：按 ID 做主查找。key = 食谱 ID，value = Recipe 对象。
    private readonly Dictionary<int, Recipe> _recipes = new();

    // 购物清单：按顺序存原始食材字符串。
    private readonly List<string> _shoppingList = new();

    // 烹饪计划：按顺序存食谱 ID，支持在任意位置删除（所以用 LinkedList）。
    private readonly LinkedList<int> _cookingPlan = new();

    // 移除历史：最近从烹饪计划删掉的食谱 ID，后进先出（LIFO）。
    private readonly Stack<int> _removedHistory = new();

    // 当前食谱的烹饪指令队列：按原始顺序处理，先进先出（FIFO）。
    private readonly Queue<string> _instructionQueue = new();

    // ── 构造函数：用传入的食谱列表构建 Dictionary ──
    public RecipeManager(IEnumerable<Recipe> recipes)
    {
        // 传入 null 直接抛异常（规范要求 ArgumentNullException）
        if (recipes == null)
            throw new ArgumentNullException(nameof(recipes));

        foreach (var recipe in recipes)
        {
            // 列表里夹了 null 也不行
            if (recipe == null)
                throw new ArgumentException("Recipe entry cannot be null.", nameof(recipes));

            // ID 必须是正整数
            if (recipe.Id <= 0)
                throw new ArgumentException($"Recipe ID must be positive, but got {recipe.Id}.");

            // 标题不能是空字符串或全空格
            if (string.IsNullOrWhiteSpace(recipe.Title))
                throw new ArgumentException($"Recipe {recipe.Id} has a blank title.");

            // ID 不能重复
            if (_recipes.ContainsKey(recipe.Id))
                throw new ArgumentException($"Duplicate recipe ID: {recipe.Id}.");

            // 全部校验通过，放进字典
            _recipes[recipe.Id] = recipe;
        }
    }

    // ── 五个计数属性：直接返回各集合的 Count ──
    public int RecipeCount => _recipes.Count;
    public int ShoppingItemCount => _shoppingList.Count;
    public int CookingPlanCount => _cookingPlan.Count;
    public int PendingInstructionCount => _instructionQueue.Count;
    public int RemovedRecipeCount => _removedHistory.Count;

    // ── 食谱目录：增 / 查 / 删 ──

    // 添加新食谱。合法返回 true，非法（null/非正ID/空标题/重复）返回 false。
    public bool AddRecipe(Recipe recipe)
    {
        if (recipe == null)
            throw new ArgumentNullException(nameof(recipe));

        if (recipe.Id <= 0 ||
            string.IsNullOrWhiteSpace(recipe.Title) ||
            _recipes.ContainsKey(recipe.Id))
        {
            return false;
        }

        _recipes[recipe.Id] = recipe;
        return true;
    }

    // 按 ID 找食谱。找不到返回 null（不抛异常）。
    public Recipe? FindRecipe(int recipeId)
    {
        // TryGetValue 比 ContainsKey + 索引器省一次查找，是字典查找的标准写法
        return _recipes.TryGetValue(recipeId, out var recipe) ? recipe : null;
    }

    // 从目录删除食谱。食谱在烹饪计划里时不能删（规范要求）。
    public bool RemoveRecipe(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId))
            return false;

        // 还在烹饪计划中的食谱不允许删除
        if (_cookingPlan.Contains(recipeId))
            return false;

        _recipes.Remove(recipeId);
        return true;
    }

    // ── 下面方法在后续步骤实现 ──

    public int AddIngredientsToShoppingList(int recipeId) =>
        throw new NotImplementedException("Step 3: implement AddIngredientsToShoppingList.");

    public IReadOnlyList<string> GetShoppingList() =>
        throw new NotImplementedException("Step 3: implement GetShoppingList.");

    public void ClearShoppingList() =>
        throw new NotImplementedException("Step 3: implement ClearShoppingList.");

    public bool AddRecipeToCookingPlan(int recipeId) =>
        throw new NotImplementedException("Step 4: implement AddRecipeToCookingPlan.");

    public bool RemoveRecipeFromCookingPlan(int recipeId) =>
        throw new NotImplementedException("Step 5: implement RemoveRecipeFromCookingPlan.");

    public bool RestoreLastRemovedRecipe() =>
        throw new NotImplementedException("Step 5: implement RestoreLastRemovedRecipe.");

    public int? PeekLastRemovedRecipe() =>
        throw new NotImplementedException("Step 5: implement PeekLastRemovedRecipe.");

    public IReadOnlyList<int> GetCookingPlan() =>
        throw new NotImplementedException("Step 4: implement GetCookingPlan.");

    public bool StartCooking(int recipeId) =>
        throw new NotImplementedException("Step 6: implement StartCooking.");

    public string? PeekNextInstruction() =>
        throw new NotImplementedException("Step 6: implement PeekNextInstruction.");

    public string? CompleteNextInstruction() =>
        throw new NotImplementedException("Step 6: implement CompleteNextInstruction.");

    // ── Part B（后续作业） ──
    public IReadOnlyList<Recipe> SearchByTitle(string searchText) =>
        throw new NotImplementedException("Part B: implement SearchByTitle.");

    public IReadOnlyList<Recipe> SearchByIngredient(string searchText) =>
        throw new NotImplementedException("Part B: implement SearchByIngredient.");

    public IReadOnlyList<Recipe> GetHighestProteinRecipes(int count) =>
        throw new NotImplementedException("Part B: implement GetHighestProteinRecipes.");

    public bool AddSavedRecipe(int recipeId) =>
        throw new NotImplementedException("Part B: implement AddSavedRecipe.");

    public bool RemoveSavedRecipe(int recipeId) =>
        throw new NotImplementedException("Part B: implement RemoveSavedRecipe.");

    public bool IsRecipeSaved(int recipeId) =>
        throw new NotImplementedException("Part B: implement IsRecipeSaved.");

    public IReadOnlyList<int> GetSavedRecipes() =>
        throw new NotImplementedException("Part B: implement GetSavedRecipes.");
}
