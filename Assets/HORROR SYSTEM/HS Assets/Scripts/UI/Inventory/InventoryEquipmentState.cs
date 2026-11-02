using System;
using System.Collections.Generic;

public class InventoryEquipmentState
{
    private static EquipmentActionState equipmentActions = new()
    {
        flashlightAction = null,
        equipmentToTake = null,
        equipmentsToRemove = new()
    };

    private class EquipmentActionState
    {
        public EquipmentAction flashlightAction;
        public EquipmentAction equipmentToTake;
        public List<EquipmentAction> equipmentsToRemove;
    }

    private class EquipmentAction
    {
        public Action action;
        public ItemDatabase.Type itemType;
    }

    public static void TakeEquipment(InteractiveItem item)
    {
        var playerController = item.GetPlayer().GetComponent<PlayerController>();
        var itemType = item.GetDatabaseItem().type;

        switch (itemType)
        {
            case ItemDatabase.Type.FLASHLIGHT:
                Action flashlightAction = () =>
                {
                    playerController.SetFlashlighState(true);
                };

                equipmentActions.flashlightAction = new EquipmentAction()
                {
                    itemType = itemType,
                    action = flashlightAction
                };

                break;

            case ItemDatabase.Type.PISTOL:
                Action pistolAction = () =>
                {
                    playerController.SetPistolState(true);
                };

                equipmentActions.equipmentToTake = new EquipmentAction()
                {
                    itemType = itemType,
                    action = pistolAction
                };

                break;

            case ItemDatabase.Type.KNIFE:
                Action knifeAction = () =>
                {
                    playerController.SetKnifeState(true);
                };

                equipmentActions.equipmentToTake = new EquipmentAction()
                {
                    itemType = itemType,
                    action = knifeAction
                };

                break;
        }
    }

    public static void RemoveEquipment(InteractiveItem item)
    {
        var playerController = item.GetPlayer().GetComponent<PlayerController>();
        var itemType = item.GetDatabaseItem().type;

        switch (itemType)
        {
            case ItemDatabase.Type.FLASHLIGHT:
                Action flashlightAction = () =>
                {
                    playerController.SetFlashlighState(false);
                };

                equipmentActions.flashlightAction = new EquipmentAction()
                {
                    itemType = itemType,
                    action = flashlightAction
                };

                break;

            case ItemDatabase.Type.PISTOL:
                Action pistolAction = () =>
                {
                    playerController.SetPistolState(false);
                };

                equipmentActions.equipmentsToRemove.Add(
                    new EquipmentAction()
                    {
                        itemType = itemType,
                        action = pistolAction
                    }
                );

                break;

            case ItemDatabase.Type.KNIFE:
                Action knifeAction = () =>
                {
                    playerController.SetKnifeState(false);
                };

                equipmentActions.equipmentsToRemove.Add(
                    new EquipmentAction()
                    {
                        itemType = itemType,
                        action = knifeAction
                    }
                );

                break;
        }
    }

    public static void ExecuteActions()
    {
        DeleteEquipmentToTakeIfAlreadyRemoved();

        equipmentActions.equipmentsToRemove.ForEach(action => Execute(action));
        Execute(equipmentActions.equipmentToTake);
        Execute(equipmentActions.flashlightAction);

        ClearActions();
    }

    private static void DeleteEquipmentToTakeIfAlreadyRemoved()
    {
        if (equipmentActions.equipmentToTake == null)
        {
            return;
        }

        var equipmentToTake = equipmentActions.equipmentToTake;
        var deleteEquipmentToTake = equipmentActions.equipmentsToRemove.Exists(action =>
        {
            var itemType = action.itemType;
            if (equipmentToTake.itemType.Equals(itemType))
            {
                return true;
            }
            return false;
        });

        if (deleteEquipmentToTake)
        {
            equipmentActions.equipmentToTake = null;
        }
    }

    private static void Execute(EquipmentAction equipmentAction)
    {
        if (equipmentAction == null)
        {
            return;
        }
        equipmentAction.action();
    }

    private static void ClearActions()
    {
        equipmentActions = new()
        {
            flashlightAction = null,
            equipmentToTake = null,
            equipmentsToRemove = new()
        };
    }
}