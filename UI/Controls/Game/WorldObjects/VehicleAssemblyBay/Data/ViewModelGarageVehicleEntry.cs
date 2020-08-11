using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
using AtomicTorch.GameEngine.Common.Extensions;
using VehicleCustomNaming.Scripts.VehicleCustomNaming;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay
{
    public class ViewModelGarageVehicleEntry : BaseViewModel
    {
        private VehicleStatus status;

        private string name;

        public ViewModelGarageVehicleEntry(
            uint vehicleGameObjectId,
            IProtoVehicle protoVehicle,
            VehicleStatus status)
        {
            VehicleGameObjectId = vehicleGameObjectId;
            ProtoVehicle = protoVehicle;
            Status = status;
            UpdateName();
        }

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.ProtoVehicle.Icon);

        public IProtoVehicle ProtoVehicle { get; }

        public void UpdateName()
        {
            var customName = VehicleCustomNamingDataManager.GetVehicleCustomName(VehicleGameObjectId) ?? string.Empty;
            Title = customName;
        }

        public VehicleStatus Status
        {
            get => this.status;
            set
            {
                UpdateName();
                if (status == value) return;
                status = value;
                NotifyThisPropertyChanged();
                NotifyPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText => Status.GetDescription();

        public string Title
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value.Length == 0 ? ProtoVehicle.Name : value;
                NotifyThisPropertyChanged();
            }
        }

        public uint VehicleGameObjectId { get; }
    }
}