using UnityEngine;
namespace xn.ui
{
    internal class NvpuPlacementTool : MonoBehaviour
    {
        private static NvpuPlacementTool _inst;
        private bool _active;
        public static void BeginOneShot()
        {
            if (_inst == null)
            {
                var host = MapBox.instance != null ? MapBox.instance.gameObject : new GameObject("XN_NvpuPlacementHost");
                _inst = host.AddComponent<NvpuPlacementTool>();
                DontDestroyOnLoad(host);
            }
            _inst._active = true;
        }
        private void Update()
        {
            if (!_active) return;
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                _active = false;
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                var tile = MapBox.instance.getMouseTilePos();
                if (tile != null)
                {
                    var power = AssetManager.powers.get(xn.race.NvpuRace.POWER_ID);
                    if (power != null && power.click_action != null)
                    {
                        power.click_action(tile, power.id);
                    }
                }
                _active = false; 
            }
        }
    }
}