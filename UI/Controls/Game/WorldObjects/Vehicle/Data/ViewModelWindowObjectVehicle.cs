﻿using VehicleCustomNaming.UI.VehicleCustomNaming.Data;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowObjectVehicle : BaseViewModel
    {
        private readonly IClientItemsContainer cargoItemsContainer;

        private readonly VehiclePrivateState vehiclePrivateState;

        private readonly VehiclePublicState vehiclePublicState;

        private bool isVehicleTabActive;

        public ViewModelWindowObjectVehicle(
            IDynamicWorldObject vehicle,
            FrameworkElement vehicleExtraControl,
            IViewModelWithActiveState vehicleExtraControlViewModel)
        {
            this.VehicleExtraControl = vehicleExtraControl;
            this.VehicleExtraControlViewModel = vehicleExtraControlViewModel;
            if (vehicleExtraControl != null)
            {
                vehicleExtraControl.DataContext = vehicleExtraControlViewModel;
            }

            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.ContainerPlayerInventory = (IClientItemsContainer) currentCharacter.SharedGetPlayerContainerInventory();

            this.ProtoVehicle = (IProtoVehicle) vehicle.ProtoGameObject;
            this.vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            this.vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();

            var structurePointsMax = this.ProtoVehicle.SharedGetStructurePointsMax(vehicle);
            this.ViewModelStructurePoints = new ViewModelStructurePointsBarControl()
            {
                ObjectStructurePointsData = new ObjectStructurePointsData(vehicle, structurePointsMax)
            };

            this.ViewModelVehicleEnergy = new ViewModelVehicleEnergy(vehicle);

            this.cargoItemsContainer = this.vehiclePrivateState.CargoItemsContainer as IClientItemsContainer;
            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(this.cargoItemsContainer,
                callbackTakeAllItemsSuccess:
                null)
            {
                IsContainerTitleVisible = false
            };

            this.ViewModelItemsContainerExchange.IsActive = false;

            var isOwner = WorldObjectOwnersSystem.SharedIsOwner(
                ClientCurrentCharacterHelper.Character,
                vehicle);

            this.ViewModelOwnersEditor =
                new ViewModelWorldObjectOwnersEditor(this.vehiclePrivateState.Owners,
                    canEditOwners: isOwner
                                   || CreativeModeSystem.ClientIsInCreativeMode(),
                    callbackServerSetOwnersList:
                    ownersList => WorldObjectOwnersSystem.ClientSetOwners(
                        vehicle,
                        ownersList),
                    title: CoreStrings.ObjectOwnersList_Title2);

            this.ViewModelCustomNameEditor = new ViewModelWorldObjectCustomNameEditor(
                vehicle.Id,
                ProtoVehicle.Name,
                isOwner || CreativeModeSystem.ClientIsInCreativeMode());

            this.RefreshCanRepair();

            this.IsVehicleTabActive = true;
            this.ViewModelItemsContainerExchange.IsActive = true;

            if (this.cargoItemsContainer != null)
            {
                this.cargoItemsContainer.ItemAdded += this.CargoItemsContainerItemAddedHandler;
                this.cargoItemsContainer.ItemCountChanged += this.CargoItemsContainerItemCountChangedHandler;
            }
        }

        public string CannotRepairErrorMessage { get; private set; }

        public BaseCommand CommandEnterVehicle => new ActionCommand(ExecuteCommandEnterVehicle);

        public BaseCommand CommandRepair => new ActionCommand(this.ExecuteCommandRepair);

        public IClientItemsContainer ContainerPlayerInventory { get; }

        public IClientItemsContainer FuelItemsContainer
            => (IClientItemsContainer) this.vehiclePrivateState.FuelItemsContainer;

        public bool HasCargoItemsContainer => this.vehiclePrivateState.CargoItemsContainer.SlotsCount > 0;

        public bool IsCanRepair { get; private set; }

        public bool IsCargoTabActive { get; set; }

        public bool IsVehicleTabActive
        {
            get => this.isVehicleTabActive;
            set
            {
                if (this.isVehicleTabActive == value)
                {
                    return;
                }

                this.isVehicleTabActive = value;

                var wasContainerExchangeActive = this.ViewModelItemsContainerExchange.IsActive;
                this.ViewModelItemsContainerExchange.IsActive = false;

                if (this.isVehicleTabActive)
                {
                    var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
                    ClientContainersExchangeManager.Register(
                        this,
                        this.FuelItemsContainer,
                        allowedTargets: new[]
                        {
                            this.ContainerPlayerInventory,
                            currentCharacter.SharedGetPlayerContainerHotbar()
                        });
                }
                else
                {
                    ClientContainersExchangeManager.Unregister(this);
                }

                if (this.VehicleExtraControlViewModel != null)
                {
                    this.VehicleExtraControlViewModel.IsActive = value;
                }

                this.ViewModelItemsContainerExchange.IsActive = wasContainerExchangeActive;

                this.NotifyThisPropertyChanged();
            }
        }

        public IProtoVehicle ProtoVehicle { get; }

        public string RepairPercentPerStage => (100.0 / this.ProtoVehicle.RepairStagesCount).ToString("0.#");

        public uint RepairRequiredElectricityAmount => this.ProtoVehicle.RepairRequiredElectricityAmount;

        public IReadOnlyList<ProtoItemWithCount> RepairStageRequiredItems => this.ProtoVehicle.RepairStageRequiredItems;

        [ViewModelNotAutoDisposeField] public FrameworkElement VehicleExtraControl { get; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }

        public ViewModelWorldObjectCustomNameEditor ViewModelCustomNameEditor { get; }

        public ViewModelStructurePointsBarControl ViewModelStructurePoints { get; }

        public ViewModelVehicleEnergy ViewModelVehicleEnergy { get; }

        private IViewModelWithActiveState VehicleExtraControlViewModel { get; }

        protected override void DisposeViewModel()
        {
            this.IsVehicleTabActive = false;

            if (this.cargoItemsContainer != null)
            {
                this.cargoItemsContainer.ItemAdded -= this.CargoItemsContainerItemAddedHandler;
                this.cargoItemsContainer.ItemCountChanged -= this.CargoItemsContainerItemCountChangedHandler;
            }

            base.DisposeViewModel();
        }

        private static void ExecuteCommandEnterVehicle()
        {
            VehicleSystem.ClientOnVehicleEnterOrExitRequest();
        }

        private void CargoItemsContainerItemAddedHandler(IItem item)
        {
            this.IsCargoTabActive = true;
        }

        private void CargoItemsContainerItemCountChangedHandler(IItem item, ushort previousCount, ushort currentCount)
        {
            if (currentCount > previousCount)
            {
                this.IsCargoTabActive = true;
            }
        }

        private void ExecuteCommandRepair()
        {
            this.ProtoVehicle.ClientRequestRepair();
        }

        private void RefreshCanRepair()
        {
            if (this.IsDisposed)
            {
                return;
            }

            ClientTimersSystem.AddAction(delaySeconds: 0.5, this.RefreshCanRepair);

            var checkResult = this.ProtoVehicle.SharedPlayerCanRepair(ClientCurrentCharacterHelper.Character);
            this.IsCanRepair = checkResult == VehicleCanRepairCheckResult.Success;

            switch (checkResult)
            {
                case VehicleCanRepairCheckResult.Success:
                    this.CannotRepairErrorMessage = null;
                    break;

                case VehicleCanRepairCheckResult.NotEnoughPower:
                    this.CannotRepairErrorMessage = PowerGridSystem.SetPowerModeResult.NotEnoughPower.GetDescription();
                    break;

                default:
                    this.CannotRepairErrorMessage = checkResult.GetDescription();
                    break;
            }
        }
    }
}