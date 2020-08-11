using System.Windows;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
using VehicleCustomNaming.Scripts.VehicleCustomNaming;

namespace VehicleCustomNaming.UI.VehicleCustomNaming.Data
{
    public class ViewModelWorldObjectCustomNameEditor : BaseViewModel
    {
        public ViewModelWorldObjectCustomNameEditor(
            uint vehicleGameObjectId,
            string defaultName,
            bool canEditCustomName = true)
        {
            this.defaultName = defaultName;
            VehicleGameObjectId = vehicleGameObjectId;
            CanEditCustomName = canEditCustomName;
            var name = VehicleCustomNamingDataManager.GetVehicleCustomName(VehicleGameObjectId) ?? string.Empty;
            if (name.Length == 0) return;
            CurrentCustomName = new NameEntry(name, CommandRemoveName, GetNameEntryRemoveButtonVisibility());
            Refresh();
        }

        private readonly string defaultName;
        public uint VehicleGameObjectId { get; }
        public BaseCommand CommandSetCustomName => new ActionCommand(ExecuteCommandSetCustomName);
        public BaseCommand CommandRemoveName => new ActionCommand(ExecuteCommandRemoveName);
        public bool CanEditCustomName { get; }
        public string CustomNameField { get; set; }
        public NameEntry CurrentCustomName { get; private set; }
        public bool IsCustomNameSet => CurrentCustomName != null;

        private void ExecuteCommandSetCustomName()
        {
            var name = CustomNameField?.Trim() ?? string.Empty;
            if (name.Length == 0) return;
            CurrentCustomName = new NameEntry(name, CommandRemoveName, GetNameEntryRemoveButtonVisibility());
            VehicleCustomNamingDataManager.AddCustomName(VehicleGameObjectId, name);
            CustomNameField = string.Empty;
            Refresh();
        }

        private void ExecuteCommandRemoveName()
        {
            CurrentCustomName = null;
            VehicleCustomNamingDataManager.RemoveCustomName(VehicleGameObjectId);
            // CurrentCustomName = new NameEntry(defaultName, CommandRemoveName, GetNameEntryRemoveButtonVisibility());
            Refresh();
        }

        private Visibility GetNameEntryRemoveButtonVisibility() => Visibility.Visible;

        private void Refresh()
        {
            NotifyPropertyChanged(nameof(IsCustomNameSet));
            NotifyPropertyChanged(nameof(CurrentCustomName));
        }
    }
}