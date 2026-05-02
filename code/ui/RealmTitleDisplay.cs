using UnityEngine;
using UnityEngine.UI;
namespace xn.ui
{
    public class RealmTitleDisplay : MonoBehaviour
    {
        private RectTransform _canvasRect;
        private bool _hasCleanedUp = false; 
        public static void Init()
        {
            var canvas = Object.Instantiate(CanvasMain.instance.canvas_tooltip, CanvasMain.instance.transform);
            canvas.name = "RealmTitleDisplay";
            canvas.gameObject.AddComponent<RealmTitleDisplay>();
        }
        private void Start()
        {
            _canvasRect = GetComponent<RectTransform>();
        }
        private void Update()
        {
            if (!xn.config.ModConfigHooks.EnableTitles)
            {
                if (!_hasCleanedUp)
                {
                    CleanupAll();
                    _hasCleanedUp = true;
                }
                return;
            }
            if (_hasCleanedUp)
            {
                _hasCleanedUp = false;
            }
            var list = World.world.units.getSimpleList();
            if (list == null) return;
            foreach (var u in list)
            {
                if (u == null || !u.isAlive() || !xn.access.ActorAccess.IsVisible(u))
                {
                    CleanupOne(u);
                    continue;
                }
                var title = ExtractBracketTitle(u, u.getName());
                if (string.IsNullOrEmpty(title))
                {
                    CleanupOne(u);
                    continue;
                }
                var col = PickTierColor(u);
                CreateOrUpdateText(u, title, col);
            }
        }
        private static string ExtractBracketTitle(Actor a, string name)
        {
            if (a != null)
            {
                xn.access.ActorAccess.GetData(a).get("xn.title.current", out string storedTitle, "");
                if (!string.IsNullOrEmpty(storedTitle))
                {
                    return storedTitle;
                }
            }
            return null;
        }
        private void CreateOrUpdateText(Actor a, string text, Color col)
        {
            xn.access.ActorAccess.GetData(a).get("xn_title_obj_id", out string id, "");
            GameObject go = null;
            if (!string.IsNullOrEmpty(id))
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child != null && child.gameObject.name == id)
                    {
                        go = child.gameObject;
                        break;
                    }
                }
            }
            if (go == null)
            {
                go = new GameObject("XN_TitleText_" + xn.access.ActorAccess.GetData(a).id);
                go.transform.SetParent(transform);
                var t = go.AddComponent<Text>();
                t.font = LocalizedTextManager.current_font;
                t.alignment = TextAnchor.UpperCenter;
                t.resizeTextForBestFit = false;
                t.fontSize = 12;
                t.supportRichText = true; 
                xn.access.ActorAccess.GetData(a).set("xn_title_obj_id", go.name);
            }
            var txt = go.GetComponent<Text>();
            int realmIdx = GetRealmIndex(a);
            if (realmIdx == 15) 
            {
                txt.text = ApplyRandomColorPerChar(a, text);
                txt.color = Color.white; 
            }
            else
            {
                txt.text = text;
                txt.color = col;
            }
            var posWorld = xn.access.BaseSimObjectAccess.GetCurrentTransformPosition(a) + new Vector3(0, 2.0f, 0);
            var screen = World.world.camera.WorldToViewportPoint(posWorld);
            var lp = new Vector2(
                screen.x * _canvasRect.sizeDelta.x - _canvasRect.sizeDelta.x * 0.5f,
                screen.y * _canvasRect.sizeDelta.y - _canvasRect.sizeDelta.y * 0.5f
            );
            txt.rectTransform.localPosition = lp;
        }
        private void CleanupOne(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).get("xn_title_obj_id", out string id, "");
            if (string.IsNullOrEmpty(id)) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child != null && child.gameObject.name == id)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
            xn.access.ActorAccess.GetData(a).removeString("xn_title_obj_id");
        }
        private void CleanupAll()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != null && child.gameObject.name.StartsWith("XN_TitleText_"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            var list = World.world.units.getSimpleList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    if (a != null)
                    {
                        xn.access.ActorAccess.GetData(a).removeString("xn_title_obj_id");
                    }
                }
            }
        }
        private Color PickTierColor(Actor a)
        {
            int realm = GetRealmIndex(a);
            if (realm >= 0)
            {
                if (realm <= 3) return Color.white;
                if (realm <= 7) return Color.yellow;
                if (realm <= 11) return Color.blue;
                return Color.red;
            }
            int ancient = GetAncientStar(a);
            if (ancient > 0)
            {
                if (ancient <= 3) return Color.white;
                if (ancient <= 6) return Color.yellow;
                if (ancient <= 8) return Color.blue;
                return Color.red;
            }
            int beast = GetBeastStage(a);
            if (beast > 0)
            {
                if (beast <= 3) return Color.white;
                if (beast <= 6) return Color.yellow;
                if (beast <= 8) return Color.blue;
                return Color.red;
            }
            return Color.white;
        }
        private static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        private int GetRealmIndex(Actor a)
        {
            var ts = a.getTraits(); if (ts == null) return -1;
            int idx = -1;
            foreach (var t in ts) if (t != null)
                for (int i = 0; i < REALM_IDS.Length; i++)
                    if (t.id == REALM_IDS[i]) { if (i > idx) idx = i; }
            return idx;
        }
        private int GetAncientStar(Actor a)
        {
            var ts = a.getTraits(); if (ts == null) return 0;
            int star = 0;
            foreach (var t in ts) if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupAncientRealm)
            {
                if (t.id.Length >= 14 && int.TryParse(t.id.Substring(8, 2), out int n) && n > star) star = n;
            }
            return star;
        }
        private int GetBeastStage(Actor a)
        {
            var ts = a.getTraits(); if (ts == null) return 0;
            int st = 0;
            foreach (var t in ts) if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupBeastStage)
            {
                if (t.id.Length >= 13 && int.TryParse(t.id.Substring(6, 2), out int n) && n > st) st = n;
            }
            return st;
        }
        private string ApplyRandomColorPerChar(Actor a, string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            Color[] colors = new Color[]
            {
                Color.red,                          
                new Color(1.0f, 0.5f, 0.0f, 1.0f), 
                Color.yellow,                       
                Color.green,                        
                Color.cyan,                         
                Color.blue,                         
                new Color(0.5f, 0.0f, 0.5f, 1.0f)  
            };
            long actorId = a != null ? xn.access.ActorAccess.GetData(a).id : 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                    continue;
                }
                System.Random rng = new System.Random((int)((actorId + i) & 0x7FFFFFFF));
                int colorIndex = rng.Next(0, colors.Length);
                Color selectedColor = colors[colorIndex];
                string colorHex = ColorUtility.ToHtmlStringRGB(selectedColor);
                sb.Append("<color=#");
                sb.Append(colorHex);
                sb.Append(">");
                sb.Append(c);
                sb.Append("</color>");
            }
            return sb.ToString();
        }
    }
}
