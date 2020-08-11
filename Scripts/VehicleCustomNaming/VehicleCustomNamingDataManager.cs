using System.Collections.Generic;
using System.Linq;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.ServicesClient;

namespace VehicleCustomNaming.Scripts.VehicleCustomNaming
{
    public static class VehicleCustomNamingDataManager
    {
        private static readonly IClientStorage ClientStorage;

        private static Settings settingsInstance;

        private static readonly Dictionary<uint, string> RenamedVehicles;

        static VehicleCustomNamingDataManager()
        {
            ClientStorage = Api.Client.Storage.GetStorage("Mods/VehicleCustomNaming/VehicleCustomNamingDataManager");
            ClientStorage.RegisterType(typeof(Settings));
            if (ClientStorage.TryLoad(out settingsInstance))
            {
                RenamedVehicles = settingsInstance.SavedRenamedVehicles;
            }
            else
            {
                settingsInstance.SavedRenamedVehicles = RenamedVehicles = new Dictionary<uint, string>();
            }
        }

        // Do not remove this - it's for the static constructor initialization purpose only.
        public static void Init()
        {
        }

        public static string GetVehicleCustomName(uint id) => RenamedVehicles.FirstOrDefault(it => it.Key.Equals(id)).Value;

        public static void AddCustomName(uint id, string newCustomName)
        {
            if (RenamedVehicles.ContainsKey(id)) RenamedVehicles[id] = newCustomName;
            else RenamedVehicles.Add(id, newCustomName);
            settingsInstance.SavedRenamedVehicles = RenamedVehicles;
            ClientStorage.Save(settingsInstance);
        }

        public static void RemoveCustomName(uint id)
        {
            if (!RenamedVehicles.ContainsKey(id)) return;
            RenamedVehicles.Remove(id);
            settingsInstance.SavedRenamedVehicles = RenamedVehicles;
            ClientStorage.Save(settingsInstance);
        }

        private struct Settings
        {
            public Dictionary<uint, string> SavedRenamedVehicles;
        }
    }
}