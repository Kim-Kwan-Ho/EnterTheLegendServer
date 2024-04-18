using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Equipment/Item")]
public class EquipmentSO : ScriptableObject
{
    public int ItemId;
    public ushort StatHp;
    public ushort StatAttack;
    public ushort StatDefense;
    
}
