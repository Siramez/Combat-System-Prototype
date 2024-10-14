using UnityEngine;

[CreateAssetMenu(fileName = "ComboList", menuName = "ScriptableObjects/ComboList", order = 1)]
public class ComboList : ScriptableObject
{
    public ComboData[] combos;
    public bool isMelee;
}

