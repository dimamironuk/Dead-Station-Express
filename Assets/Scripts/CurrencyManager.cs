using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public event Action<int> OnNutsChanged;

    [SerializeField] private int _initialNuts = 0;
    public int CurrentNuts { get; private set; }
    public int NutsBonus { get; private set; } = 0;

    private void Awake()
    {
        CurrentNuts = _initialNuts;
    }

    public void AddNuts(int amount)
    {
        if (amount <= 0) return;

        CurrentNuts += (amount + NutsBonus);
        OnNutsChanged?.Invoke(CurrentNuts);
    }

    public void AddNutsBonus(int bonusStep)
    {
        if (bonusStep <= 0) return;
        NutsBonus += bonusStep;
    }

    public bool TrySpendNuts(int amount)
    {
        if (CurrentNuts >= amount)
        {
            CurrentNuts -= amount;
            OnNutsChanged?.Invoke(CurrentNuts);
            return true;
        }

        return false;
    }

    public void LoadData(int nuts, int nutsBonus)
    {
        CurrentNuts = nuts;
        NutsBonus = nutsBonus;
        OnNutsChanged?.Invoke(CurrentNuts);
    }
}