using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
namespace xn.bloodline
{
    public static class BloodlineWindow
    {
        private static bool _inited;
        private static ScrollWindow _window;
        private static Transform _content;
        private static ObjectPoolGenericMono<BloodlineFamilyElement> _pool;
        public const string WINDOW_ID = "xn_bloodline_list";
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            _window = WindowCreator.CreateEmptyWindow(WINDOW_ID, "血脉传承");
            var bg = _window.transform.Find("Background");
            var scrollView = bg?.Find("Scroll View");
            var viewport = scrollView?.Find("Viewport");
            _content = viewport?.Find("Content");
            if (_content == null)
            {
                Debug.LogError("[BloodlineWindow] Content not found!");
                return;
            }
            SetupContentLayout();
            BloodlineFamilyElement.CreatePrefab();
            _pool = new ObjectPoolGenericMono<BloodlineFamilyElement>(BloodlineFamilyElement.Prefab, _content);
            ScrollWindow.addCallbackShow(OnWindowShow);
        }
        private static void SetupContentLayout()
        {
            var contentRect = _content.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
            }
            var layoutGroup = _content.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 4f;
            layoutGroup.padding = new RectOffset(8, 8, 8, 8);
            var sizeFitter = _content.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = _content.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        public static void Open()
        {
            if (!_inited) Init();
            ScrollWindow.showWindow(WINDOW_ID);
        }
        public static void Toggle()
        {
            if (_window != null && _window.gameObject.activeSelf)
            {
                _window.clickHide();
            }
            else
            {
                Open();
            }
        }
        private static void OnWindowShow(string windowId)
        {
            if (windowId != WINDOW_ID) return;
            RefreshList();
        }
        private static void RefreshList()
        {
            if (_content == null || _pool == null) return;
            _pool.clear();
            var families = GetBloodlineFamilies();
            int rank = 0;
            if (families != null && families.Count > 0)
            {
                foreach (var family in families)
                {
                    if (family.Founder != null && family.Founder.isAlive())
                    {
                        rank++;
                        var element = _pool.getNext();
                        element.Setup(family.Founder, rank, "始祖");
                    }
                    if (family.Chief != null && family.Chief.isAlive() && family.Chief != family.Founder)
                    {
                        rank++;
                        var element = _pool.getNext();
                        element.Setup(family.Chief, rank, "族长");
                    }
                }
            }
            if (rank == 0)
            {
                var element = _pool.getNext();
                element.SetupEmpty("暂无血脉家族");
            }
        }
        private static List<BloodlineFamily> GetBloodlineFamilies()
        {
            var families = new Dictionary<long, BloodlineFamily>();
            foreach (var actor in World.world.units)
            {
                if (actor == null || !actor.isAlive()) continue;
                if (!BloodlineSystem.HasBloodline(actor)) continue;
                long founderId = BloodlineSystem.IsFounder(actor) ? actor.getID() : BloodlineSystem.GetFounderId(actor);
                if (founderId <= 0) continue;
                if (!families.TryGetValue(founderId, out var family))
                {
                    family = new BloodlineFamily
                    {
                        FounderId = founderId,
                        BloodlineType = BloodlineSystem.GetBloodlineType(actor)
                    };
                    families[founderId] = family;
                }
                if (BloodlineSystem.IsFounder(actor))
                {
                    family.Founder = actor;
                }
                int position = BloodlineElectionSystem.GetPosition(actor);
                if (position == 1)
                {
                    family.Chief = actor;
                }
            }
            var result = new List<BloodlineFamily>(families.Values);
            result.Sort((a, b) =>
            {
                float concA = a.Founder != null ? BloodlineSystem.GetConcentration(a.Founder) : 0f;
                float concB = b.Founder != null ? BloodlineSystem.GetConcentration(b.Founder) : 0f;
                return concB.CompareTo(concA);
            });
            return result;
        }
        private class BloodlineFamily
        {
            public long FounderId;
            public string BloodlineType;
            public Actor Founder;
            public Actor Chief;
        }
    }
    public class BloodlineFamilyElement : MonoBehaviour
    {
        public static BloodlineFamilyElement Prefab { get; private set; }
        public Text rank;
        public Image frame;
        public Image icon;
        public Text title;
        public Text power;
        private Actor _actor;
        public static void CreatePrefab()
        {
            if (Prefab != null) return;
            var obj = new GameObject(nameof(BloodlineFamilyElement), typeof(Image), typeof(HorizontalLayoutGroup));
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 48);
            obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
            var horizon_layout = obj.GetComponent<HorizontalLayoutGroup>();
            horizon_layout.childControlWidth = false;
            horizon_layout.childControlHeight = false;
            horizon_layout.childForceExpandWidth = false;
            horizon_layout.childForceExpandHeight = false;
            horizon_layout.spacing = 2;
            var avatar_item = new GameObject("Avatar", typeof(Image));
            avatar_item.transform.SetParent(obj.transform);
            avatar_item.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
            avatar_item.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowAvatarElement");
            var avatar_frame = new GameObject("Background", typeof(Image));
            avatar_frame.transform.SetParent(avatar_item.transform);
            var frameRect = avatar_frame.GetComponent<RectTransform>();
            frameRect.sizeDelta = new Vector2(48, 48);
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            avatar_frame.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowAvatarElement");
            var avatar_icon = new GameObject("Icon", typeof(Image));
            avatar_icon.transform.SetParent(avatar_item.transform);
            var iconRect = avatar_icon.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(32, 32);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            var avatar_rank = new GameObject("Rank", typeof(Text));
            avatar_rank.transform.SetParent(avatar_item.transform);
            var rankText = avatar_rank.GetComponent<Text>();
            rankText.font = LocalizedTextManager.current_font;
            rankText.color = Color.white;
            rankText.alignment = TextAnchor.MiddleCenter;
            rankText.fontSize = 12;
            var rankRect = avatar_rank.GetComponent<RectTransform>();
            rankRect.sizeDelta = new Vector2(24, 24);
            rankRect.anchorMin = new Vector2(0.5f, 0.5f);
            rankRect.anchorMax = new Vector2(0.5f, 0.5f);
            rankRect.anchoredPosition = new Vector2(-32, 0);
            var vert_group = new GameObject("VertGroup", typeof(VerticalLayoutGroup));
            vert_group.transform.SetParent(obj.transform);
            var vertRect = vert_group.GetComponent<RectTransform>();
            vertRect.sizeDelta = new Vector2(130, 48);
            var vert_layout = vert_group.GetComponent<VerticalLayoutGroup>();
            vert_layout.childControlWidth = false;
            vert_layout.childControlHeight = false;
            vert_layout.childForceExpandWidth = false;
            vert_layout.childForceExpandHeight = false;
            vert_layout.spacing = 0;
            var top_part = new GameObject("TopPart", typeof(RectTransform));
            top_part.transform.SetParent(vert_group.transform);
            top_part.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 24);
            top_part.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            var name_text = new GameObject("Title", typeof(Text));
            name_text.transform.SetParent(top_part.transform);
            var titleText = name_text.GetComponent<Text>();
            titleText.font = LocalizedTextManager.current_font;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.fontSize = 12;
            titleText.supportRichText = true;
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 8;
            titleText.resizeTextMaxSize = 12;
            var nameRect = name_text.GetComponent<RectTransform>();
            nameRect.pivot = new Vector2(0, 0.5f);
            nameRect.sizeDelta = new Vector2(130, 24);
            nameRect.localPosition = Vector3.zero;
            var bottom_part = new GameObject("BottomPart", typeof(RectTransform));
            bottom_part.transform.SetParent(vert_group.transform);
            bottom_part.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 24);
            bottom_part.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            var power_text = new GameObject("Power", typeof(Text));
            power_text.transform.SetParent(bottom_part.transform);
            var powerText = power_text.GetComponent<Text>();
            powerText.font = LocalizedTextManager.current_font;
            powerText.color = Color.white;
            powerText.alignment = TextAnchor.MiddleLeft;
            powerText.fontSize = 10;
            powerText.supportRichText = true;
            powerText.resizeTextForBestFit = true;
            powerText.resizeTextMinSize = 6;
            powerText.resizeTextMaxSize = 10;
            var powerRect = power_text.GetComponent<RectTransform>();
            powerRect.pivot = new Vector2(0, 0.5f);
            powerRect.sizeDelta = new Vector2(130, 24);
            powerRect.localPosition = Vector3.zero;
            var layoutElement = obj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 48f;
            layoutElement.preferredHeight = 48f;
            var button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            var element = obj.AddComponent<BloodlineFamilyElement>();
            element.icon = avatar_icon.GetComponent<Image>();
            element.title = titleText;
            element.power = powerText;
            element.rank = rankText;
            element.frame = avatar_frame.GetComponent<Image>();
            obj.SetActive(false);
            Prefab = element;
        }
        public void Setup(Actor actor, int rankNum, string roleTitle)
        {
            gameObject.SetActive(true);
            _actor = actor;
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
            if (actor == null || !actor.isAlive())
            {
                SetupEmpty("(已故)");
                return;
            }
            rank.text = rankNum.ToString();
            switch (rankNum)
            {
                case 1:
                    frame.sprite = SpriteTextureLoader.getSprite("ui/special/windowAvatarElement_king");
                    break;
                case 2:
                    frame.sprite = SpriteTextureLoader.getSprite("ui/special/windowAvatarElement_leader");
                    break;
                default:
                    frame.sprite = SpriteTextureLoader.getSprite("ui/special/windowAvatarElement");
                    break;
            }
            icon.gameObject.SetActive(true);
            icon.sprite = xn.access.ActorAccess.GetLastColoredSprite(actor) ?? actor.asset.getSpriteIcon();
            string nameColor = roleTitle == "始祖" ? "#FF8C00" : "#FFD700";
            title.text = $"<color={nameColor}>[{roleTitle}]</color>{actor.getName()}";
            string bloodlineType = BloodlineTypes.GetLocaleName(BloodlineSystem.GetBloodlineType(actor));
            float concentration = BloodlineSystem.GetConcentration(actor);
            string concColor = concentration >= 80f ? "#FF6666" :
                               concentration >= 50f ? "#FFD700" :
                               concentration >= 20f ? "#66FF66" : "#999999";
            power.text = $"{bloodlineType} | <color={concColor}>{concentration:F1}%</color>";
        }
        public void SetupEmpty(string message)
        {
            gameObject.SetActive(true);
            _actor = null;
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
            rank.text = "";
            title.text = $"<color=#888888>{message}</color>";
            power.text = "";
            icon.gameObject.SetActive(false);
        }
        private void OnClick()
        {
            if (_actor != null && _actor.isAlive())
            {
                ActionLibrary.openUnitWindow(_actor);
            }
        }
        private void OnDisable()
        {
            _actor = null;
        }
    }
}
