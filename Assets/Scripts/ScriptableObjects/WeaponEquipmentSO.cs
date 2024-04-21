using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Equipment/Items/WeaponItem")]
public class WeaponEquipmentSO : EquipmentSO
{
    [Header("Weapon")]
    public WeaponType WeaponType;

    public float AttackAngle;
    public float AttackRange;
    public WeaponSkillSO WeaponSkill;

}
