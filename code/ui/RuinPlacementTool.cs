using UnityEngine;
namespace xn.ui
{
    internal class RuinPlacementTool : MonoBehaviour
    {
        private static RuinPlacementTool _inst;
        private bool _active;
        public static void BeginOneShot()
        {
            if (_inst == null)
            {
                var host = MapBox.instance != null ? MapBox.instance.gameObject : new GameObject("XN_RuinPlacementHost");
                _inst = host.AddComponent<RuinPlacementTool>();
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
                    xn.world.RuinBuildingAssets.InitSafe();
                    var b = xn.world.RuinBuildingAssets.PlaceAt(tile, playSfx: true);
                }
                _active = false; 
            }
        }
    }
}