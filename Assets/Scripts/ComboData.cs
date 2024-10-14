using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewComboData", menuName = "Combo System/Combo Data")]
public class ComboData : ScriptableObject // Ensure it's a ScriptableObject
{
    public string[] inputSequence; // The input sequence for the combo
    public string[] animationTriggers; // The animation triggers to play in sequence
    public float[] attackCooldowns; // Cooldown duration for each attack in the combo
    public Sprite abilityIcon; // Icon for the ability
}
