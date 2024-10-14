using UnityEngine;
using UnityEngine.UI;

public class AbilityIconDisplay : MonoBehaviour
{
    public Image abilityIconImage; // Reference to the UI Image component
    public ComboData comboData; // Reference to your ComboData

    private void Start()
    {
        UpdateAbilityIcon();
    }

    private void UpdateAbilityIcon()
    {
        if (comboData != null && abilityIconImage != null)
        {
            abilityIconImage.sprite = comboData.abilityIcon; // Set the icon from ComboData
        }
    }
}
