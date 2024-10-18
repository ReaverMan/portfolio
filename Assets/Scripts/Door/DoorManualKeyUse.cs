using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManualKeyUse : DoorManual
{
    public bool isExitDoor = false;
    bool isOpend = false;

    public override void Interect()
    {
        Open();
    }

    protected override bool preOpen()
    {
        return GameManager.Instance.InventoryUI.Inventory.FindItem(ItemCode.Key);
    }

    protected override void Open()
    {
        if (isExitDoor)
        {
            if (preOpen())
            {
                Debug.Log("탈출");
                //화면전환(게임메뉴)
            }
        }
        else 
        {

            if (!isOpend)
            {
                if (preOpen())
                {
                    animator.SetBool(IsOpenHash, true);
                    isOpend = true;
                }
            }
        }
    }
}
