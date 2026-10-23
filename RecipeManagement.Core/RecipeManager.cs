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

    // ── 购物清单（List<string>） ──

    // 把指定食谱的所有食材追加到购物清单末尾。返回添加了多少条。
    public int AddIngredientsToShoppingList(int recipeId)
    {
        var recipe = FindRecipe(recipeId);
        if (recipe == null)
            return 0;

        foreach (var ingredient in recipe.Ingredients)
        {
            _shoppingList.Add(ingredient);
        }

        return recipe.Ingredients.Count;
    }

    // 返回当前购物清单的只读视图（外部不能通过这个改内部 List）。
    public IReadOnlyList<string> GetShoppingList()
    {
        return _shoppingList.AsReadOnly();
    }

    // 清空购物清单。
    public void ClearShoppingList()
    {
        _shoppingList.Clear();
    }

    // ── 烹饪计划（LinkedList<int>） ──

    // 把食谱 ID 追加到烹饪计划末尾。食谱不存在或已在计划中则返回 false。
    public bool AddRecipeToCookingPlan(int recipeId)
    {
        if (FindRecipe(recipeId) == null)
            return false;

        if (_cookingPlan.Contains(recipeId))
            return false;

        // AddLast = 追加到链表末尾（规范要求的行为）
        _cookingPlan.AddLast(recipeId);
        return true;
    }

    // 从烹饪计划中移除食谱 ID，并把它压入移除历史栈。
    public bool RemoveRecipeFromCookingPlan(int recipeId)
    {
        if (!_cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.Remove(recipeId);
        // 成功移除后才压栈——规范说失败时不能动栈
        _removedHistory.Push(recipeId);
        return true;
    }

    // 弹出最近移除的食谱 ID，追加回烹饪计划末尾。
    public bool RestoreLastRemovedRecipe()
    {
        if (_removedHistory.Count == 0)
            return false;

        var recipeId = _removedHistory.Pop();

        // 食谱必须还存在，且还没在计划里
        if (FindRecipe(recipeId) == null || _cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    // 查看栈顶 ID 但不弹出。空栈返回 null。
    public int? PeekLastRemovedRecipe()
    {
        if (_removedHistory.Count == 0)
            return null;

        return _removedHistory.Peek();
    }

    // 按计划顺序返回所有食谱 ID 的只读列表。
    public IReadOnlyList<int> GetCookingPlan()
    {
        // 复制一份再返回，外部改不了内部 LinkedList
        return new List<int>(_cookingPlan).AsReadOnly();
    }

    // ── 烹饪指令队列（Queue<string>，FIFO） ──

    // 开始烹饪指定食谱：清空旧队列，把食谱的指令按原顺序入队。
    public bool StartCooking(int recipeId)
    {
        var recipe = FindRecipe(recipeId);
        if (recipe == null || recipe.Instructions.Count == 0)
            return false;

        // 先清空之前的指令队列
        _instructionQueue.Clear();

        // 按原始顺序逐条入队
        foreach (var instruction in recipe.Instructions)
        {
            _instructionQueue.Enqueue(instruction);
        }

        return true;
    }

    // 查看下一条指令但不取出。队列空返回 null。
    public string? PeekNextInstruction()
    {
        if (_instructionQueue.Count == 0)
            return null;

        return _instructionQueue.Peek();
    }

    // 完成下一条指令：取出并返回队首。队列空返回 null。
    public string? CompleteNextInstruction()
    {
        if (_instructionQueue.Count == 0)
            return null;

        return _instructionQueue.Dequeue();
    }

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
