using AtomicTorch.CBND.GameApi.Scripting;

namespace VehicleCustomNaming.Scripts.VehicleCustomNaming
{
    public class VehicleCustomNamingBootstrapperClient : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            VehicleCustomNamingDataManager.Init();
        }
    }
}