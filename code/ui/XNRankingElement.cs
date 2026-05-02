using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
namespace xn.ui
{
    public class XNRankingElement : MonoBehaviour
    {
        public static XNRankingElement Prefab { get; private set; }
        public UiUnitAvatarElement avatarElement;
        public Text textName;
        public Text textRank;
        public Text textRealm;
        public Text textPower;
        public Text textLevel;
        public Text textKills;
        public Text textAge;
        public Image iconSex;
        public Image iconSpecies;
        public Image iconRealm;
        public Image iconPower;
        private Actor _actor;
        public static void CreatePrefab()
        {
            if (Prefab != null) return;
            var obj = new GameObject(nameof(XNRankingElement), typeof(Image), typeof(HorizontalLayoutGroup), typeof(Button));
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 52);
            var bgImage = obj.GetComponent<Image>();
            bgImage.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
            bgImage.type = Image.Type.Sliced;
            var horizLayout = obj.GetComponent<HorizontalLayoutGroup>();
            horizLayout.childControlWidth = false;
            horizLayout.childControlHeight = false;
            horizLayout.childForceExpandWidth = false;
            horizLayout.childForceExpandHeight = false;
            horizLayout.spacing = 4;
            horizLayout.padding = new RectOffset(2, 2, 2, 2);
            horizLayout.childAlignment = TextAnchor.MiddleLeft;
            var rankObj = new GameObject("Rank", typeof(Text));
            rankObj.transform.SetParent(obj.transform);
            rankObj.transform.localScale = Vector3.one;
            rankObj.transform.localPosition = Vector3.zero;
            var rankText = rankObj.GetComponent<Text>();
            rankText.font = LocalizedTextManager.current_font;
            rankText.fontSize = 14;
            rankText.fontStyle = FontStyle.Bold;
            rankText.color = new Color(1f, 0.84f, 0f);
            rankText.alignment = TextAnchor.MiddleCenter;
            rankText.resizeTextForBestFit = true;
            rankText.resizeTextMinSize = 10;
            rankText.resizeTextMaxSize = 14;
            rankObj.GetComponent<RectTransform>().sizeDelta = new Vector2(24, 48);
            var avatarContainer = CreateAvatarElement(obj.transform);
            var infoGroup = new GameObject("InfoGroup", typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            infoGroup.transform.SetParent(obj.transform);
            infoGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 48);
            var infoLayout = infoGroup.GetComponent<VerticalLayoutGroup>();
            infoLayout.childControlWidth = false;
            infoLayout.childControlHeight = true;
            infoLayout.childForceExpandWidth = false;
            infoLayout.childForceExpandHeight = false;
            infoLayout.spacing = 0;
            infoLayout.childAlignment = TextAnchor.UpperLeft;
            var infoFitter = infoGroup.GetComponent<ContentSizeFitter>();
            infoFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var nameObj = new GameObject("Name", typeof(Text), typeof(LayoutElement));
            nameObj.transform.SetParent(infoGroup.transform);
            nameObj.transform.localScale = Vector3.one;
            nameObj.transform.localPosition = Vector3.zero;
            var nameText = nameObj.GetComponent<Text>();
            nameText.font = LocalizedTextManager.current_font;
            nameText.fontSize = 11;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.supportRichText = true;
            nameText.resizeTextForBestFit = true;
            nameText.resizeTextMinSize = 8;
            nameText.resizeTextMaxSize = 11;
            nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 12);
            var nameLayout = nameObj.GetComponent<LayoutElement>();
            nameLayout.preferredHeight = 12;
            nameLayout.minHeight = 12;
            var realmRow = new GameObject("RealmRow", typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            realmRow.transform.SetParent(infoGroup.transform);
            realmRow.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 12);
            var realmRowLayout = realmRow.GetComponent<LayoutElement>();
            realmRowLayout.preferredHeight = 12;
            realmRowLayout.minHeight = 12;
            var realmLayout = realmRow.GetComponent<HorizontalLayoutGroup>();
            realmLayout.childControlWidth = false;
            realmLayout.childControlHeight = false;
            realmLayout.childForceExpandWidth = false;
            realmLayout.childForceExpandHeight = false;
            realmLayout.spacing = 2;
            realmLayout.childAlignment = TextAnchor.MiddleLeft;
            var realmIconObj = new GameObject("RealmIcon", typeof(Image));
            realmIconObj.transform.SetParent(realmRow.transform);
            var realmIconImg = realmIconObj.GetComponent<Image>();
            realmIconImg.preserveAspect = true;
            realmIconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            var realmTextObj = new GameObject("RealmText", typeof(Text));
            realmTextObj.transform.SetParent(realmRow.transform);
            realmTextObj.transform.localScale = Vector3.one;
            realmTextObj.transform.localPosition = Vector3.zero;
            var realmText = realmTextObj.GetComponent<Text>();
            realmText.font = LocalizedTextManager.current_font;
            realmText.fontSize = 9;
            realmText.color = new Color(0.6f, 0.9f, 1f);
            realmText.alignment = TextAnchor.MiddleLeft;
            realmText.supportRichText = true;
            realmText.resizeTextForBestFit = true;
            realmText.resizeTextMinSize = 6;
            realmText.resizeTextMaxSize = 9;
            realmTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(105, 12);
            var statsRow = new GameObject("StatsRow", typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            statsRow.transform.SetParent(infoGroup.transform);
            statsRow.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 12);
            var statsRowLayout = statsRow.GetComponent<LayoutElement>();
            statsRowLayout.preferredHeight = 12;
            statsRowLayout.minHeight = 12;
            var statsLayout = statsRow.GetComponent<HorizontalLayoutGroup>();
            statsLayout.childControlWidth = false;
            statsLayout.childControlHeight = false;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;
            statsLayout.spacing = 2;
            statsLayout.childAlignment = TextAnchor.MiddleLeft;
            var levelObj = CreateStatItem(statsRow.transform, "ui/Icons/iconLevels");
            var killsObj = CreateStatItem(statsRow.transform, "ui/Icons/iconKills");
            var ageObj = CreateStatItem(statsRow.transform, "ui/Icons/iconAge");
            var powerRow = new GameObject("PowerRow", typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            powerRow.transform.SetParent(infoGroup.transform);
            powerRow.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 12);
            var powerRowLayout = powerRow.GetComponent<LayoutElement>();
            powerRowLayout.preferredHeight = 12;
            powerRowLayout.minHeight = 12;
            var powerLayout = powerRow.GetComponent<HorizontalLayoutGroup>();
            powerLayout.childControlWidth = false;
            powerLayout.childControlHeight = false;
            powerLayout.childForceExpandWidth = false;
            powerLayout.childForceExpandHeight = false;
            powerLayout.spacing = 2;
            powerLayout.childAlignment = TextAnchor.MiddleLeft;
            var powerIconObj = new GameObject("PowerIcon", typeof(Image));
            powerIconObj.transform.SetParent(powerRow.transform);
            var powerIconImg = powerIconObj.GetComponent<Image>();
            powerIconImg.sprite = SpriteTextureLoader.getSprite("ui/icon/chartsui");
            powerIconImg.preserveAspect = true;
            powerIconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            var powerObj = new GameObject("Power", typeof(Text));
            powerObj.transform.SetParent(powerRow.transform);
            powerObj.transform.localScale = Vector3.one;
            powerObj.transform.localPosition = Vector3.zero;
            var powerText = powerObj.GetComponent<Text>();
            powerText.font = LocalizedTextManager.current_font;
            powerText.fontSize = 9;
            powerText.color = Color.white;
            powerText.alignment = TextAnchor.MiddleLeft;
            powerText.resizeTextForBestFit = true;
            powerText.resizeTextMinSize = 6;
            powerText.resizeTextMaxSize = 9;
            powerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 12);
            var sexIcon = new GameObject("SexIcon", typeof(Image));
            sexIcon.transform.SetParent(powerRow.transform);
            var sexImg = sexIcon.GetComponent<Image>();
            sexImg.preserveAspect = true;
            sexIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            var speciesIcon = new GameObject("SpeciesIcon", typeof(Image));
            speciesIcon.transform.SetParent(powerRow.transform);
            var speciesImg = speciesIcon.GetComponent<Image>();
            speciesImg.preserveAspect = true;
            speciesIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            var layoutElement = obj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 52f;
            layoutElement.preferredHeight = 52f;
            var element = obj.AddComponent<XNRankingElement>();
            element.avatarElement = avatarContainer;
            element.textName = nameText;
            element.textRank = rankText;
            element.textRealm = realmText;
            element.iconRealm = realmIconImg;
            element.textPower = powerText;
            element.iconPower = powerIconImg;
            element.textLevel = levelObj.GetComponentInChildren<Text>();
            element.textKills = killsObj.GetComponentInChildren<Text>();
            element.textAge = ageObj.GetComponentInChildren<Text>();
            element.iconSex = sexImg;
            element.iconSpecies = speciesImg;
            obj.GetComponent<Button>().targetGraphic = bgImage;
            obj.SetActive(false);
            Prefab = element;
        }
        private static UiUnitAvatarElement CreateAvatarElement(Transform parent)
        {
            var prefab = Resources.Load<UiUnitAvatarElement>("ui/UnitAvatarElement");
            if (prefab != null)
            {
                var avatar = Object.Instantiate(prefab, parent);
                avatar.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
                avatar.show_banner_kingdom = false;
                avatar.show_banner_clan = false;
                if (avatar.kingdomBanner != null)
                    avatar.kingdomBanner.gameObject.SetActive(false);
                if (avatar.clanBanner != null)
                    avatar.clanBanner.gameObject.SetActive(false);
                return avatar;
            }
            var avatarObj = new GameObject("Avatar", typeof(Image));
            avatarObj.transform.SetParent(parent);
            avatarObj.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
            avatarObj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowAvatarElement");
            return null;
        }
        private static GameObject CreateStatItem(Transform parent, string iconPath)
        {
            var itemObj = new GameObject("Stat", typeof(HorizontalLayoutGroup));
            itemObj.transform.SetParent(parent);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(34, 12);
            var itemLayout = itemObj.GetComponent<HorizontalLayoutGroup>();
            itemLayout.childControlWidth = false;
            itemLayout.childControlHeight = false;
            itemLayout.childForceExpandWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.spacing = 1;
            itemLayout.childAlignment = TextAnchor.MiddleLeft;
            var iconObj = new GameObject("Icon", typeof(Image));
            iconObj.transform.SetParent(itemObj.transform);
            iconObj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite(iconPath);
            iconObj.GetComponent<Image>().preserveAspect = true;
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            var textObj = new GameObject("Text", typeof(Text));
            textObj.transform.SetParent(itemObj.transform);
            textObj.transform.localScale = Vector3.one;
            textObj.transform.localPosition = Vector3.zero;
            var text = textObj.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 9;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 6;
            text.resizeTextMaxSize = 9;
            textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(22, 12);
            return itemObj;
        }
        public void Show(Actor actor, int rank, long power)
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
                ShowEmpty();
                return;
            }
            string rankColor = rank == 1 ? "#FFD700" : rank == 2 ? "#C0C0C0" : rank == 3 ? "#CD7F32" : "#FFFFFF";
            textRank.text = $"<color={rankColor}>{rank}</color>";
            if (avatarElement != null)
            {
                avatarElement.show(actor);
                if (avatarElement.kingdomBanner != null)
                    avatarElement.kingdomBanner.gameObject.SetActive(false);
                if (avatarElement.clanBanner != null)
                    avatarElement.clanBanner.gameObject.SetActive(false);
            }
            textName.text = actor.coloredName;
            SetRealmDisplay(actor);
            textLevel.text = actor.level.ToString();
            textKills.text = xn.access.ActorAccess.GetData(actor).kills.ToString();
            textAge.text = actor.getAge().ToString();
            textPower.text = FormatPower(power);
            if (actor.asset.inspect_sex)
            {
                iconSex.gameObject.SetActive(true);
                iconSex.sprite = actor.isSexMale()
                    ? SpriteTextureLoader.getSprite("ui/icons/IconMale")
                    : SpriteTextureLoader.getSprite("ui/icons/IconFemale");
            }
            else
            {
                iconSex.gameObject.SetActive(false);
            }
            iconSpecies.sprite = actor.asset.getSpriteIcon();
        }
        private void SetRealmDisplay(Actor actor)
        {
            if (actor == null)
            {
                iconRealm.gameObject.SetActive(false);
                textRealm.text = "";
                return;
            }
            var traits = actor.getTraits();
            if (traits == null)
            {
                iconRealm.gameObject.SetActive(false);
                textRealm.text = "";
                return;
            }
            foreach (var t in traits)
            {
                if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupAncientRealm)
                {
                    iconRealm.gameObject.SetActive(true);
                    iconRealm.sprite = t.getSprite();
                    textRealm.text = $"<color=#FF6666>{t.getTranslatedName()}</color>";
                    return;
                }
            }
            foreach (var t in traits)
            {
                if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupBeastStage)
                {
                    iconRealm.gameObject.SetActive(true);
                    iconRealm.sprite = t.getSprite();
                    textRealm.text = $"<color=#66FF66>{t.getTranslatedName()}</color>";
                    return;
                }
            }
            foreach (var t in traits)
            {
                if (t != null && t.group_id == xn.Traits.RealmTraitGroup.GroupRealm)
                {
                    iconRealm.gameObject.SetActive(true);
                    iconRealm.sprite = t.getSprite();
                    textRealm.text = $"<color=#66CCFF>{t.getTranslatedName()}</color>";
                    return;
                }
            }
            iconRealm.gameObject.SetActive(false);
            textRealm.text = "<color=#888888>无境界</color>";
        }
        private void ShowEmpty()
        {
            textRank.text = "";
            textName.text = "<color=#888888>(空)</color>";
            textRealm.text = "";
            textPower.text = "-";
            textLevel.text = "-";
            textKills.text = "-";
            textAge.text = "-";
            iconSex.gameObject.SetActive(false);
            iconRealm.gameObject.SetActive(false);
        }
        private string FormatPower(long power)
        {
            string text;
            if (power >= 1_000_000_000_000)
                text = $"{power / 1_000_000_000_000f:F1}兆";
            else if (power >= 100_000_000)
                text = $"{power / 100_000_000f:F1}亿";
            else if (power >= 10_000)
                text = $"{power / 10_000f:F1}万";
            else
                text = power.ToString();
            string color;
            if (power >= 10_000_000_000)
                color = "#FF4444";      
            else if (power >= 100_000_000)
                color = "#FF9999";      
            else if (power >= 1_000_000)
                color = "#FFD700";      
            else if (power >= 10_000)
                color = "#66FF66";      
            else
                return text;            
            return $"<color={color}>{text}</color>";
        }
        private void OnClick()
        {
            if (_actor != null && _actor.isAlive())
                ActionLibrary.openUnitWindow(_actor);
        }
        private void OnDisable() => _actor = null;
    }
}