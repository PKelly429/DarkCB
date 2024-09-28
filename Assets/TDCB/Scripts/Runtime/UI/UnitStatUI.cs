using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TDCB
{
    public class UnitStatUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text damageTypeText;
        [SerializeField] private TMP_Text meleeArmorValue;
        [SerializeField] private TMP_Text pierceArmorValue;

        public void Bind(UnitStats stats)
        {
            damageText.text = stats.damage.ToString();
            damageTypeText.text = stats.damageType.ToString();
            meleeArmorValue.text  = stats.armor.melee.ToString();
            pierceArmorValue.text  = stats.armor.piercing.ToString();
        }
    }
}
