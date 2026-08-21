using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Inventory
{
    public readonly struct InventoryResult
    {
        public bool Success { get; }

        public InventoryError Error { get; }

        public int AffectedAmount { get; }

        private InventoryResult(bool success, InventoryError error, int affectedAmount)
        {
            Success = success;
            Error = error;
            AffectedAmount = affectedAmount;
        }

        public static InventoryResult Ok(int affectedAmount)
        {
            return new InventoryResult(
                true,
                InventoryError.None,
                affectedAmount);
        }

        public static InventoryResult Fail(InventoryError error)
        {
            return new InventoryResult(
                false,
                error,
                0);
        }
    }
}
